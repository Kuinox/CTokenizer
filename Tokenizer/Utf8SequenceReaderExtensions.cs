using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer
{
    public static class Utf8SequenceReaderExtensions
    {
        

        public static ReadOnlySequence<T> SliceToNow<T>( this SequenceReader<T> reader, SequencePosition startPosition ) where T : unmanaged, IEquatable<T>
            => reader.Sequence.Slice( startPosition, reader.Position );
    }
}
