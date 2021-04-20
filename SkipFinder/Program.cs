using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkipFinder
{
    /// <summary>
    /// Utility for finding all skipped tests and unskipping tests
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Get a list of unskipped test from a file with all skipped tests, unskip them in corresponding packs, create a new file with skipped tests
        /// </summary>
        /// <param name="args">Paths to directories that contain test packs </param>
        static void Main(string[] args)
        {
            foreach (var path in args)
            {
                GetUnskippedTestsFromFile(path);
                UnskipTests();
                Console.WriteLine("----------Search for skipped tests----------");
                GetAllSkippedTestsInDirectory(path);
                Console.WriteLine("\r\n----------Search is over---------\r\n");
                SaveSkippedTestsToFile(path);
            }
        }

        public static List<string> TestsToUnskip { get; set; } = new List<string>();

        public static List<string> SkippedTests { get; set; } = new List<string>();

        /// <summary>
        /// Saving skipped tests to file
        /// </summary>
        /// <param name="path"></param>
        public static void SaveSkippedTestsToFile(string path)
        {
            Console.WriteLine("----------Saving tests to file----------\r\n");
            var message = SkippedTests.Count() == 0 ? "No skipped tests to save - saving empty" : "Saving skipped tests to";
            Console.WriteLine($"{message} file {path}\\SkippedTests.txt");
            File.WriteAllLines($"{path}\\SkippedTests.txt", SkippedTests);
            Console.WriteLine("\r\n----------File has been saved----------\r\n");
            SkippedTests.Clear();
        }

        /// <summary>
        /// Unskip tests
        /// </summary>
        public static void UnskipTests()
        {
            if (TestsToUnskip.Count() == 0)
            {
                return;
            }

            foreach (var entry in TestsToUnskip)
            {
                var path = entry.Split(' ').Last();
                var test = entry.Split(' ').First();
                if (!File.Exists(path))
                {
                    Console.WriteLine($"File {path} hasn't been found.");
                    continue;
                }
                var tests = File.ReadAllLines(path).ToList();
                for (int i = 0; i < tests.Count(); i++)
                {
                    if (tests[i].Contains("skip") && tests[i].Contains(test))
                    {
                        tests[i] = tests[i].Split(new string[] { @"--\skip"}, StringSplitOptions.RemoveEmptyEntries).First();
                    }
                }
                Console.WriteLine($"Unskipping test: {test} in a pack: {path}");
                File.WriteAllLines(path, tests);
                Console.WriteLine($"Test: {test} has been succesfully unskipped and saved to file.");
            }
            Console.WriteLine("\r\nUnskip over\r\n---------\r\n");
        }

        /// <summary>
        /// Get a list of tests to unskip
        /// </summary>
        /// <param name="path"></param>
        public static void GetUnskippedTestsFromFile(string path)
        {
            Console.WriteLine("\r\n----------Performing unskip----------");
            Console.WriteLine($"\r\nScanning {path}\\SkippedTests.txt for tests to unskip\r\n");

            if (!Directory.Exists(path) && !File.Exists($"{path}\\SkippedTests.txt"))
            {
                Console.WriteLine($"File {path + "\\SkippedTests.txt"} has not been found");
                return;
            }

            var skippedTests = from line in File.ReadAllLines($"{path}\\SkippedTests.txt")
                                where !line.Contains(@"--\skip")
                                select line;
            foreach (var test in skippedTests)
            {
                TestsToUnskip.Add(test);
                Console.WriteLine($"Test: {test.Split(' ').First()} should be unskipped");
            }
            if (TestsToUnskip.Count() == 0)
            {
                Console.WriteLine("No tests to unskip\r\n\r\n----------Unskip over----------\r\n");
            }
        } 

        /// <summary>
        /// Get a list of skipped tests
        /// </summary>
        /// <param name="path"></param>
        public static void GetAllSkippedTestsInDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Directory {path} doesn't exist");
                return;
            }

            Console.WriteLine($"\r\nScanning directory {path} for skipped tests\r\n");
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.Contains("SkippedTests"))
                {
                    var tests = from line in File.ReadAllLines(file)
                                where line.Contains(@"--\skip")
                                select line;
                    foreach (var test in tests)
                    {
                        SkippedTests.Add($"{test.Replace(" --\\relation", "")} Pack: {file}");
                        Console.WriteLine($"Adding to list Test: {test.Replace(" --\\relation", "")} Pack: {file}");
                    }
                }
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                GetAllSkippedTestsInDirectory(dir);
            }
           
        }
    }
}
