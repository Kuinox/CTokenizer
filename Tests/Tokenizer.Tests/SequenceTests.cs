using FluentAssertions;
using System;
using System.Buffers;
using Xunit;

namespace Tokenizer.Tests
{
    public class SequenceTests
    {
        [Fact]
        public void Test()
        {
            ReadOnlySequence<byte> buffer = new ReadOnlySequence<byte>( new byte[50] );
            var test = buffer.Slice( 1, 49 ).End;
        }
    }
}
