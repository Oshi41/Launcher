using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Updator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                return;

            var downloaded = args[0];
            var exeMame = args[1];
            var threadName = args[2];

            Process[] processes;

            do
            {
                processes = Process.GetProcessesByName(threadName);
                foreach (var proc in processes)
                {
                    proc.Kill();
                }

                Thread.Sleep(50);

            } while (processes.Length != 0);

            File.Delete(exeMame);

            File.Move(downloaded, exeMame);
        }
    }
}
