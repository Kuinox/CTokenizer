using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Tokenizer
{
    public ref struct CTokenizer
    {
        SequenceReader<char> _reader;
        int _rewindCount;
        SequencePosition _tokenStart;

        public CTokenizer( ReadOnlySequence<char> sequence )
        {
            _reader = new SequenceReader<char>( sequence );
            CurrentToken = Token.None;
            _rewindCount = 0;
            _tokenStart = _reader.Position;
        }
        public bool End => _reader.End;
        public bool CustomEnd => _reader.Position.Equals( _reader.Sequence.End );
        public Token CurrentToken { get; private set; }
        public SequencePosition Position => _reader.Position;

        void Reset()
        {
            _rewindCount = 0;
            _tokenStart = _reader.Position;
        }

        OperationStatus PreviousCharsAsToken( TokenType tokenType )
        {
            CurrentToken = new Token( tokenType, _reader.SliceToNow( _tokenStart ) );
            Reset();
            return OperationStatus.Done;
        }

        OperationStatus NotEnoughData()
        {
            CurrentToken = new Token( TokenType.Unknown, _reader.SliceToNow( _tokenStart ) );
            _reader.Rewind( _rewindCount );
            Debug.Assert( _tokenStart.Equals( _reader.Position ) );
            Reset();
            return OperationStatus.NeedMoreData;
        }

        void Advance()
        {
            _reader.Advance( 1 );
            _rewindCount++;
        }

        public OperationStatus Read()
        {
            var test = _reader.Sequence.Slice( _reader.Position, _reader.Sequence.End );
            Console.WriteLine( test.Length + " " + _reader.Remaining );
            if(_reader.End)
            {

            }
            if( !_reader.TryPeek( out char chr ) )
            {
                CurrentToken = new Token( TokenType.Unknown, ReadOnlySequence<char>.Empty );
                return OperationStatus.NeedMoreData;
            }

            if( char.IsLetter( chr ) || chr == '_' ) return ReadWord( false );

            if( char.IsNumber( chr ) ) return ReadNumber( Position );
            Advance();
            if( char.IsWhiteSpace( chr ) ) return PreviousCharsAsToken( TokenType.Whitespace );
            bool hasNextChar = _reader.TryPeek( out char chr2 );
            switch( chr )
            {
                case ';':
                    return PreviousCharsAsToken( TokenType.Semicolon );
                case '{':
                    return PreviousCharsAsToken( TokenType.BlockOpen );
                case '}':
                    return PreviousCharsAsToken( TokenType.BlockClose );
                case '(':
                    return PreviousCharsAsToken( TokenType.BlockOpen );
                case ')':
                    return PreviousCharsAsToken( TokenType.BlockClose );
                case '[':
                    return PreviousCharsAsToken( TokenType.BlockOpen );
                case ']':
                    return PreviousCharsAsToken( TokenType.BlockClose );
                case '~':
                    return PreviousCharsAsToken( TokenType.Operator );
                case '^':
                    return PreviousCharsAsToken( TokenType.Operator );
                case '%':
                    return PreviousCharsAsToken( TokenType.Operator );
                case ',':
                    return PreviousCharsAsToken( TokenType.Operator );
                case '"':
                case '\'':
                    return ReadString( chr, false, '\\' );
            }
            if( !hasNextChar ) return NotEnoughData();
            switch( chr )
            {
                case '.':
                    if( chr2 == '=' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '*':
                    if( chr2 == '=' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '=':
                    if( chr2 == '=' || chr2 == '>' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '!':
                    if( chr2 == '=' || chr2 == '.' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '&':
                    if( chr2 == '=' || chr2 == '|' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case ':':
                    if( chr2 == '=' || chr2 == ':' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '|':
                    if( chr2 == '=' || chr2 == '|' ) Advance();
                    return PreviousCharsAsToken( TokenType.BlockClose );
                case '+':
                    if( chr2 == '=' || chr2 == '+' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '-':
                    if( chr2 == '=' || chr2 == '-' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '<':
                    if( chr2 == '=' ) Advance();
                    if( chr2 == '<' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData();
                        if( chr3 == '=' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
                case '>':
                    if( chr2 == '=' ) Advance();
                    if( chr2 == '>' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData();
                        if( chr3 == '=' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
                case '/':
                    if( chr2 == '/' )
                    {
                        Advance();
                        return ReadLine( TokenType.Comment );
                    }
                    if( chr2 == '*' )
                    {
                        Advance();
                        return ReadMultiLineComment();
                    }
                    if( chr2 == '=' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '@':
                    if( chr2 == '"' )
                    {
                        Advance();
                        return ReadString( '"', false, '"' );
                    }
                    if( chr2 == '$' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData();
                        if( chr3 == '"' )
                        {
                            Advance();
                            return ReadString( '"', true, '"' );
                        }
                        return PreviousCharsAsToken( TokenType.Unknown );
                    }
                    return ReadWord( true );
                case '$':
                    if( chr2 == '"' )
                    {
                        Advance();
                        return ReadString( '\\', true, '"' );
                    }
                    if( chr2 == '@' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData();
                        if( chr3 == '"' )
                        {
                            Advance();
                            return ReadString( '"', true, '"' );
                        }
                    }
                    return PreviousCharsAsToken( TokenType.Unknown );
                case '#':
                    return ReadLine( TokenType.PreprocessorDirective );
                case '?':
                    if( chr2 == '?' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData();
                        if( chr3 == '=' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
            }
            return PreviousCharsAsToken( TokenType.Unknown );
        }


        OperationStatus ReadWord( bool startWithVerbatim )
        {
            if( _reader.Remaining == 0 ) return NotEnoughData();
            while( _reader.TryPeek( out char curr ) )
            {
                if( !char.IsLetterOrDigit( curr ) && curr != '_' )
                {
                    if( startWithVerbatim ) return PreviousCharsAsToken( TokenType.Unknown );
                    return PreviousCharsAsToken( TokenType.Word );
                }
                startWithVerbatim = false;
                Advance();
            }
            return NotEnoughData();
        }


        OperationStatus ReadNumber( SequencePosition startPos )
        {
            bool reachedDot = false;
            while( _reader.TryPeek( out char curr ) )
            {
                if( char.IsDigit( curr ) || curr == '_' )
                {
                    Advance();
                    continue;
                }
                if( curr == '.' && !reachedDot )
                {
                    reachedDot = true;
                    continue;
                }
                return PreviousCharsAsToken( TokenType.Number );
            }
            return NotEnoughData();
        }

        OperationStatus ReadString( char closeStringChar, bool interpolated, char? escapeChar )
        {
            while( _reader.TryPeek( out char curr ) )
            {
                Advance();
                if( escapeChar.HasValue && curr == escapeChar.Value )
                {
                    //We met an escape char
                    if( closeStringChar == escapeChar.Value )
                    {
                        //we are on an odd escape char. strings like @"""" double the termination string to escape the double-quote.
                        if( !_reader.TryPeek( out char secondEscape ) ) return NotEnoughData();
                        if( secondEscape != escapeChar.Value ) return PreviousCharsAsToken( TokenType.StringDeclaration );
                        Advance();//it was a double quote escaped, we can continue.
                    }
                    Advance();
                }
                else if( curr == closeStringChar )
                {
                    return PreviousCharsAsToken( TokenType.StringDeclaration );
                }
            }
            return NotEnoughData();
        }


        OperationStatus ReadMultiLineComment()
        {
            bool lastCharIsAStar = false;
            while( _reader.TryPeek( out char curr ) )
            {
                Advance();//We advance first, because we want to take the character that make the 
                if( lastCharIsAStar && curr == '/' ) return PreviousCharsAsToken( TokenType.Comment );
                lastCharIsAStar = curr == '*';
            }
            return NotEnoughData();
        }

        OperationStatus ReadLine( TokenType tokenType )
        {
            while( _reader.TryPeek( out char curr ) )
            {
                if( curr == '\n' ) return PreviousCharsAsToken( tokenType );
                Advance();
            }
            return NotEnoughData();
        }
    }
}
