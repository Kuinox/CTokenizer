using FluentAssertions;
using System.Linq;
using Xunit;

namespace Tokenizer.Tests
{
    public class TokenizeWords
    {
        [Theory]
        [InlineData( "makeParameters(", 2, new int[] { 0 } )]
        public void regular_words_tokenized( string theString, int count, int[] wordPositions )
        {
            var res = new TokenizerTestHelper( theString );
            res.Tokens.Count.Should().Be( count );
            for( int i = 0; i < res.Tokens.Count; i++ )
            {
                bool isWord = res.Tokens[i].TokenType == TokenType.Word;
                bool shouldBeWord = wordPositions.Contains( i );
                isWord.Should().Be( shouldBeWord );
            }
        }
    }
}
