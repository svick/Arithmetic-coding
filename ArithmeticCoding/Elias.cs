namespace ArithmeticCoding
{
    static class Elias
    {
        public static void GammaCode(ulong number, BitWriter writer)
        {
            // to allow zero
            number += 1;

            int size = 0;
            ulong tmp = number;

            while (tmp >= 1)
            {
                size++;
                tmp /= 2;
            }

            int zeroes = size - 1;

            for (int i = 0; i < zeroes; i++)
                writer.Write(false);

            for (int i = size - 1; i >= 0; i--)
            {
                bool bit = (number & (1UL << i)) != 0;
                writer.Write(bit);
            }
        }

        public static ulong GammaDecode(BitReader reader)
        {
            int size = 0;

            while (reader.ReadBit() == false)
                size++;

            ulong result = 1;

            for (int i = 0; i < size; i++)
                result = (result << 1) + (reader.ReadBit() ? 1UL : 0UL);

            return result - 1;
        }
    }
}