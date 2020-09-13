using FluentAssertions;
using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tokenizer.Tests
{
    public class TokenizeString
    {
        [TestCase( "\"A regular string\"", 1 )]
        [TestCase( @"@""A """"string""""""", 1 )]
        [TestCase( "$\"A string with {interpolation}.\"", 1 )]
        [TestCase( @"""""", 1 )]
        [TestCase( @"""""", 1 )]
        [TestCase( @"""\""""", 1 )]
        [TestCase( @"""a\""a""", 1 )]
        [TestCase( @"""bleh""", 1 )]
        [TestCase( @"""bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blebleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh blehhhhhhhhhhhhhhbleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh bleh""", 1 )]
        [TestCase( @"""'""", 1 )]
        [TestCase( @"""\\""", 1 )]
        [TestCase( @"""\a""", 1 )]
        [TestCase( @"""\b""", 1 )]
        [TestCase( @"""\t""", 1 )]
        [TestCase( @"""\n""", 1 )]
        [TestCase( @"""\v""", 1 )]
        [TestCase( @"""\f""", 1 )]
        [TestCase( @"""\r""", 1 )]
        [TestCase( @"""\0""", 1 )]
        [TestCase( @"""\xff""", 1 )]
        [TestCase( @"""\\xff""", 1 )]
        [TestCase( @"""\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n""", 1 )]
        [TestCase( @"""\\n""", 1 )]
        [TestCase( @"""\\\n""", 1 )]
        [TestCase( @"""", 1 )]
        [TestCase( @"""", 1 )]
        [TestCase( @"""\", 1 )]
        [TestCase( @"""", 1 )]
        [TestCase( @"""\\", 1 )]
        [TestCase( @"""", 1 )]
        [TestCase( @"""\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
""", 1 )]
        [TestCase( @"""", 1 )]
        [TestCase( @"""a""""b""", 2 )]
        [TestCase( @"""a"" ""b""", 3 )]
        public void regular_string_tokenized( string theString, int count )
        {
            var res = new TokenizerTestHelper( theString );
            res.Tokens.Count.Should().Be( count );
            if( count == 1 ) res.Tokens.Single().TokenType.Should().Be( TokenType.StringDeclaration );

        }
    }
}
