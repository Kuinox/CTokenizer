using FluentAssertions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tokenizer.Tests
{
    public class TokenizerTestHelper
    {
        List<Token> _tokens = new List<Token>();
        public TokenizerTestHelper( string theString )
        {
            byte[] buffer = Encoding.UTF8.GetBytes( theString );
            ReadOnlySequence<byte> seq = new ReadOnlySequence<byte>( buffer );
            CTokenizer tokenizer = new CTokenizer( seq );
            while( tokenizer.Read() )
            {
                _tokens.Add( tokenizer.CurrentToken );
            }
            string rebuiltString = string.Concat( _tokens.Select( s => s.ToString() ) );
            rebuiltString.Should().BeEquivalentTo( theString );
        }
        public IReadOnlyList<Token> Tokens => _tokens;
    }
}
