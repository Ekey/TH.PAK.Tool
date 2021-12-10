using System;

namespace TH.Unpacker
{
    class PakCipher
    {
        private static Int32 mt_state;

        private static Byte[] m_Key = new Byte[64];
        private static UInt32[] mt_magic = { 0x0, 0x9908b0df };
        private static UInt32[] mt_table = new UInt32[624];

        private static void mt_init_genrand(UInt32 dwSeed)
        {
            mt_table[0] = dwSeed & 0xffffffffU;

            for (mt_state = 1; mt_state < 624; mt_state++)
            {
                mt_table[mt_state] = (UInt32)(1812433253U * (mt_table[mt_state - 1] ^ (mt_table[mt_state - 1] >> 30)) + mt_state);
                mt_table[mt_state] &= 0xffffffffU;
            }
        }

        private static UInt32 mt_genrand_int32()
        {
            UInt32 mt_result;

            Int32 j = 0;
            if (mt_state == 624 + 1)
                mt_init_genrand(5489U);

            for (j = 0; j < 624 - 397; j++)
            {
                mt_result = (mt_table[j] & 0x80000000U) | (mt_table[j + 1] & 0x7fffffffU);
                mt_table[j] = mt_table[j + 397] ^ (mt_result >> 1) ^ mt_magic[mt_result & 0x1U];
            }

            for (; j < 624 - 1; j++)
            {
                mt_result = (mt_table[j] & 0x80000000U) | (mt_table[j + 1] & 0x7fffffffU);
                mt_table[j] = mt_table[j + (397 - 624)] ^ (mt_result >> 1) ^ mt_magic[mt_result & 0x1U];
            }

            mt_result = (mt_table[624 - 1] & 0x80000000U) | (mt_table[0] & 0x7fffffffU);
            mt_table[624 - 1] = mt_table[397 - 1] ^ (mt_result >> 1) ^ mt_magic[mt_result & 0x1U];

            mt_state = 0;

            mt_result = mt_table[mt_state++];

            mt_result ^= (mt_result >> 11);
            mt_result ^= (mt_result << 7) & 0x9d2c5680U;
            mt_result ^= (mt_result << 15) & 0xefc60000U;
            mt_result ^= (mt_result >> 18);

            return mt_result;
        }

        public static Byte[] iDecryptTable(Byte[] lpBuffer)
        {
            Int32 mti_next = 0;
            Int32 dwSize = lpBuffer.Length;

            mt_init_genrand((UInt32)dwSize);
            for (Int32 i = 0; i < dwSize; ++i, ++mti_next)
            {
                if (mti_next == 624)
                    mti_next = 0;

                if (mti_next == 0)
                    mt_genrand_int32();

                UInt32 dwTempA = mt_table[mti_next];
                UInt32 dwTempB = ((((dwTempA >> 11) ^ dwTempA) & 0xff3a58adU) << 7) ^ (dwTempA >> 11) ^ dwTempA;
                UInt32 dwTempC = ((dwTempB & 0xffffdf8cU) << 15) ^ dwTempB;

                Byte bXorByte = (Byte)(lpBuffer[i] ^ (Byte)(dwTempC >> 18));
                lpBuffer[i] = (Byte)(dwTempC ^ bXorByte);
            }

            return lpBuffer;
        }

        public static void iMakeKey(UInt32 dwSeed)
        {
            Int32 mti_next = 0;
            mt_init_genrand(dwSeed);

            for (Int32 i = 0; i < 64; i += 4, ++mti_next)
            {
                if (mti_next == 64)
                    mti_next = 0;

                if (mti_next == 0)
                    mt_genrand_int32();

                UInt32 dwTempA = mt_table[mti_next];
                UInt32 dwTempB = ((((dwTempA >> 11) ^ dwTempA) & 0xff3a58adU) << 7) ^ (dwTempA >> 11) ^ dwTempA;
                UInt32 dwTempC = ((dwTempB & 0xffffdf8cU) << 15) ^ dwTempB ^ ((((dwTempB & 0xffffdf8cU) << 15) ^ dwTempB) >> 18);

                m_Key[i + 0] = (Byte)dwTempC;
                m_Key[i + 1] = (Byte)(dwTempC >> 8);
                m_Key[i + 2] = (Byte)(dwTempC >> 16);
                m_Key[i + 3] = (Byte)(dwTempC >> 24);
            }
        }

        public static Byte[] iDecryptData(Byte[] lpBuffer, UInt32 dwSeed)
        {
            iMakeKey(dwSeed);
            for (Int32 i = 0; i < lpBuffer.Length; ++i)
            {
                lpBuffer[i] ^= m_Key[i % 64];
            }

            return lpBuffer;
        }
    }
}
