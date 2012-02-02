using System;
using System.IO;

namespace ArithmeticCoding
{
    class Coder
    {
        private static readonly ulong Quarter = ulong.MaxValue / 4;
        private static readonly ulong Half = ulong.MaxValue / 2;

        private BitWriter m_writer;

        private ulong m_low;
        private ulong m_range;
        private ulong m_counter;
        private ulong m_length;
        private ulong[] m_byteCounts;

        public void Encode(Stream reader, BitWriter writer)
        {
            if (!reader.CanRead || !reader.CanSeek)
                throw new InvalidOperationException();

            m_writer = writer;

            Encode(reader);
        }

        private void Encode(Stream reader)
        {
            m_byteCounts = new ulong[256];

            int b;
            m_length = 0;

            while ((b = reader.ReadByte()) != -1)
            {
                m_byteCounts[b]++;
                m_length++;
            }

            m_writer.Write(m_length);

            for (int i = 0; i < m_byteCounts.Length; i++)
            {
                m_writer.Write(m_byteCounts[i]);

                // to compute cumulated counts
                if (i != 0)
                    m_byteCounts[i] += m_byteCounts[i - 1];
            }

            m_low = 0;
            m_range = Half;
            m_counter = 0;

            reader.Position = 0;

            while ((b = reader.ReadByte()) != -1)
            {
                EncodeByte((byte)b);
            }

            for (int i = 0; i < 64; i++)
            {
                bool bit = ((1UL << i) & m_low) == 1;
                OutputBit(bit);
            }
        }

        private void EncodeByte(byte b)
        {
            ulong rangeUnit = m_range / m_length;

            ulong byteCumulatedCount = m_byteCounts[b];
            ulong previousByteCumultedCount = b == 0 ? 0 : m_byteCounts[b - 1];

            m_low += rangeUnit * previousByteCumultedCount;

            if (byteCumulatedCount != m_length)
                m_range = rangeUnit * (byteCumulatedCount - previousByteCumultedCount);
            else
                m_range = m_range - rangeUnit * previousByteCumultedCount;

            while (m_range < Quarter)
            {
                if (m_low + m_range <= Half)
                    OutputBit(false);
                else if (m_low >= Half)
                {
                    OutputBit(true);
                    m_low -= Half;
                }
                else
                {
                    m_counter++;
                    m_low -= Quarter;
                }

                m_low *= 2;
                m_range *= 2;
            }
        }

        private void OutputBit(bool bit)
        {
            m_writer.Write(bit);

            while (m_counter > 0)
            {
                m_writer.Write(!bit);
                m_counter--;
            }
        }
    }
}