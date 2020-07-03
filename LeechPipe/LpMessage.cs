using System;
using System.Collections.Generic;
using System.Text;

namespace LeechPipe
{
    public class LpMessage
    {
        public ushort Pipe { get; set; }
        public byte[] Data { get; set; }
    }
}
