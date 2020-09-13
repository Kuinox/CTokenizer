using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Text.Unicode;

namespace Tokenizer
{
    public ref struct CTokenizer
    {
        SequenceReader<byte> _reader;
        readonly StringBuilder _builder;
        public CTokenizer( ReadOnlySequence<byte> sequence )
        {
            _reader = new SequenceReader<byte>( sequence );
            CurrentToken = Token.None;
            _builder = new StringBuilder();
        }
        public Token CurrentToken { get; private set; }
        public SequencePosition Position => _reader.Position;

        public bool Read()
        {
            if( !_reader.TryPeakChar( out char chr ) ) return false;
            if( char.IsWhiteSpace( chr ) )
            {
                CurrentToken = new Token( TokenType.Whitespace, chr.ToString() );
                _reader.Advance( 1 );
                return true;
            }

            if( char.IsLetter( chr ) || chr == '_' )
            {
                CurrentToken = new Token( TokenType.Word, ReadWord() );
                return true;
            }

            if( char.IsNumber( chr ) )
            {
                CurrentToken = new Token( TokenType.Number, ReadNumber() );
                return true;
            }

            switch( chr )
            {
                case ';':
                    CurrentToken = Token.Semicolon;
                    _reader.Advance( 1 );
                    return true;
                case '.':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char dotOp ) && dotOp == '=' )
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.DotAssign;
                        return true;
                    }
                    CurrentToken = Token.Dot;
                    return true;
                case '=':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char equalOp ) && equalOp == '=' )
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.Equal;
                        return true;
                    }
                    CurrentToken = Token.Assign;
                    return true;
                case '{':
                    CurrentToken = Token.CurlyOpen;
                    _reader.Advance( 1 );
                    return true;
                case '}':
                    CurrentToken = Token.CurlyClose;
                    _reader.Advance( 1 );
                    return true;
                case '(':
                    CurrentToken = Token.ParenthesesOpen;
                    _reader.Advance( 1 );
                    return true;
                case ')':
                    CurrentToken = Token.ParenthesesClose;
                    _reader.Advance( 1 );
                    return true;
                case '[':
                    CurrentToken = Token.BracketOpen;
                    _reader.Advance( 1 );
                    return true;
                case ']':
                    CurrentToken = Token.BracketClose;
                    _reader.Advance( 1 );
                    return true;
                case '<':
                    if( _reader.TryPeakChar( out char lessOP ) && lessOP == '=' )
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.LessOrEqual;
                        return true;
                    }
                    _reader.Advance( 1 );
                    CurrentToken = Token.AngleOpen;
                    return true;
                case '>':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char moreOp ) && moreOp == '=' )
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.MoreOrEqual;
                        return true;
                    }
                    CurrentToken = Token.AngleClose;
                    return true;
                case '!':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char diffOp ) && diffOp == '=' )
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.NotEqual;
                        return true;
                    }
                    CurrentToken = Token.Not;
                    return true;
                case ',':
                    _reader.Advance( 1 );
                    CurrentToken = Token.Comma;
                    return true;
                case '|':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char orOp ) )
                    {
                        if( orOp == '=' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.BitwiseOrAssign;
                            return true;
                        }
                        if(orOp == '|' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.Or;
                            return true;
                        }
                    }
                    CurrentToken = Token.BitwiseOr;
                    return true;
                case '&':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char andOp ) )
                    {
                        if( andOp == '=' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.BitwiseAnd;
                            return true;
                        }
                        if( andOp == '|' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.And;
                            return true;
                        }
                    }
                    CurrentToken = Token.BitwiseAnd;
                    return true;
                case ':':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char colonOp ) )
                    {
                        if( colonOp == '=' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.ColonAssign;
                            return true;
                        }
                        if( colonOp == ':' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.DoubleColon;
                            return true;
                        }
                    }
                    CurrentToken = Token.Colon;
                    return true;
                case '+':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char plusOp ) )
                    {
                        if( plusOp == '=' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.PlusAssign;
                            return true;
                        }
                        if( plusOp == '+' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.Increment;
                            return true;
                        }
                    }
                    CurrentToken = Token.Plus;
                    return true;
                case '-':
                    _reader.Advance( 1 );
                    CurrentToken = Token.Minus;
                    return true;
                case '@':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char nextChar ) && nextChar == '"' )
                    {
                        CurrentToken = new Token( TokenType.StringDeclaration, '@' + ReadString( (byte)'"', (byte)'"' ) );
                        return true;
                    }
                    CurrentToken = new Token( TokenType.Word, '@' + ReadWord() );
                    return true;
                case '$':
                    _reader.Advance( 1 );
                    if( _reader.TryPeakChar( out char nextChar2 ) && nextChar2 == '"' )
                    {
                        CurrentToken = new Token( TokenType.StringDeclaration, '$' + ReadString( (byte)'"', (byte)'"' ) );
                        return true;
                    }
                    break;
                case '"':
                case '\'':
                    CurrentToken = new Token( TokenType.StringDeclaration, ReadString( (byte)'\\', (byte)chr ) );
                    return true;
            }

            CurrentToken = new Token( TokenType.Unknown, chr.ToString() );
            _reader.Advance( 1 );
            return true;
        }


        string ReadWord()
        {
            bool res = ReadWordPart( out string str );
            if( res ) return str;
            _builder.Clear();
            _builder.Append( str );
            while( !res )
            {
                res = ReadWordPart( out str );
                _builder.Append( str );
            }
            return _builder.ToString();
        }
        bool ReadWordPart( out string output )
        {
            ReadOnlySpan<byte> span = _reader.UnreadSpan;
            if( span.Length == 0 )
            {
                output = string.Empty;
                return true;
            }
            for( int i = 0; i < span.Length; i++ )
            {
                byte curr = span[i];
                if( (curr & 128) > 0 ) throw new NotImplementedException( "beurk" );
                char chr = (char)curr;
                if( !char.IsLetterOrDigit( chr ) && chr != '_' )
                {
                    i++;
                    output = Encoding.UTF8.GetString( span[..i] );
                    _reader.Advance( i );
                    return true;
                }
            }
            output = Encoding.UTF8.GetString( span );
            _reader.Advance( span.Length );
            return false;
        }


        string ReadNumber()
        {
            (bool end, bool reachedDot) = ReadNumberPart( false, out string str );
            if( end ) return str;
            _builder.Clear();
            _builder.Append( str );
            while( !end )
            {
                (end, reachedDot) = ReadNumberPart( reachedDot, out str );
                _builder.Append( str );
            }
            return _builder.ToString();
        }

        (bool end, bool reachedDot) ReadNumberPart( bool allowDot, out string output )
        {
            ReadOnlySpan<byte> span = _reader.UnreadSpan;
            if( span.Length == 0 )
            {
                output = string.Empty;
                return (true, false);
            }
            for( int i = 0; i < span.Length; i++ )
            {
                byte curr = span[i];
                if( (curr & 128) > 0 ) throw new NotImplementedException( "beurk" );
                char chr = (char)curr;
                if( !char.IsDigit( chr ) && chr != '_' )
                {
                    if( chr == '.' && allowDot )
                    {
                        allowDot = false;
                    }
                    else
                    {
                        i++;
                        output = Encoding.UTF8.GetString( span[..i] );
                        _reader.Advance( i );
                        return (true, allowDot);
                    }

                }
            }
            output = Encoding.UTF8.GetString( span );
            _reader.Advance( span.Length );
            return (false, allowDot);
        }

        string ReadString( byte escapeChar, byte closeChar )
        {
            (bool escapeNextChar, bool end) = ReadCurrSpanToString( true /*the first char is the string openning*/, escapeChar, closeChar, out string str );
            if( end ) return str;
            _builder.Clear();
            _builder.Append( str );
            while( !end )
            {
                (escapeNextChar, end) = ReadCurrSpanToString( escapeNextChar, escapeChar, closeChar, out str );
                _builder.Append( str );
            }
            return _builder.ToString();
        }

        (bool escapeNextChar, bool end) ReadCurrSpanToString( bool escapeFirstchar, byte escapeChar, byte closeString, out string output )
        {
            ReadOnlySpan<byte> span = _reader.UnreadSpan;
            if( span.Length == 0 )
            {
                output = string.Empty;
                return (false, true);
            }
            for( int i = escapeFirstchar ? 1 : 0; i < span.Length; i++ )
            {
                byte curr = span[i];
                if( curr == escapeChar )
                {
                    i++;//escape next char
                    if( i == span.Length )
                    {
                        output = Encoding.UTF8.GetString( span );
                        _reader.Advance( span.Length );
                        return (true, false);
                    }
                    continue;
                }
                if( curr == closeString )
                {
                    i++;
                    output = Encoding.UTF8.GetString( span[..i] );
                    _reader.Advance( i );
                    return (false, true);
                }
            }
            output = Encoding.UTF8.GetString( span );
            _reader.Advance( span.Length );
            return (false, false);
        }
    }
}
