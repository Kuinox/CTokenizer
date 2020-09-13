using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Tokenizer
{
    class Program
    {
        static async Task Main( string[] args )
        {
            FileStream stream = File.OpenRead( @"C:\dev\Tokenizer\Tokenizer\CTokenizer.cs" );
            PipeReader pipe = PipeReader.Create( stream );
            var result = await pipe.ReadAsync();
            while( !result.IsCompleted )
            {
                pipe.AdvanceTo( result.Buffer.Start, result.Buffer.End );
                result = await pipe.ReadAsync();
            }
            ParseSelf( result.Buffer );
        }

        static void ParseSelf( ReadOnlySequence<byte> content )
        {
            var c = new CTokenizer( content );
            while( c.Read() )
            {
                if(c.CurrentToken.TokenType == TokenType.Unknown)
                //if(c.CurrentToken.TokenType != TokenType.Whitespace)
                //if( c.CurrentToken.TokenType == TokenType.Number )
                {
                    Console.WriteLine( c.CurrentToken.TokenType + " '" + c.CurrentToken.Value + "'" );
                }
            }
        }
    }
}
