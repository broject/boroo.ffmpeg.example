using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace boroo.ffmpeg.example
{
    class Program
    {
        const string InputDir = @"E:\\inputs\\";
        const string OutputDir =  @"E:\\outputs\\";
        const string FFmpegPath = @"C:\\ffmpeg\\bin\\ffmpeg.exe";

        bool isCompleted = false;
        List<Process> processList = new List<Process>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Program p = new Program();
            p.start(args);

            Console.ReadLine();
        }

        public void start(string[] args)
        {
            string dirPath = InputDir;
            if (args != null && args.Length > 0)
            {
                dirPath = args[0];
            }
            Thread thread = null;
            while (!isCompleted)
            {
                if (thread == null)
                {
                    thread = new Thread(convertProcess);
                    thread.Start(dirPath);
                }

                Thread.Sleep(2000);
            }
        }

        // private method

        private void convertProcess(object param) 
        {
            isCompleted = false;
            Console.WriteLine("Хөрвүүлэлт эхэллээ.");

            string dirPath = (string)param;
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                convertFile(file); 
            }

            isCompleted = true;
            killInterruptProcess();
            Console.WriteLine("Хөрвүүлэлт дууслаа.");
        }

        private void convertFile(FileInfo file)
        {
            string fileTitle = Path.GetFileNameWithoutExtension(file.Name);
            string extension = file.Extension;
            string srcFilePath = file.FullName;

            if (extension == ".mp4" || extension == ".avi")
            {
                Console.WriteLine("===========================================================================");
                Console.WriteLine("Хөрвүүлэх файл олдлоо: '{0}'.", file.Name);

                String destPath = OutputDir + fileTitle;

                try
                {
                    Process proc1 = createProcess(srcFilePath, destPath, "webm");
                    processList.Add(proc1);
                    proc1.WaitForExit();
                    proc1.Close();
                    processList.Remove(proc1);

                    if (extension == ".avi")
                    {
                        Process proc2 = createProcess(srcFilePath, destPath, "mp4");
                        processList.Add(proc2);
                        proc2.WaitForExit();
                        proc2.Close();
                        processList.Add(proc2);
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("Алдаа: '{0}'.", ex.Message);
                    killInterruptProcess();
                }

                Console.WriteLine("Файлыг хөрвүүлж дууслаа: '{0}'.", file.Name);
                Console.WriteLine("===========================================================================");
            }
            else
            {
                Console.WriteLine("Хөрвүүлэх шаардлагад нийцэхгүй файл олдлоо.");
                Console.WriteLine("Файлыг устгаж байна. '{0}'.", file.FullName);
                File.Delete(file.FullName);
            }

        }

        void killInterruptProcess()
        {
            foreach(Process p in processList)
            {
                try
                {
                    p.Kill();
                }
                catch { }
            }
            processList.Clear();
        }

        Process createProcess(string srcFilePath, string destPath, string toType)
        {
            Process proc = new Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = FFmpegPath;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.Arguments = "-i \"" + srcFilePath + "\" \"" 
                + destPath + (toType.StartsWith(".") ? toType : "." + toType) + "\" -y";
            proc.Start();
            return proc;
        }
    }
}
