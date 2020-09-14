using System;
using System.Buffers;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Tokenizer
{
    class Program
    {
        static async Task Main( string[] args )
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var files = Directory.GetFiles( @"C:\dev\", "*.cs", SearchOption.AllDirectories );
            for( int i = 0; i < files.Length; i++ )
            {
                await ParseFile( files[i] );
            }
            stopwatch.Stop();
            Console.WriteLine( $"Processed {files.Length} files in {stopwatch.Elapsed}" );
            Console.WriteLine( $"Or {stopwatch.Elapsed / files.Length} per file." );
        }

        static async Task ParseFile( string filePath )
        {
            using( FileStream stream = File.OpenRead( filePath ) )
            {
                PipeReader pipe = PipeReader.Create( stream );
                var result = await pipe.ReadAsync();
                while( !result.IsCompleted )
                {
                    pipe.AdvanceTo( result.Buffer.Start, result.Buffer.End );
                    result = await pipe.ReadAsync();
                }
                ParseSelf( filePath, result.Buffer );   
            }
        }

        static void ParseSelf( string filePath, ReadOnlySequence<byte> content )
        {
            var c = new CTokenizer( content );
            while( c.Read() )
            {
                if( c.CurrentToken.TokenType == TokenType.Unknown )
                //if(c.CurrentToken.TokenType != TokenType.Whitespace)
                //if( c.CurrentToken.TokenType == TokenType.Number )
                {
                    Console.WriteLine( filePath + " " + c.CurrentToken.TokenType + " " + ToLiteral( c.CurrentToken.Value ) );
                }
            }
        }

        static string ToLiteral( string input )
        {
            using( var writer = new StringWriter() )
            {
                using( var provider = CodeDomProvider.CreateProvider( "CSharp" ) )
                {
                    provider.GenerateCodeFromExpression( new CodePrimitiveExpression( input ), writer, null );
                    return writer.ToString();
                }
            }
        }
    }
}
