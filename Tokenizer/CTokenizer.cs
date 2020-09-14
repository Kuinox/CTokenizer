using System;
using System.Buffers;
using System.IO;
using System.Text;

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
            if( _reader.TryPeek( out byte firstByte ) && firstByte == 0xEF )
            {
                _reader.Advance( 1 );
                if( !_reader.TryPeek( out firstByte ) || firstByte != 0xBB ) throw new InvalidDataException( "Invalid BOM" );
                _reader.Advance( 1 );
                if( !_reader.TryPeek( out firstByte ) || firstByte != 0xBF ) throw new InvalidDataException( "Invalid BOM" );
                _reader.Advance( 1 );
            }
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
                    {
                        CurrentToken = Token.Semicolon;
                        _reader.Advance( 1 );
                        return true;
                    }
                case '.':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) && op == '=' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.DotAssign;
                            return true;
                        }
                        CurrentToken = Token.Dot;
                        return true;
                    }
                case '=':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.Equal;
                            }

                            if( op == '>' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.Arrow;
                            }
                            return true;
                        }
                        CurrentToken = Token.Assign;
                        return true;
                    }
                case '{':
                    {
                        CurrentToken = Token.CurlyOpen;
                        _reader.Advance( 1 );
                        return true;
                    }
                case '}':
                    {
                        CurrentToken = Token.CurlyClose;
                        _reader.Advance( 1 );
                        return true;
                    }
                case '(':
                    {
                        CurrentToken = Token.ParenthesesOpen;
                        _reader.Advance( 1 );
                        return true;
                    }
                case ')':
                    {
                        CurrentToken = Token.ParenthesesClose;
                        _reader.Advance( 1 );
                        return true;
                    }
                case '[':
                    {
                        CurrentToken = Token.BracketOpen;
                        _reader.Advance( 1 );
                        return true;
                    }
                case ']':
                    {
                        CurrentToken = Token.BracketClose;
                        _reader.Advance( 1 );
                        return true;
                    }
                case '<':
                    {
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.LessOrEqual;
                                return true;
                            }
                            if( op == '<' )
                            {
                                _reader.Advance( 1 );
                                if( _reader.TryPeakChar( out char op2 ) && op2 == '=' )
                                {
                                    _reader.Advance( 1 );
                                    CurrentToken = Token.LeftShiftAssign;
                                    return true;
                                }
                                CurrentToken = Token.LeftShift;
                                return true;
                            }
                        }
                        _reader.Advance( 1 );
                        CurrentToken = Token.AngleOpen;
                        return true;
                    }
                case '>':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.MoreOrEqual;
                                return true;
                            }
                            if( op == '>' )
                            {
                                _reader.Advance( 1 );
                                if( _reader.TryPeakChar( out char op2 ) && op2 == '=' )
                                {
                                    _reader.Advance( 1 );
                                    CurrentToken = Token.RightShiftAssign;
                                    return true;
                                }
                                CurrentToken = Token.RightShift;
                                return true;
                            }
                        }
                        CurrentToken = Token.AngleClose;
                        return true;
                    }
                case '!':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.NotEqual;
                                return true;
                            }
                            if( op == '.' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.NullForgiving;
                                return true;
                            }
                        }
                        CurrentToken = Token.Not;
                        return true;
                    }
                case ',':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.Comma;
                        return true;
                    }
                case '|':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.BitwiseOrAssign;
                                return true;
                            }
                            if( op == '|' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.Or;
                                return true;
                            }
                        }
                        CurrentToken = Token.BitwiseOr;
                        return true;
                    }
                case '&':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.BitwiseAnd;
                                return true;
                            }
                            if( op == '|' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.And;
                                return true;
                            }
                        }
                        CurrentToken = Token.BitwiseAnd;
                        return true;
                    }
                case ':':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.ColonAssign;
                                return true;
                            }
                            if( op == ':' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.DoubleColon;
                                return true;
                            }
                        }
                        CurrentToken = Token.Colon;
                        return true;
                    }
                case '+':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.PlusAssign;
                                return true;
                            }
                            if( op == '+' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.Increment;
                                return true;
                            }
                        }
                        CurrentToken = Token.Plus;
                        return true;
                    }
                case '-':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.MinusAssign;
                                return true;
                            }
                            if( op == '+' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.Increment;
                                return true;
                            }
                        }
                        CurrentToken = Token.Minus;
                        return true;
                    }
                case '*':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) && op == '=' )
                        {
                            _reader.Advance( 1 );
                            CurrentToken = Token.MultiplyAssign;
                            return true;
                        }
                        CurrentToken = Token.Multiply;
                        return true;
                    }
                case '~':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.Unary;
                        return true;
                    }
                case '^':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.Xor;
                        return true;
                    }
                case '%':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = Token.Modulo;
                        return true;
                    }
                case '/':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '/' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = new Token( TokenType.Comment, "//" + ReadSingleLineComment() );
                                return true;
                            }
                            if( op == '*' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = new Token( TokenType.Comment, "/*" + ReadMultiLineComment() );
                                return true;
                            }
                            if( op == '=' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = Token.DivideAssign;
                            }
                        }
                        CurrentToken = Token.Divide;
                        return true;
                    }
                case '@':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char nextChar ) )
                        {
                            if( nextChar == '"' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = new Token( TokenType.StringDeclaration, "@\"" + ReadString( (byte)'"', (byte)'"' ) );
                                return true;
                            }
                            if( nextChar == '$' )
                            {
                                _reader.Advance( 1 );
                                if( _reader.TryPeakChar( out char char2 ) && char2 == '"' )
                                {
                                    _reader.Advance( 1 );
                                    CurrentToken = new Token( TokenType.StringDeclaration, "@$\"" + ReadString( (byte)'"', (byte)'"' ) );
                                    return true;
                                }
                                CurrentToken = new Token( TokenType.Unknown, "@$" );
                                return true;
                            }
                        }
                        CurrentToken = new Token( TokenType.Word, '@' + ReadWord() );//TODO: improve error detection: next char should be a word char.
                        return true;
                    }
                case '$':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char nextChar ) )
                        {
                            if( nextChar == '"' )
                            {
                                _reader.Advance( 1 );
                                CurrentToken = new Token( TokenType.StringDeclaration, "$\"" + ReadString( (byte)'"', (byte)'"' ) );
                                return true;
                            }
                            if( nextChar == '@' )
                            {
                                _reader.Advance( 1 );
                                if( _reader.TryPeakChar( out char char2 ) && char2 == '"' )
                                {
                                    _reader.Advance( 1 );
                                    CurrentToken = new Token( TokenType.StringDeclaration, "$@\"" + ReadString( (byte)'"', (byte)'"' ) );
                                    return true;
                                }
                                CurrentToken = new Token( TokenType.Unknown, "$@" );
                            }
                        }
                        CurrentToken = new Token( TokenType.Unknown, "$" );
                        return true;
                    }
                case '"':
                case '\'':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = new Token( TokenType.StringDeclaration, chr + ReadString( (byte)'\\', (byte)chr ) );
                        return true;
                    }
                case '`':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = new Token( TokenType.StringDeclaration, "`" + ReadString( default, (byte)chr ) );
                        return true;
                    }
                case '#':
                    {
                        _reader.Advance( 1 );
                        CurrentToken = new Token( TokenType.PreprocessorDirective, "#" + ReadSingleLineComment() );
                        return true;
                    }
                case '?':
                    {
                        _reader.Advance( 1 );
                        if( _reader.TryPeakChar( out char op ) )
                        {
                            if( op == '?' )
                            {
                                _reader.Advance( 1 );
                                if( _reader.TryPeakChar( out char op2 ) && op2 == '=' )
                                {
                                    _reader.Advance( 1 );
                                    CurrentToken = Token.NullCoalescingAssign;
                                    return true;
                                }
                                CurrentToken = Token.NullCoalescing;
                                return true;
                            }
                        }
                        CurrentToken = Token.Ternary;
                        return true;
                    }
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
                char chr = (char)curr;
                if( (!char.IsDigit( chr ) && chr != '_') )
                {
                    if( chr == '.' && allowDot )
                    {
                        allowDot = false;
                    }
                    else
                    {
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

        string ReadString( byte? escapeChar, byte closeChar )
        {
            (bool escapeNextChar, bool end) = ReadStringPart( false, escapeChar, closeChar, out string str );
            if( end ) return str;
            _builder.Clear();
            _builder.Append( str );
            while( !end )
            {
                (escapeNextChar, end) = ReadStringPart( escapeNextChar, escapeChar, closeChar, out str );
                _builder.Append( str );
            }
            return _builder.ToString();
        }

        (bool escapeNextChar, bool end) ReadStringPart( bool escapeFirstchar, byte? escapeChar, byte closeString, out string output )
        {
            bool litteralString = escapeChar.HasValue && escapeChar.Value == closeString;
            ReadOnlySpan<byte> span = _reader.UnreadSpan;
            if( span.Length == 0 )
            {
                output = string.Empty;
                return (false, true);
            }
            if( escapeFirstchar && litteralString && span[0] != closeString )
            {
                output = ((char)span[0]).ToString();
                return (false, true);
            }
            for( int i = escapeFirstchar ? 1 : 0; i < span.Length; i++ )
            {
                byte curr = span[i];
                if( escapeChar.HasValue && curr == escapeChar.Value )
                {
                    i++;//escape next char
                    if( i == span.Length )
                    {
                        output = Encoding.UTF8.GetString( span );
                        _reader.Advance( span.Length );
                        return (true, false);
                    }
                    if( litteralString && span[i] != closeString )
                    {
                        // we are at the end of a string like this: @""""
                        output = Encoding.UTF8.GetString( span[..i] );
                        _reader.Advance( i );
                        return (false, true);
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


        string ReadMultiLineComment()
        {
            (bool end, bool lastCharStar) = ReadMultiLineComment( out string str, false );
            if( end ) return str;
            _builder.Clear();
            _builder.Append( str );
            while( !end )
            {
                (end, lastCharStar) = ReadMultiLineComment( out str, lastCharStar );
                _builder.Append( str );
            }
            return _builder.ToString();
        }

        (bool end, bool lastCharStar) ReadMultiLineComment( out string output, bool lastCharStar )
        {
            ReadOnlySpan<byte> span = _reader.UnreadSpan;
            if( span.Length == 0 )
            {
                output = string.Empty;
                return (false, false);
            }
            for( int i = 0; i < span.Length; i++ )
            {
                byte curr = span[i];
                if( lastCharStar && curr == (byte)'/' )
                {
                    output = Encoding.UTF8.GetString( span[..i] );
                    _reader.Advance( i );
                    return (true, false);
                }
                lastCharStar = curr == (byte)'*';
            }
            output = Encoding.UTF8.GetString( span );
            _reader.Advance( span.Length );
            return (false, lastCharStar);
        }
        string ReadSingleLineComment()
        {
            bool end = ReadSingleLineCommentPart( out string str );
            if( end ) return str;
            _builder.Clear();
            _builder.Append( str );
            while( !end )
            {
                end = ReadSingleLineCommentPart( out str );
                _builder.Append( str );
            }
            return _builder.ToString();
        }

        bool ReadSingleLineCommentPart( out string output )
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
                if( curr == (byte)'\n' || curr == (byte)'\r' )
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
    }
}
