using Microsoft.Toolkit.HighPerformance.Buffers;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Tokenizer
{
    public ref struct CTokenizer
    {
        //TODO: in case of not enough byte, i need to return a NotEnough bytes, and the position must be on the previous token !
        SequenceReader<char> _reader;
        readonly StringBuilder _builder;
        public CTokenizer( ReadOnlySequence<char> sequence )
        {
            _reader = new SequenceReader<char>( sequence );
            CurrentToken = Token.None;
            _builder = new StringBuilder();
        }
        public Token CurrentToken { get; private set; }
        public SequencePosition Position => _reader.Position;

        OperationStatus PreviousCharsAsToken( TokenType tokenType, SequencePosition startPosition )
        {
            CurrentToken = new Token( tokenType, _reader.SliceToNow( startPosition ) );
            return OperationStatus.Done;
        }

        OperationStatus NotEnoughData( SequencePosition startPosition, int rewind )
        {
            CurrentToken = new Token( TokenType.Unknown, _reader.SliceToNow( startPosition ) );
            _reader.Rewind( rewind );
            return OperationStatus.NeedMoreData;
        }

        public OperationStatus Read()
        {
            if( !_reader.TryPeek( out char chr ) ) return OperationStatus.Done;

            if( char.IsLetter( chr ) || chr == '_' ) return ReadWord( Position );

            if( char.IsNumber( chr ) ) return ReadNumber();

            SequencePosition startPos = Position;
            _reader.Advance( 1 );
            if( char.IsWhiteSpace( chr ) ) return PreviousCharsAsToken( TokenType.Whitespace, startPos );
            bool hasNextChar = _reader.TryPeek( out char chr2 );
            switch( chr )
            {
                case ';':
                    return PreviousCharsAsToken( TokenType.Semicolon, startPos );
                case '{':
                    return PreviousCharsAsToken( TokenType.BlockOpen, startPos );
                case '}':
                    return PreviousCharsAsToken( TokenType.BlockClose, startPos );
                case '(':
                    return PreviousCharsAsToken( TokenType.BlockOpen, startPos );
                case ')':
                    return PreviousCharsAsToken( TokenType.BlockClose, startPos );
                case '[':
                    return PreviousCharsAsToken( TokenType.BlockOpen, startPos );
                case ']':
                    return PreviousCharsAsToken( TokenType.BlockClose, startPos );
                case '~':
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '^':
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '%':
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case ',':
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '"':
                case '\'':
                    return ReadString( startPos, chr, false, '\\' );
            }
            if( !hasNextChar ) return NotEnoughData( startPos, 1 );
            switch( chr )
            {
                case '.':
                    if( chr2 == '=' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '*':
                    if( chr2 == '=' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '=':
                    if( chr2 == '=' || chr2 == '>' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '!':
                    if( chr2 == '=' || chr2 == '.' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '&':
                    if( chr2 == '=' || chr2 == '|' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case ':':
                    if( chr2 == '=' || chr2 == ':' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '|':
                    if( chr2 == '=' || chr2 == '|' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.BlockClose, startPos );
                case '+':
                    if( chr2 == '=' || chr2 == '+' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '-':
                    if( chr2 == '=' || chr2 == '-' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '<':
                    if( chr2 == '=' ) _reader.Advance( 1 );
                    if( chr2 == '<' )
                    {
                        _reader.Advance( 1 );
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( startPos, 2 );
                        if( chr3 == '=' ) _reader.Advance( 1 );
                    }
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '>':
                    if( chr2 == '=' ) _reader.Advance( 1 );
                    if( chr2 == '>' )
                    {
                        _reader.Advance( 1 );
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( startPos, 2 );
                        if( chr3 == '=' ) _reader.Advance( 1 );
                    }
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '/':
                    if( chr2 == '/' )
                    {
                        _reader.Advance( 1 );
                        return ReadSingleLineComment( startPos );
                    }
                    if( chr2 == '*' )
                    {
                        _reader.Advance( 1 );
                        return ReadMultiLineComment( startPos );
                    }
                    if( chr2 == '=' ) _reader.Advance( 1 );
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
                case '@':
                    if( chr2 == '"' )
                    {
                        _reader.Advance( 1 );
                        return ReadString( startPos, '"', false, '"' );
                    }
                    if( chr2 == '$' )
                    {
                        _reader.Advance( 1 );
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( startPos, 2 );
                        if( chr3 == '"' )
                        {
                            _reader.Advance( 1 );
                            return ReadString( startPos, '"', true, '"' );
                        }
                        return PreviousCharsAsToken( TokenType.Unknown, startPos );
                    }
                    return ReadWord( startPos );
                case '$':
                    if( chr2 == '"' )
                    {
                        _reader.Advance( 1 );
                        return ReadString( startPos, '\\', true, '"' );
                    }
                    if( chr2 == '@' )
                    {
                        _reader.Advance( 1 );
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( startPos, 2 );
                        if( chr3 == '"' )
                        {
                            _reader.Advance( 1 );
                            return ReadString( startPos, '"', true, '"' );
                        }
                    }
                    return PreviousCharsAsToken( TokenType.Unknown, startPos );
                case '#':
                    return PreprocessorDirective( startPos );
                case '?':
                    if( chr2 == '?' )
                    {
                        _reader.Advance( 1 );
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( startPos, 2 );
                        if( chr3 == '=' ) _reader.Advance( 1 );
                    }
                    return PreviousCharsAsToken( TokenType.Operator, startPos );
            }
            return PreviousCharsAsToken( TokenType.Unknown, startPos );
        }


        OperationStatus ReadWord( SequencePosition startPos )
        {
            Debug.Assert( _reader.Remaining > 0 );
            SequencePosition beginning = _reader.Position;
            while( _reader.TryPeek( out char curr ) )
            {
                if( !char.IsLetterOrDigit( curr ) && curr != '_' ) break;
                _reader.Advance( 1 );
            }
            SequencePosition end = _reader.Position;
            Debug.Assert( !beginning.Equals( end ) );
            CurrentToken = new Token( TokenType.Word, _reader.Sequence.Slice( beginning, end ) );
            return true;
        }


        OperationStatus ReadNumber()
        {
            bool reachedDot = false;
            SequencePosition beginning = _reader.Position;
            while( _reader.TryPeek( out char curr ) )
            {
                if( (!char.IsDigit( curr ) && curr != '_') )
                {
                    if( curr == '.' && !reachedDot )
                    {
                        reachedDot = true;
                    }
                    else
                    {
                        break;
                    }
                }
                _reader.Advance( 1 );
            }
            SequencePosition end = _reader.Position;
            Debug.Assert( !beginning.Equals( end ) );
            CurrentToken = new Token( TokenType.Number, _reader.Sequence.Slice( beginning, end ) );
            return true;
        }

        OperationStatus ReadString( SequencePosition startPosition, char closeStringChar, bool interpolated, char? escapeChar )
        {
            ReadOnlySequence<byte> remainingSequence = _reader.Sequence.Slice( _reader.Position );
            SequenceReader<byte> reader = new SequenceReader<byte>( remainingSequence );
            while( reader.TryPeek( out char curr ) )
            {
                reader.Advance( 1 );
                if( escapeChar.HasValue && curr == escapeChar.Value )
                {
                    if( closeStringChar == escapeChar.Value )
                    {
                        //we are on an escape char. strings like @"""" double the termination string to escape the double-quote.
                        if( !reader.TryPeek( out char secondEscape ) )
                        {
                            CurrentToken = new Token( TokenType.StringDeclaration, _reader.SliceToNow( startPosition ) );
                            return true;
                        }
                        if( secondEscape == escapeChar.Value )
                        {
                            reader.Advance( 1 );//it was a double quote escaped, we can continue.
                            continue;
                        }
                        CurrentToken = new Token( TokenType.StringDeclaration, _reader.SliceToNow( startPosition ) );
                        return true;
                    }
                    continue;
                }
                if( curr == closeStringChar )
                {
                    CurrentToken = new Token( TokenType.StringDeclaration, _reader.SliceToNow( startPosition ) );
                    return true;
                }
            }
            CurrentToken = new Token( TokenType.Unknown, _reader.SliceToNow( startPosition ) );
            return true;
            for( int i = 0; i < span.Length; i++ )
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
                    if( isEscapeCharCloseChar && span[i] != closeStringChar )
                    {
                        // we are at the end of a string like this: @""""
                        output = Encoding.UTF8.GetString( span[..i] );
                        _reader.Advance( i );
                        return (false, true);
                    }
                    continue;
                }
                if( curr == closeStringChar )
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


        OperationStatus ReadMultiLineComment( SequencePosition startPosition )
        {
            bool lastCharIsAStar = false;
            while( _reader.TryPeek( out char curr ) )
            {
                _reader.Advance( 1 );//We advance first, because we want to take the character that make the 
                if( lastCharIsAStar && curr == '/' )
                {
                    CurrentToken = new Token( TokenType.Comment, _reader.SliceToNow( startPosition ) );
                    return true;
                }
                lastCharIsAStar = curr == '*';
            }
            CurrentToken = new Token( TokenType.Unknown, _reader.SliceToNow( startPosition ) );
            return true;
        }


        OperationStatus ReadSingleLineComment( SequencePosition startPosition )
        {
            while( _reader.TryPeek( out char curr ) )
            {
                if( curr == '\n' ) break;
                _reader.Advance( 1 );
            }
            return new Token( TokenType.Comment, _reader.SliceToNow( startPosition ) );
        }

        OperationStatus PreprocessorDirective( SequencePosition startPosition )
    }
}
