using System;
using System.IO;
using log4net;

namespace ArithmeticCoding
{
    class Coder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Coder));

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

            Log.DebugFormat("Coding file writh {0} bytes.", m_length);

            m_writer.Write(m_length);

            for (int i = 0; i < m_byteCounts.Length; i++)
            {
                m_writer.Write(m_byteCounts[i]);

                // to compute cumulated counts
                if (i != 0)
                    m_byteCounts[i] += m_byteCounts[i - 1];
            }

            m_low = 0;
            m_range = Constants.Half;
            m_counter = 0;

            reader.Position = 0;

            while ((b = reader.ReadByte()) != -1)
            {
                EncodeByte((byte)b);
            }

            for (int i = Constants.BitsUsed; i >= 0; i--)
            {
                bool bit = ((1UL << i) & m_low) != 0;
                OutputBit(bit);
            }
        }

        private void EncodeByte(byte b)
        {
            Log.DebugFormat("Coding byte 0x{0:X2}.", b);

            ulong rangeUnit = m_range / m_length;

            ulong byteCumulatedCount = m_byteCounts[b];
            ulong previousByteCumultedCount = b == 0 ? 0 : m_byteCounts[b - 1];

            Log.DebugFormat(
                "D={0}; R={1}; r={2}; mz={3}; mz-1={4}",
                m_low / Constants.LogConstant, m_range / Constants.LogConstant, rangeUnit, byteCumulatedCount, previousByteCumultedCount);

            m_low += rangeUnit * previousByteCumultedCount;

            if (byteCumulatedCount != m_length)
                m_range = rangeUnit * (byteCumulatedCount - previousByteCumultedCount);
            else
                m_range = m_range - rangeUnit * previousByteCumultedCount;

            Log.DebugFormat("D={0}; R={1}", m_low / Constants.LogConstant, m_range / Constants.LogConstant);

            while (m_range <= Constants.Quarter)
            {
                if (m_low + m_range <= Constants.Half)
                    OutputBit(false);
                else if (m_low >= Constants.Half)
                {
                    OutputBit(true);
                    m_low -= Constants.Half;
                }
                else
                {
                    Log.Debug("Incrementing counter of delayed bits.");

                    m_counter++;
                    m_low -= Constants.Quarter;
                }

                m_low *= 2;
                m_range *= 2;

                Log.DebugFormat("D={0}; R={1}", m_low / Constants.LogConstant, m_range / Constants.LogConstant);
            }
        }

        private void OutputBit(bool bit)
        {
            Log.DebugFormat("Writing bit {0}.", Convert.ToInt32(bit));

            m_writer.Write(bit);

            while (m_counter > 0)
            {
                Log.DebugFormat("Writing delayed bit {0}.", Convert.ToInt32(!bit));

                m_writer.Write(!bit);
                m_counter--;
            }
        }
    }
}