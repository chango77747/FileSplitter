using System;
using System.Collections;
using CommandLine;
using System.IO;
using System.Threading.Tasks;
/*File splitter application. Good use case is when exfiling large files over C2 or other means.
 Author: Evan Pena
 Usage:
 Split a file: FileSplitter.exe -f {pathToFile} -s -n 50 -o {pathToOutputFolder}
 Merge a files: FileSplitter.exe -c -d {DirectoryWithTempFiles}
 copy folder to another: FileSplitter.exe -e -l {sourceFolder} -r {destinationFolder}
 Exfil Data: 
     */
namespace FileSplitter
{
    class SplitMerge
    {
        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(opts => DoSomeWork(opts))
                .WithNotParsed((errs) => HandleParseError(errs));
        }

        private static void DoSomeWork(CommandLineOptions opts)
        {
            //Checking to make sure user did not put combine and split at same time
            //Splitting files
            if ((opts.Split) && !(opts.Combine) && !(opts.Exfil))
            {
                if (opts.File != "")
                {
                    Console.WriteLine("You are going to split"+ $": {opts.File}");

                    //Split the file into several
                    splitFile(opts.File, opts.Output, opts.Number);
                }
                else { Console.WriteLine("You need a file to split."); Environment.Exit(1); }

            }

            //Combining files
            else if ((opts.Combine) && !(opts.Split) && !(opts.Exfil))
            {
                Console.WriteLine("You are going to combine" + $": {opts.Directory}");
                mergeFiles(opts.Directory);
            }                                 

            //Exfil data
            else if (!(opts.Split) && !(opts.Combine) && (opts.Exfil))
            {
                if (opts.source != "" && opts.destination != "")
                {
                    int fCount = Directory.GetFiles(opts.source, "*", SearchOption.TopDirectoryOnly).Length;
                    Console.WriteLine("Source folder is " + $": {opts.source}");
                    Console.WriteLine("Destination folder is " + $": {opts.destination}");
                    Console.WriteLine("Total file count is " + $": {fCount}");

                    //Copy function here below. Make sure to writeline after each file was sent and then delete it.
                    CopyFolder(opts.source, opts.destination);
                }
                else { Console.WriteLine("You need a file to split."); Environment.Exit(1); }
            }
            else
            { Console.WriteLine("split and combine are mutually exclusive"); Environment.Exit(1); }
        }

        private static void HandleParseError(IEnumerable errs)
        {
            Console.WriteLine("Command Line parameters provided were not valid!");
        }

        private static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
                File.Delete(file);
                Console.WriteLine("Copied and Deleted" + $": {file}");                
            }

            //This is for recursive if we need to copy the directories as well.
            /*string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }*/
        }

        //The split and merge methods were taken from: http://codemaverick.blogspot.com/2007/01/split-and-merge-file-in-c.html
        private static void splitFile(string filename, string outputdir, int numberFiles)
        {
            string inputFile = filename; // Substitute this with your Input File 
            FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            int numberOfFiles = Convert.ToInt32(numberFiles);
            int sizeOfEachFile = (int)Math.Ceiling((double)fs.Length / numberOfFiles);

            for (int i = 1; i <= numberOfFiles; i++)
            {
                string baseFileName = Path.GetFileNameWithoutExtension(inputFile);
                string extension = Path.GetExtension(inputFile);
                //FileStream outputFile = new FileStream(Path.GetDirectoryName(inputFile) + "\\" + baseFileName + "." + i.ToString().PadLeft(5, Convert.ToChar("0")) + extension + ".tmp", FileMode.Create, FileAccess.Write);
                FileStream outputFile = new FileStream(outputdir + "\\" + baseFileName + "." + i.ToString().PadLeft(5, Convert.ToChar("0")) + extension + ".tmp", FileMode.Create, FileAccess.Write);
                int bytesRead = 0;
                byte[] buffer = new byte[sizeOfEachFile];

                if ((bytesRead = fs.Read(buffer, 0, sizeOfEachFile)) > 0)
                {
                    //Console.WriteLine(outputdir + "\\" + baseFileName + "." + i.ToString().PadLeft(5, Convert.ToChar("0")) + extension + ".tmp");
                    outputFile.Write(buffer, 0, bytesRead);
                }
                outputFile.Close();
            }
            fs.Close();
        }

        private static void mergeFiles(string inputDir)
        {
            string outPath = inputDir; 
            string[] tmpFiles = Directory.GetFiles(outPath, "*.tmp");
            FileStream outputFile = null;
            string prevFileName = "";

            foreach (string tempFile in tmpFiles)
            {

                string fileName = Path.GetFileNameWithoutExtension(tempFile);
                string baseFileName = fileName.Substring(0, fileName.IndexOf(Convert.ToChar(".")));
                string extension = Path.GetExtension(fileName);

                if (!prevFileName.Equals(baseFileName))
                {
                    if (outputFile != null)
                    {
                        outputFile.Flush();
                        outputFile.Close();
                    }
                    outputFile = new FileStream(outPath + baseFileName + extension, FileMode.OpenOrCreate, FileAccess.Write);
                    //Console.WriteLine(outputFile);
                }

                int bytesRead = 0;
                byte[] buffer = new byte[1024];
                FileStream inputTempFile = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.Read);

                while ((bytesRead = inputTempFile.Read(buffer, 0, 1024)) > 0)
                    outputFile.Write(buffer, 0, bytesRead);

                inputTempFile.Close();
                //Console.WriteLine("File written was: " + outPath + baseFileName + extension);
                //Add line below if you want to delete files after merging them.
                //File.Delete(tempFile);
                prevFileName = baseFileName;
            }
            outputFile.Close();
        }
    }

    public sealed class CommandLineOptions
    {
        [Option('f', "file", Required = false, HelpText = "Path to filename")]
        public string File { get; set; }
        [Option('d', "directory", Required = false, HelpText = "Path to input directory")]
        public string Directory { get; set; }
        [Option('o', "output", Required = false, HelpText = "Path to output directory")]
        public string Output { get; set; }
        [Option('c', "combine", Required = false, HelpText = "Combine files in directory")]
        public Boolean Combine { get; set; }
        [Option('s', "split", Required = false, HelpText = "Split file into chunks")]
        public Boolean Split { get; set; }
        [Option('e', "exfil", Required = false, HelpText = "Exfil/Copy data to destination. Deletes files that were sent")]
        public Boolean Exfil { get; set; }
        [Option('l', "source", Required = false, HelpText = "Source directory")]
        public string source { get; set; }
        [Option('r', "destination", Required = false, HelpText = "Destination directory")]
        public string destination { get; set; }
        [Option('n', "number", Required = false, HelpText = "Number of files")]
        public int Number { get; set; }
    }

}
