using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tokenizer.Tests
{
    public class TokenizeWords
    {
        [Theory]
        [InlineData( "makeParameters(", 2)]
        public void regular_words_tokenized( string theString, int count )
        {
            var res = new TokenizerTestHelper( theString );
            res.Tokens.Count.Should().Be( count );
            if( count == 1 ) res.Tokens.Single().TokenType.Should().Be( TokenType.Word );
        }
    }
}
