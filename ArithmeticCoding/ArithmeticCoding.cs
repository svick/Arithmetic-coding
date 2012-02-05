using System.IO;

namespace ArithmeticCoding
{
    public static class ArithmeticCoding
    {
        public static void Compress(string inputFile, string outputFile)
        {
            var coder = new Coder();

            using (var reader = File.OpenRead(inputFile))
            using (var writer = new BitWriter(outputFile))
            {
                coder.Encode(reader, writer);
            }
        }

        public static void Decompress(string inputFile, string outputFile)
        {
            var decoder = new Decoder();

            using (var reader = new BitReader(inputFile))
            using (var writer = new FileStream(outputFile, FileMode.Create))
            {
                decoder.Decode(reader, writer);
            }
        }
    }
}