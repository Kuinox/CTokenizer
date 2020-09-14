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
        const string test = "$\"{Element.ToStringPath()}: Reusable '{name}' does not replace any previously registered item. Replace=\\\"True\\\" attribute should be removed.\"";
        const string test2 = "$@\"#!/bin/sh\r\n#script-dispatcher-0.0.3\r\n# Hook that execute all scripts in a directory\r\nremote=\"\"$1\"\";\r\nurl=\"\"$2\"\";\r\nhook_directory=\"\".git/hooks\"\"\r\nsearch_dir=\"\"pre-push_scripts\"\"\r\nsearch_path=\"\"$hook_directory/$search_dir\"\"\r\ni=0\r\nstdin=`cat`\r\nfor scriptFile in \"\"$search_path\"\"/*; do\r\n  i=$((i+=1))\r\n  echo \"\"Running script $scriptFile\"\";\r\n  echo \"\"$stdin\"\" | $scriptFile $@;  # execute successfully or break\r\n    # Or more explicitly: if this execution fails, then stop the `for`:\r\n   exitCode=$?\r\n   if [ $exitCode -ne 0 ] ; then\r\n   echo >&2 \"\"Script $scriptFile exit code is $exitCode. Aborting push.\"\";\r\n   exit $exitCode;\r\n   fi\r\ndone\r\necho \"\"Executed successfully $i scripts.\"\"\r\nexit 0\r\n\";";
        const string test3 = "@\"#!/bin/sh\r\n# inspired from https://github.com/bobgilmore/githooks/blob/master/pre-push\r\n# Hook stopping the push if we find a [NOPUSH] commit.\r\nremote=\"\"$1\"\"\r\nurl=\"\"$2\"\"\r\n\r\nz40=0000000000000000000000000000000000000000\r\n\r\necho \"\"Checking if a commit contain NOPUSH...\"\"\r\nwhile read local_ref local_sha remote_ref remote_sha\r\ndo\r\n\tif [ \"\"$local_sha\"\" = $z40 ]\r\n\tthen\r\n\t\techo \"\"Deleting files, OK.\"\"\r\n\telse\r\n\t\tif [ \"\"$remote_sha\"\" = $z40 ]\r\n\t\tthen\r\n\t\t\t# New branch, examine all commits\r\n\t\t\trange=\"\"$local_sha\"\"\r\n\t\telse\r\n\t\t\t# Update to existing branch, examine new commits\r\n\t\t\trange=\"\"${remote_sha}..${local_sha}\"\"\r\n\t\tfi\r\n\r\n\t\t# Check for foo commit\r\n\t\tcommit=`git rev-list -n 1 --grep 'NOPUSH' \"\"$range\"\"`\r\n    echo $commit\r\n\t\tif [ -n \"\"$commit\"\" ]\r\n\t\tthen\r\n\t\t\techo >&2 \"\"ERROR: Found commit message containing 'NOPUSH' in $local_ref so you should not push this commit !!!\"\"\r\n      echo >&2 \"\"Commit containing the message: $commit\"\"\r\n\t\t\texit 1\r\n\t\tfi\r\n\tfi\r\ndone\r\necho \"\"No commit found with NOPUSH. Push can continue.\"\"\r\nexit 0\r\n\";";
        [TestCase( "\"A regular string\"", 1 )]
        [TestCase( @"@""A """"string""""""", 1 )]
        [TestCase( @"@""A """"string"""""" @""A second""""string""""""", 3 )]
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
        [TestCase( test3, 2 )]
        [TestCase( test2, 2 )]
        [TestCase( test, 1 )]
        public void regular_string_tokenized( string theString, int count )
        {
            var res = new TokenizerTestHelper( theString );
            res.Tokens.Count.Should().Be( count );
            if( count == 1 ) res.Tokens.Single().TokenType.Should().Be( TokenType.StringDeclaration );
        }
    }
}
