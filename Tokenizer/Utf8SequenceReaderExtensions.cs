using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer
{
    public static class Utf8SequenceReaderExtensions
    {

        public static bool TryGetChar( this SequenceReader<byte> reader, out char value )
        {
            bool ret = reader.TryPeakChar( out value );
            reader.Advance( 1 );
            return ret;
        }

        public static bool TryPeakChar( this SequenceReader<byte> reader, out char value )
        {
            bool ret = reader.TryRead( out byte val );
            if( (val & 128) > 0 ) throw new NotImplementedException( "beurk" );
            value = (char)val;
            return ret;
        }
    }
}
