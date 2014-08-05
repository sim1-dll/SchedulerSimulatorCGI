using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace DotToSvg
{
    class Program
    {
        static void Main(string[] args)
        {
             //Remembering the original output writer
            TextWriter outWriter = Console.Out;

            //Temporarily disable output
            Console.SetOut(TextWriter.Null);

            string queryString = System.Environment.GetEnvironmentVariable("QUERY_STRING");

            string data = null;
            if (!string.IsNullOrEmpty(queryString))
            {
                NameValueCollection query = HttpUtility.ParseQueryString(queryString);
                data = query["Layout"];
            }

            //usato per debug via command line
            if (string.IsNullOrEmpty(data))
            {
                if (args.Length > 0)
                {
                    data = args[0];
                }
            }

            if (data != null)
            {

                Process process = new Process();
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = @"c:\Program Files (x86)\Graphviz2.38\bin\dot.exe";
                process.StartInfo.Arguments = "-Tsvg";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();

                process.StandardInput.WriteLine(data);
                process.StandardInput.Close();

                Console.SetOut(outWriter);
                Console.Write("Content-Type: image/svg\n\n");

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }

                process.WaitForExit();// Waits here for the process to exit.
                process.Dispose();
            }
        }
    }
}
