using System;
using System.IO;

namespace ArithmeticCoding
{
    class Decoder
    {
        private static readonly ulong Quarter = ulong.MaxValue / 4;
        private static readonly ulong Half = ulong.MaxValue / 2;

        private BitReader m_reader;

        private ulong m_low;
        private ulong m_range;
        private ulong m_length;
        private ulong[] m_byteCounts;

        public void Decode(BitReader reader, Stream writer)
        {
            if (!writer.CanWrite)
                throw new InvalidOperationException();

            m_reader = reader;

            Decode(writer);
        }

        private void Decode(Stream writer)
        {
            m_length = m_reader.ReadUInt64();

            m_byteCounts = new ulong[256];

            for (int i = 0; i < m_byteCounts.Length; i++)
            {
                m_byteCounts[i] = m_reader.ReadUInt64();
                if (i != 0)
                    m_byteCounts[i] += m_byteCounts[i - 1];
            }

            m_range = Half;
            m_low = m_reader.ReadUInt64();


            for (ulong i = 0; i < m_length; i++)
            {
                ulong rangeUnit = m_range / m_length;

                ulong target = Math.Min(m_length - 1, m_low / rangeUnit);

                byte b = 0;

                for (byte j = 0; j < m_byteCounts.Length; j++)
                {
                    if (target < m_byteCounts[j])
                    {
                        b = j;
                        break;
                    }
                }

                writer.WriteByte(b);

                FixValues(b);
            }
        }

        private void FixValues(byte b)
        {
            ulong rangeUnit = m_range / m_length;

            ulong byteCumulatedCount = m_byteCounts[b];
            ulong previousByteCumultedCount = b == 0 ? 0 : m_byteCounts[b - 1];

            m_low -= rangeUnit * previousByteCumultedCount;

            if (byteCumulatedCount < m_length)
                m_range = rangeUnit * (byteCumulatedCount - previousByteCumultedCount);
            else
                m_range -= rangeUnit * previousByteCumultedCount;

            while (m_range <= Quarter)
            {
                m_range *= 2;
                m_low *= 2;
                if (m_reader.ReadBit())
                    m_low++;
            }
        }
    }
}