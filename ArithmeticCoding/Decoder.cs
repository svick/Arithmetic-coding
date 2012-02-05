using System;
using System.IO;
using log4net;

namespace ArithmeticCoding
{
    class Decoder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Decoder));

        private static readonly ulong Quarter = 1UL << 61;
        private static readonly ulong Half = 1UL << 62;
        private static readonly ulong LogConstant = 1000000000000000;

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

            Log.DebugFormat("Decoding file of {0} bytes.", m_length);

            m_byteCounts = new ulong[256];

            for (int i = 0; i < m_byteCounts.Length; i++)
            {
                m_byteCounts[i] = m_reader.ReadUInt64();
                if (i != 0)
                    m_byteCounts[i] += m_byteCounts[i - 1];
            }

            m_range = Half;
            for (int i = 0; i < 63; i++)
            {
                m_low = (m_low << 1) + Convert.ToUInt64(m_reader.ReadBit());
            }

            Log.DebugFormat("Read first 64 bits: 0x{0:X}.", m_low);

            for (ulong i = 0; i < m_length; i++)
            {
                ulong rangeUnit = m_range / m_length;

                ulong target = Math.Min(m_length - 1, m_low / rangeUnit);

                Log.DebugFormat(
                    "D={0}; R={1}; r={2}; target={3}",
                    m_low / LogConstant, m_range / LogConstant, rangeUnit, target);

                byte b = 0;

                for (byte j = 0; j < m_byteCounts.Length; j++)
                {
                    if (target < m_byteCounts[j])
                    {
                        b = j;
                        break;
                    }
                }

                Log.DebugFormat("Writing byte 0x{0:X2}.", b);

                writer.WriteByte(b);

                FixValues(b);
            }
        }

        private void FixValues(byte b)
        {
            ulong rangeUnit = m_range / m_length;

            ulong byteCumulatedCount = m_byteCounts[b];
            ulong previousByteCumultedCount = b == 0 ? 0 : m_byteCounts[b - 1];

            Log.DebugFormat("mz={0}; mz-1={1}", byteCumulatedCount, previousByteCumultedCount);

            m_low -= rangeUnit * previousByteCumultedCount;

            if (byteCumulatedCount < m_length)
                m_range = rangeUnit * (byteCumulatedCount - previousByteCumultedCount);
            else
                m_range -= rangeUnit * previousByteCumultedCount;

            Log.DebugFormat("D={0}; R={1}", m_low / LogConstant, m_range / LogConstant);

            while (m_range <= Quarter)
            {
                m_range *= 2;
                m_low *= 2;
                if (m_reader.ReadBit())
                {
                    Log.Debug("Read bit 1.");
                    m_low++;
                }
                else
                    Log.Debug("Read bit 0.");
            }

            Log.DebugFormat("D={0}; R={1}", m_low / LogConstant, m_range / LogConstant);
        }
    }
}