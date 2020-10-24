using System;
using System.Buffers;
using System.Collections.Generic;

namespace Tokenizer.Tests
{
    public class TokenizerTestHelper
    {
        readonly List<Token> _tokens = new List<Token>();
        public TokenizerTestHelper( string theString )
        {
            ReadOnlySequence<char> seq = new ReadOnlySequence<char>( theString.AsMemory() );
            CTokenizer tokenizer = new CTokenizer( seq );
            while( true )
            {
                OperationStatus status = tokenizer.Read();
                if( status == OperationStatus.NeedMoreData )
                {
                    if( tokenizer.CurrentToken.TokenType != TokenType.Unknown ) _tokens.Add( tokenizer.CurrentToken );
                    break;
                }
                _tokens.Add( tokenizer.CurrentToken );
            }
        }
        public IReadOnlyList<Token> Tokens => _tokens;
    }
}
