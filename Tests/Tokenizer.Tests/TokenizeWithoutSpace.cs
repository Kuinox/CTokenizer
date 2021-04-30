using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tokenizer.Tests
{
    public class TokenizeWithoutSpace
    {
        [Theory]
        [InlineData( "internal/**/sealed/**/class/**/ArgumentParser", 7 )]
        public void tokens_can_be_separated_by_something_else_than_whitespace(string input, int tokenCount)
        {
            var res = new TokenizerTestHelper( input );
            res.Tokens.Count.Should().Be( tokenCount );
        }
    }
}
