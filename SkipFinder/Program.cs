using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkipFinder
{
    /// <summary>
    /// Utility for finding all skipped tests and unskipping tests
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            foreach (var path in args)
            {
                List<string> input = GetUnskippedTests(path);
                if (input.Count() > 0)
                {
                    UnskipTests(input);
                }
                List<string> output = new List<string>();
                GetAllSkippedTestsInDirectory(output, path);
                File.WriteAllLines($"{path}\\SkippedTests.txt", output);
            }
        }

        /// <summary>
        /// Unskip tests
        /// </summary>
        /// <param name="input"></param>
        public static void UnskipTests(List<string> input)
        {
            foreach (var entry in input)
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
                Console.WriteLine($"Unskipping test {test} in a pack: {path}");
                File.WriteAllLines(path, tests);
            }
        }

        /// <summary>
        /// Get a list of tests to unskip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetUnskippedTests(string path)
        {
            Console.WriteLine($"Scanning {path}\\SkippedTests.txt for tests to unskip");
            List<string> input = new List<string>();
            if (!Directory.Exists(path) && !File.Exists($"{path}\\SkippedTests.txt"))
            {
                Console.WriteLine($"File {path + "\\SkippedTests.txt"} has not been found");
                return input;
            }

            var skippedTests = from line in File.ReadAllLines($"{path}\\SkippedTests.txt")
                                where !line.Contains(@"--\skip")
                                select line;
            foreach (var test in skippedTests)
            {
                input.Add(test);
                Console.WriteLine($"Test {test.Split(' ').First()} should be unskipped");
            }
            if (input.Count() == 0)
            {
                Console.WriteLine("No tests to unskip");
            }
            return input;
        } 

        /// <summary>
        /// Get a list of skipped tests
        /// </summary>
        /// <param name="output"></param>
        /// <param name="path"></param>
        public static void GetAllSkippedTestsInDirectory(List<string> output, string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Directory {path} doesn't exist");
                return;
            }

            Console.WriteLine($"Scanning directory {path} for skipped tests");
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.Contains("SkippedTests"))
                {
                    var tests = from line in File.ReadAllLines(file)
                                where line.Contains(@"--\skip")
                                select line;
                    foreach (var test in tests)
                    {
                        output.Add($"{test.Replace(" --\\relation", "")} Pack: {file}");
                        Console.WriteLine($"Adding to SkippedTest.txt test {test.Replace(" --\\relation", "")} Pack: {file}");
                    }
                }
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                GetAllSkippedTestsInDirectory(output, dir);
            }
        }
    }
}
