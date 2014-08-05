using GestioneAreeCritiche;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MissioniToSvg
{
    public class Program
    {
        private static Random r = new Random();
        private static int colorIdx = 1;

        private static Color NextColor()
        {
            Color color = Color.Black;
            switch (colorIdx)
            {
                case 1:
                    {
                        color = Color.Red;
                    }
                    break;
                case 2:
                    {
                        color = Color.Blue;
                    }
                    break;
                case 3:
                    {
                        color = Color.Green;
                    }
                    break;
                case 4:
                    {
                        color = Color.Olive;
                    }
                    break;
                case 5:
                    {
                        color = Color.Purple;
                    } break;
                case 6:
                    {
                        color = Color.Brown;
                    }
                    break;
                case 7:
                    {
                        color = Color.Orange;
                    }
                    break;
                default:
                    {
                        color = Color.Black;
                    }
                    break;
            }
            colorIdx++;
            return color;
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private static string GeneraLayout(List<MissioneTreno> missioni)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"digraph G {
                ranksep=1.0;
                nodesep=0.75;
                rankdir=LR;
        
                node [shape=circle,fontsize=20;fixedsize=true;];
                edge [arrowhead=none,dir=none];");

            HashSet<int> nodi = new HashSet<int>();

            //identifico i nodi
            foreach (MissioneTreno missione in missioni)
            {
                var cdblist = missione.CdbList;

                for (var j = 0; j < cdblist.Count; j++)
                {
                    var cdb = cdblist[j];
                    if (!nodi.Contains(cdb))
                    {
                        nodi.Add(cdb);
                    }
                }
            }

            foreach (int nodo in nodi)
            {
                sb.AppendLine(string.Format("n{0} [label=\"{0}\"];", nodo));
            }

            sb.Append("}");

            return sb.ToString();
        }

        static void Main(string[] args)
        {
            //Remembering the original output writer
            TextWriter outWriter = Console.Out;

            //Temporarily disable output
            Console.SetOut(TextWriter.Null);

            string queryString = System.Environment.GetEnvironmentVariable("QUERY_STRING");

            string missioniStr = null;
            string layoutStr = null;
            if (!string.IsNullOrEmpty(queryString))
            {
                NameValueCollection query = HttpUtility.ParseQueryString(queryString);
                missioniStr = query["Missions"];
                layoutStr = query["Layout"];
            }

            //usato per debug via command line
            if (string.IsNullOrEmpty(missioniStr))
            {
                if (args.Length > 0)
                {
                    missioniStr = args[0];
                }
            }

            if (missioniStr != null)
            {
                try
                {
                    List<MissioneTreno> missioni = GestioneAreeCritiche.TrovaAree.TrovaAreeCritiche.CaricaMissioni(missioniStr);
                    
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

                    if (string.IsNullOrEmpty(layoutStr))
                    {
                        layoutStr = GeneraLayout(missioni);
                    }

                    int idx = layoutStr.LastIndexOf("}");
                    if (idx > 0)
                    {
                        layoutStr = layoutStr.Substring(0, idx);
                    }

                    Edges edges = new Edges();
                    //identifico gli archi
                    foreach (MissioneTreno missione in missioni)
                    {
                        Color missioneColor = NextColor();
                        var cdbprec = -1;
                        for (int j = 0; j < missione.CdbList.Count; j++)
                        {
                            var cdb = missione.CdbList[j];
                            if (cdbprec != -1)
                            {
                                int src = cdbprec;
                                int dest = cdb;

                                KeyValuePair<int, int> arco = new KeyValuePair<int, int>(src, dest);
                                edges.Add(src, dest, missioneColor);
                            }
                            cdbprec = cdb;
                        }
                    }

                    foreach (KeyValuePair<int, int> edge in edges.archi.Keys)
                    {
                        string edgeStr = string.Format("n{0} -> n{1};", edge.Key, edge.Value);

                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine("{");
                        IEnumerable<string> coloriHex = edges.archi[edge].Select(color => HexConverter(color));
                        string colori = string.Join(":", coloriHex);
                        sb.AppendLine(string.Format("edge [color=\"{0}\"]", colori));
                        sb.AppendLine(edgeStr);
                        sb.AppendLine("}");

                        //se trovo nel layout l'edge che sto inserendo, lo sostituisco con quello colorato
                        if (layoutStr.Contains(edgeStr))
                        {
                            layoutStr = layoutStr.Replace(edgeStr, sb.ToString());
                        }
                        else
                        {
                            layoutStr += sb.ToString();
                        }                        
                    }

                    process.StandardInput.WriteLine(layoutStr);

                   
                    process.StandardInput.WriteLine("}");
                    process.StandardInput.Close();

                    //forward output
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
                catch (Exception e)
                {
                    Console.SetOut(outWriter);
                    Console.Write("Content-Type: text/html\n\n");

                    Console.WriteLine("<html><body>Error" + e + "</body></html>");
                }
            }
        }
    }
}
