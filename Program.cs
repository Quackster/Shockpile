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
            string cstDirectory = @"C:\Users\Alex\Documents\Decompiler\v14";
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

                string outputDirectory = Path.Combine(projectraysDirectory, fileName);

                if (!Directory.Exists(outputDirectory))
                {
                    Process p = new Process();
                    p.StartInfo.WorkingDirectory = projectraysDirectory;
                    p.StartInfo.FileName = node;
                    p.StartInfo.Arguments = "bin/index.js \"" + fullFileName + "\"";
                    p.Start();
                    p.WaitForExit();
                }

                PerformCleanup(fullFileName, fileName, outputDirectory, srcDirectory);
            }

            Console.Read();
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

                lingoDirectory = Path.Combine(lingoDirectory, Path.GetFileName(directories[0]));

                // Move source files
                string[] sourceFiles = Directory.GetFiles(lingoDirectory);

                foreach (string sourceFile in sourceFiles)
                {
                    File.Move(sourceFile, Path.Combine(outputDirectory, Path.GetFileName(sourceFile)));
                }
            }

            // Delete useless folders
            try
            {
                Directory.Delete(Path.Combine(outputDirectory, "casts"), true);
                Directory.Delete(Path.Combine(outputDirectory, "chunks"), true);
                Directory.Delete(Path.Combine(outputDirectory, "lingo"), true);
            } catch { }

            // Move folders
            if (Directory.Exists(outputDirectory))
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(Path.Combine(srcDirectory, fileName));
                }


                Directory.Move(outputDirectory, Path.Combine(srcDirectory, fileName));
            }

            Console.WriteLine(lingoDirectory);
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
