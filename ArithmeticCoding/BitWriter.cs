using System;
using System.IO;

namespace ArithmeticCoding
{
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
            byte result = (byte)(m_bits >> (m_bitsCount - 8));

            m_bitsCount -= Math.Min(8, m_bitsCount);

            return result;
        }

        public void Write(bool bit)
        {
            m_bits = m_bits << 1 | (bit ? 1U : 0U);

            m_bitsCount += 1;

            Flush();
        }

        public void Write(byte value)
        {
            m_bits = m_bits << 8 | value;

            m_bitsCount += 8;

            Flush();
        }

        public void Write(ulong value)
        {
            for (int i = 7; i >= 0; i--)
                Write((byte)(value >> i * 8));
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