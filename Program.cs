using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shockpile
{
    class Program
    {
        static void Main(string[] args)
        {
            string node = @"C:\Program Files\nodejs\node.exe";
            string projectraysDirectory = @"C:\Users\Alex\Documents\Decompiler\ProjectorRays";
            string cstDirectory = @"C:\Users\Alex\Documents\Decompiler\v7";
            string srcDirectory = Path.Combine(cstDirectory, "src");//@"C:\Users\Alex\Documents\Decompiler\v7\src";

            if (!Directory.Exists(projectraysDirectory))
            {
                WriteError("The projectrays directory is not supplied!", true);
                return;
            }

            if (!Directory.Exists(cstDirectory))
            {
                WriteError("The directory for cst files doesn't exist!", true);
                return;
            }

            if (!Directory.Exists(srcDirectory))
            {
                WriteWarning("The src directory doesn't exist... creating", false);
            }
            else
            {
                Directory.Delete(srcDirectory, true);
            }

            Directory.CreateDirectory(srcDirectory);

            string[] fileEntries = Directory.GetFiles(cstDirectory);

            foreach (string fullFileName in fileEntries)
            {
                string fileExtension = Path.GetExtension(fullFileName);
                string fileName = Path.GetFileNameWithoutExtension(fullFileName);

                if (fileExtension != ".cst")
                    continue;

                if (fileName == "hh_room_pool")
                {
                    string outputDirectory = Path.Combine(projectraysDirectory, fileName);

                    if (!Directory.Exists(outputDirectory))
                    {
                        Process p = new Process();
                        p.StartInfo.WorkingDirectory = projectraysDirectory;
                        p.StartInfo.FileName = node;
                        p.StartInfo.Arguments = "bin/index.js \"" + fullFileName + "\"";
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        p.Start();
                        p.WaitForExit();
                    }


                    PerformCleanup(fullFileName, fileName, outputDirectory, srcDirectory);
                }
            }
        }

        private static void PerformCleanup(string fullFileName, string fileName, string outputDirectory, string srcDirectory)
        {
            string lingoDirectory = Path.Combine(outputDirectory, "casts", "test");

            if (Directory.Exists(lingoDirectory))
            {


                string[] directories = Directory.GetDirectories(lingoDirectory);

                if (directories.Length <= 0)
                {
                    return;
                }

                string copyDirectory = Path.Combine(lingoDirectory, Path.GetFileName(directories[0]));

                // Move source files
                string[] sourceFiles = Directory.GetFiles(copyDirectory);

                foreach (string sourceFile in sourceFiles)
                {
                    string fromDirectory = sourceFile;
                    string toDirectory = Path.Combine(outputDirectory, Path.GetFileName(sourceFile));
                    File.Move(fromDirectory, toDirectory);
                }
            }

            bool copyLingoFolder = false;

            // Delete useless folders
            try
            {
                Directory.Delete(Path.Combine(outputDirectory, "casts"), true);
            } catch { copyLingoFolder = true;  }

            try
            {
                Directory.Delete(Path.Combine(outputDirectory, "chunks"), true);
            }
            catch { }

            try
            {
                if (copyLingoFolder)
                {
                    string[] directories = Directory.GetDirectories(Path.Combine(outputDirectory, "lingo"));
                    string copyDirectory = Path.Combine(outputDirectory, "lingo", Path.GetFileName(directories[0]));

                    // Move source files
                    string[] sourceFiles = Directory.GetFiles(copyDirectory);

                    foreach (string sourceFile in sourceFiles)
                    {
                        string fromDirectory = sourceFile;
                        string toDirectory = Path.Combine(outputDirectory, Path.GetFileName(sourceFile));
                        File.Move(fromDirectory, toDirectory);
                    }
                }

                Directory.Delete(Path.Combine(outputDirectory, "lingo"), true);
            }
            catch { }

            string newDirectory = Path.Combine(srcDirectory, "script_" + fileName);

            // Move folders
            if (Directory.Exists(outputDirectory))
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }


                Directory.Move(outputDirectory, newDirectory);
            }


            Console.WriteLine("Processed " + fileName);
        }

        private static void WriteError(string v, bool block = false)
        {
            Console.WriteLine("[ERROR] " + v);

            if (block)
                Console.Read();

        }

        private static void WriteWarning(string v, bool block = false)
        {
            Console.WriteLine("[WARN] " + v);


            if (block)
                Console.Read();
        }
    }
}
