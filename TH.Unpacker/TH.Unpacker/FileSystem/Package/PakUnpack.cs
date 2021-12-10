using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TH.Unpacker
{
    class PakUnpack
    {
        static List<PakEntry> m_EntryTable = new List<PakEntry>();

        public static void iDoIt(String m_Archive, String m_DstFolder)
        {
            using (FileStream TPakStream = File.OpenRead(m_Archive))
            {
                var m_Header = new PakHeader();
                m_Header.dwTableSize = TPakStream.ReadInt32();

                var lpEntryTable = TPakStream.ReadBytes(m_Header.dwTableSize);
                lpEntryTable = PakCipher.iDecryptTable(lpEntryTable);

                m_EntryTable.Clear();
                using (var TEntryReader = new MemoryStream(lpEntryTable))
                {
                    TEntryReader.Position = 4;
                    do
                    {
                        String m_FileName = TEntryReader.ReadString(Encoding.GetEncoding("shift-jis")); //Japanese encoding 
                        Int32 dwCompressedSize = TEntryReader.ReadInt32();
                        Int32 dwDecompressedSize = TEntryReader.ReadInt32();
                        UInt32 dwOffset = TEntryReader.ReadUInt32();
                        Int32 bFlag = TEntryReader.ReadByte();
                        UInt32 dwSeed = TEntryReader.ReadUInt32();

                        var TEntry = new PakEntry
                        {
                            m_FileName = m_FileName,
                            dwCompressedSize = dwCompressedSize,
                            dwDecompressedSize = dwDecompressedSize,
                            dwOffset = dwOffset + (UInt32)m_Header.dwTableSize,
                            bFlag = bFlag,
                            dwSeed = dwSeed,
                        };

                        m_EntryTable.Add(TEntry);
                    }
                    while (TEntryReader.Position != m_Header.dwTableSize - 4);
                    TEntryReader.Dispose();
                }

                foreach (var m_Entry in m_EntryTable)
                {
                    String m_FullPath = m_DstFolder + m_Entry.m_FileName;

                    Utils.iSetInfo("[UNPACKING]: " + m_Entry.m_FileName);
                    Utils.iCreateDirectory(m_FullPath);

                    TPakStream.Seek(m_Entry.dwOffset, SeekOrigin.Begin);

                    var lpBuffer = TPakStream.ReadBytes(m_Entry.dwCompressedSize);
                    lpBuffer = PakCipher.iDecryptData(lpBuffer, m_Entry.dwSeed);

                    if (m_Entry.dwCompressedSize == m_Entry.dwDecompressedSize)
                    {
                        File.WriteAllBytes(m_FullPath, lpBuffer);
                    }
                    else
                    {
                        var lpDstBuffer = ZLIB.iDecompress(lpBuffer);
                        File.WriteAllBytes(m_FullPath, lpDstBuffer);
                    }
                }
            }
        }
    }
}
