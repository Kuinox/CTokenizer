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
                OperationStatus token = tokenizer.Read();
                if( token != OperationStatus.Done ) break;
                _tokens.Add( tokenizer.CurrentToken );
            }
        }
        public IReadOnlyList<Token> Tokens => _tokens;
    }
}
