using System;

namespace ArithmeticCoding.Console
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                ShowHelp();
                return;
            }

            switch (args[0])
            {
            case "-c":
            case "c":
                Compress(args[1], args[2]);
                break;
            case "-d":
            case "d":
                Decompress(args[1], args[2]);
                break;
            default:
                ShowHelp();
                break;
            }
        }

        private static void ShowHelp()
        {
            throw new NotImplementedException();
        }

        private static void Compress(string inputFile, string outputFile)
        {
            ArithmeticCoding.Compress(inputFile, outputFile);
        }

        private static void Decompress(string inputFile, string outputFile)
        {
            ArithmeticCoding.Decompress(inputFile, outputFile);
        }
    }
}