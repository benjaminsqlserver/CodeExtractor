/*
 * C# Code Extractor Tool
 * ======================
 * 
 * Description:
 * This console application scans a specified source directory (and its subdirectories) for all C# source files 
 * with `.cs` or `.csx` extensions. It then compiles their contents into a single formatted output file, including
 * metadata such as file name, size, last modified date, and the full file path. The resulting file serves as a 
 * comprehensive report or backup of all C# code within the given directory structure.
 * 
 * Features:
 * - Accepts source folder and output file path as command-line arguments.
 * - If arguments are not provided, it prompts the user interactively.
 * - Recursively searches the directory for C# files.
 * - Extracts and appends content with metadata per file.
 * - Logs warnings if any file fails to read.
 * - Generates a summary of the extraction process at the end of the output file.
 * 
 * Usage:
 * 1. Run via command line:
 *      > CodeExtractor.exe "C:\Projects\MyApp" "C:\Backup\ExtractedCode.txt"
 * 
 * 2. Or run without arguments and follow prompts:
 *      > CodeExtractor.exe
 * 
 * Example Output File Structure:
 * - Header with extraction date and source path
 * - For each file:
 *     - Header with metadata (name, path, size, modified date)
 *     - Actual file content
 * - Summary including count of processed and total files
 * 
 * Notes:
 * - The tool uses UTF-8 encoding for reading and writing files.
 * - Handles errors gracefully and continues processing remaining files.
 * - Uses a relative path reference for clarity in the output.
 * 
 * Author: [Benjamin Fadina]
 * Created: [25th July 2025]
 */




using System;
using System.IO;
using System.Text;
using System.Linq;

namespace CodeExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("C# Code Extractor");
            Console.WriteLine("================");

            // Determine source folder path: from args[0] or prompt the user
            string sourceFolder = GetSourceFolder(args);

            // Validate if directory exists
            if (!Directory.Exists(sourceFolder))
            {
                Console.WriteLine($"Error: Directory '{sourceFolder}' does not exist.");
                return;
            }

            // Determine output file path: from args[1] or prompt the user
            string outputFile = GetOutputFile(args, sourceFolder);

            try
            {
                // Begin extraction process
                ExtractCSharpCode(sourceFolder, outputFile);
                Console.WriteLine($"\nExtraction completed successfully!");
                Console.WriteLine($"Output file: {outputFile}");
            }
            catch (Exception ex)
            {
                // Catch and report unexpected errors
                Console.WriteLine($"Error during extraction: {ex.Message}");
            }

            // Wait for user before closing console
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Gets the source folder from command-line args or prompts the user.
        /// </summary>
        static string GetSourceFolder(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                return args[0]; // Use command-line argument
            }

            Console.Write("Enter the source folder path (or press Enter for current directory): ");
            string input = Console.ReadLine();

            // Use current directory if no input provided
            return string.IsNullOrWhiteSpace(input) ? Directory.GetCurrentDirectory() : input;
        }

        /// <summary>
        /// Gets the output file path from command-line args or prompts the user.
        /// </summary>
        static string GetOutputFile(string[] args, string sourceFolder)
        {
            if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
            {
                return args[1]; // Use command-line argument
            }

            // Default filename with timestamp
            string defaultFileName = $"ExtractedCode_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), defaultFileName);

            Console.Write($"Enter output file path (or press Enter for '{defaultPath}'): ");
            string input = Console.ReadLine();

            return string.IsNullOrWhiteSpace(input) ? defaultPath : input;
        }

        /// <summary>
        /// Scans the source directory for C# files and writes them to an output file.
        /// </summary>
        static void ExtractCSharpCode(string sourceFolder, string outputFile)
        {
            // File extensions to include
            var csharpExtensions = new[] { ".cs", ".csx" };
            var sb = new StringBuilder(); // StringBuilder to accumulate output text
            int fileCount = 0;

            // Header for the report
            sb.AppendLine("C# CODE EXTRACTION REPORT");
            sb.AppendLine("========================");
            sb.AppendLine($"Source Directory: {sourceFolder}");
            sb.AppendLine($"Extraction Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            Console.WriteLine($"Scanning directory: {sourceFolder}");

            // Recursively get all .cs and .csx files
            var csharpFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => csharpExtensions.Contains(Path.GetExtension(file).ToLower()))
                .OrderBy(file => file)
                .ToList();

            Console.WriteLine($"Found {csharpFiles.Count} C# files");

            foreach (string filePath in csharpFiles)
            {
                try
                {
                    Console.WriteLine($"Processing: {GetRelativePath(sourceFolder, filePath)}");

                    // Write metadata/header for the file
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine($"FILE: {GetRelativePath(sourceFolder, filePath)}");
                    sb.AppendLine($"FULL PATH: {filePath}");
                    sb.AppendLine($"SIZE: {new FileInfo(filePath).Length} bytes");
                    sb.AppendLine($"MODIFIED: {File.GetLastWriteTime(filePath):yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine();

                    // Read and append file content
                    string content = File.ReadAllText(filePath, Encoding.UTF8);
                    sb.AppendLine(content);
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 80));
                    sb.AppendLine();

                    fileCount++; // Successfully processed file
                }
                catch (Exception ex)
                {
                    // Log warning and note in output file if reading fails
                    Console.WriteLine($"Warning: Could not read file '{filePath}': {ex.Message}");

                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine($"FILE: {GetRelativePath(sourceFolder, filePath)} [ERROR]");
                    sb.AppendLine($"ERROR: {ex.Message}");
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine();
                }
            }

            // Append summary to the output
            sb.AppendLine();
            sb.AppendLine(new string('=', 80));
            sb.AppendLine("EXTRACTION SUMMARY");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine($"Total files processed: {fileCount}");
            sb.AppendLine($"Total files found: {csharpFiles.Count}");
            sb.AppendLine($"Source directory: {sourceFolder}");
            sb.AppendLine($"Output file: {outputFile}");
            sb.AppendLine($"Extraction completed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            // Save output to file
            File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);

            Console.WriteLine($"\nProcessed {fileCount} files successfully");
        }

        /// <summary>
        /// Generates a relative path from a full path, given a base path.
        /// </summary>
        static string GetRelativePath(string basePath, string fullPath)
        {
            // Ensure trailing slash for base URI
            Uri baseUri = new Uri(basePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri fullUri = new Uri(fullPath);

            // If paths are on different schemes (e.g., file:// vs http://), return full path
            if (baseUri.Scheme != fullUri.Scheme)
            {
                return fullPath;
            }

            // Return relative path with correct separators
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
