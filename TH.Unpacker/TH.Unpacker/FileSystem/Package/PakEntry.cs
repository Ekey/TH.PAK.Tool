using System;

namespace TH.Unpacker
{
    class PakEntry
    {
        public String m_FileName { get; set; }
        public Int32 dwCompressedSize { get; set; }
        public Int32 dwDecompressedSize { get; set; }
        public UInt32 dwOffset { get; set; }
        public Int32 bFlag { get; set; }
        public UInt32 dwSeed { get; set; }
    }
}
