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

        OperationStatus NotEnoughData( TokenType tokenType )
        {
            CurrentToken = new Token( tokenType, _reader.SliceToNow( _tokenStart ) );
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
                    return PreviousCharsAsToken( TokenType.BlockClose ); //Warning: we expect that meeting this char return immediatly a valid token in interpolated string reading.
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
            switch( chr )
            {
                case '.':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' ) Advance();
                    if( chr2 == '.' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( TokenType.Operator );
                        else if( chr3 == '.' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
                case '*':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '=':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == '>' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '!':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == '.' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '&':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == '|' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case ':':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == ':' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '|':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == '|' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '+':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == '+' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '-':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' || chr2 == '-' ) Advance();
                    return PreviousCharsAsToken( TokenType.Operator );
                case '<':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' ) Advance();
                    if( chr2 == '<' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( TokenType.Operator );
                        if( chr3 == '=' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
                case '>':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '=' ) Advance();
                    if( chr2 == '>' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( TokenType.Operator );
                        if( chr3 == '=' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
                case '/':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
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
                    if( !hasNextChar ) return NotEnoughData( TokenType.Unknown );
                    if( chr2 == '"' )
                    {
                        Advance();
                        return ReadString( '"', false, '"' );
                    }
                    if( chr2 == '$' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( TokenType.Unknown );
                        if( chr3 == '"' )
                        {
                            Advance();
                            return ReadString( '"', true, '"' );
                        }
                        return PreviousCharsAsToken( TokenType.Unknown );
                    }
                    return ReadWord( true );
                case '$':
                    if( !hasNextChar ) return NotEnoughData( TokenType.Unknown );
                    if( chr2 == '"' )
                    {
                        Advance();
                        return ReadString( '"', true, '\\' );
                    }
                    if( chr2 == '@' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( TokenType.Unknown );
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
                    if( !hasNextChar ) return NotEnoughData( TokenType.Operator );
                    if( chr2 == '?' )
                    {
                        Advance();
                        if( !_reader.TryPeek( out char chr3 ) ) return NotEnoughData( TokenType.Operator );
                        if( chr3 == '=' ) Advance();
                    }
                    return PreviousCharsAsToken( TokenType.Operator );
            }
            return PreviousCharsAsToken( TokenType.Unknown );
        }


        OperationStatus ReadWord( bool startWithVerbatim )
        {
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
            return NotEnoughData( TokenType.Word );
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
            return NotEnoughData( TokenType.Number );
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
                        if( !_reader.TryPeek( out char secondEscape ) ) return NotEnoughData( TokenType.StringDeclaration );
                        if( secondEscape != escapeChar.Value ) return PreviousCharsAsToken( TokenType.StringDeclaration );
                    }
                    Advance();
                }
                else if( curr == closeStringChar )
                {
                    return PreviousCharsAsToken( TokenType.StringDeclaration );
                }
                if( interpolated && curr == '{' )
                {
                    CTokenizer nestedTokenizer = new CTokenizer( _reader.Sequence.Slice( _reader.Position, _reader.Length - _reader.Consumed ) );//TODO: UnreadSequence in .NET 5
                    while( true )
                    {
                        OperationStatus status = nestedTokenizer.Read();
                        if( status == OperationStatus.NeedMoreData ) return NotEnoughData( TokenType.Unknown );
                        Token currToken = nestedTokenizer.CurrentToken;
                        if( currToken.TokenType != TokenType.BlockClose ) continue;
                        if( currToken.Value.Length != 1 ) continue;
                        if( currToken.Value.FirstSpan[0] != '}' ) continue;
                        _reader.Advance( nestedTokenizer._reader.Consumed );
                        break;
                    }
                }
            }
            return NotEnoughData( TokenType.StringDeclaration );
        }


        OperationStatus ReadMultiLineComment()
        {
            bool lastCharIsAStar = false;
            while( _reader.TryPeek( out char curr ) )
            {
                Advance();//We advance first, because we want to take the character that complete the comment.
                if( lastCharIsAStar && curr == '/' ) return PreviousCharsAsToken( TokenType.Comment );
                lastCharIsAStar = curr == '*';
            }
            return NotEnoughData( TokenType.Comment );
        }

        OperationStatus ReadLine( TokenType tokenType )
        {
            while( _reader.TryPeek( out char curr ) )
            {
                if( curr == '\n' ) return PreviousCharsAsToken( tokenType );
                Advance();
            }
            return NotEnoughData( tokenType );
        }
    }
}
