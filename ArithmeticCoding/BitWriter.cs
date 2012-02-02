using System;
using System.IO;

namespace ArithmeticCoding
{
    // TODO: go over this code to make sure it works correctly
    class BitWriter : IDisposable
    {
        private readonly Stream m_stream;

        private uint m_bits = 0;

        private int m_bitsCount = 0;

        public BitWriter(string fileName)
            : this(File.OpenWrite(fileName))
        {}

        public BitWriter(Stream stream)
        {
            m_stream = stream;
        }

        byte DequeueByte()
        {
            byte result = (byte)(m_bits & byte.MaxValue);

            m_bits >>= 8;

            m_bitsCount -= Math.Min(8, m_bitsCount);

            return result;
        }

        public void Write(bool bit)
        {
            m_bits |= (bit ? 1U : 0U) << m_bitsCount;

            m_bitsCount += 1;

            Flush();
        }

        public void Write(byte value)
        {
            m_bits |= (uint)(value << m_bitsCount);

            m_bitsCount += 8;

            Flush();
        }

        public void Write(uint value, int bitCount)
        {
            value = value & ((1U << bitCount) - 1);

            if (m_bitsCount + bitCount <= 32)
            {
                m_bits |= value << m_bitsCount;

                m_bitsCount += bitCount;
            }
            else
            {
                Write(value, 16);
                Write(value >> 16, bitCount - 16);
            }

            Flush();
        }

        public void Write(ulong value)
        {
            Write(value, 64);
        }

        public void Write(ulong value, int bitCount)
        {
            value = value & ((1UL << bitCount) - 1);

            while (bitCount > 16)
            {
                Write((uint)value, 16);

                bitCount -= 16;

                value >>= 16;
            }

            Write((uint)value, bitCount);
        }

        private void Flush()
        {
            while (m_bitsCount >= 8)
                m_stream.WriteByte(DequeueByte());
        }

        public void Dispose()
        {
            while (m_bitsCount > 0)
                m_stream.WriteByte(DequeueByte());

            m_stream.Dispose();
        }
    }
}