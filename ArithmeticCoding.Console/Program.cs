using System;
using System.IO;
using log4net;
using log4net.Config;

namespace ArithmeticCoding.Console
{
    static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                SetupLogging();

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
            catch (Exception e)
            {
                Log.Error(e);
                System.Console.WriteLine(e.Message);
            }
        }

        private static void SetupLogging()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
        }

        private static void ShowHelp()
        {
            string fileName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

            System.Console.WriteLine("Usage: {0} -c input output", fileName);
            System.Console.WriteLine("       {0} -d input output", fileName);
            System.Console.WriteLine();
            System.Console.WriteLine("Use -c to compress input into output.");
            System.Console.WriteLine("Use -d to decompress input into output.");
            System.Console.WriteLine("Input and output are paths to the corresponding files.");
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