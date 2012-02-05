using System;
using System.IO;

namespace ArithmeticCoding
{
    class BitReader : IDisposable
    {
        private readonly Stream m_stream;

        private byte m_bits;
        private int m_bitsRead = 8;

        public BitReader(Stream stream)
        {
            m_stream = stream;
        }

        public BitReader(string fileName)
        {
            m_stream = new FileStream(fileName, FileMode.Open);
        }

        public bool ReadBit()
        {
            if (m_bitsRead == 8)
            {
                int read = m_stream.ReadByte();
                if (read == -1)
                    throw new InvalidOperationException();

                m_bits = (byte)read;
                m_bitsRead = 0;
            }

            bool result = (m_bits & 1 << (7 - m_bitsRead)) != 0;
            m_bitsRead++;

            return result;
        }

        public ulong ReadUInt64()
        {
            if (m_bitsRead != 8)
                throw new InvalidOperationException();

            byte[] bytes = new byte[8];

            int offset = 0;

            int read;
            do
            {
                read = m_stream.Read(bytes, offset, bytes.Length - offset);
                offset += read;
            } while (read != 0 && offset < bytes.Length);

            Array.Reverse(bytes);

            return BitConverter.ToUInt64(bytes, 0);
        }

        public void Dispose()
        {
            m_stream.Dispose();
        }
    }
}