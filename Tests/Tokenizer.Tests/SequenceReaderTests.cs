using FluentAssertions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tokenizer.Tests
{
    public class SequenceReaderTests
    {
        [Fact]
        public void position_should_be_same_after_advance_then_rewind()
        {
            ReadOnlySequence<byte> sequence = new ReadOnlySequence<byte>( new byte[1] );
            SequenceReader<byte> reader = new SequenceReader<byte>();
            reader.Position.Should().Be( sequence.Start );
        }
    }
}
