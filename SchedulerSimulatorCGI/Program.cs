using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;
using GestioneAreeCritiche;
using GestioneAreeCritiche.Output;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using GeneratoreAreeCritiche;
using System.Drawing;
using GestioneAreeCritiche.AreeCritiche;

namespace SchedulerSimulatorCGI
{
    public class Program
    {
        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        static void Main(string[] args)
        {  //Remembering the original output writer
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
                    DatiAree datiAree = GestioneAreeCritiche.TrovaAree.TrovaAreeCritiche.Trova(missioni, true, false);

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
                        layoutStr = GeneratoreGrafo.GeneraLayout(missioni);
                    }

                    int idx = layoutStr.LastIndexOf("}");
                    if (idx > 0)
                    {
                        layoutStr = layoutStr.Substring(0, idx);
                    }

                    //associazione tra TRN e colore della missione nel grafo
                    Dictionary<string, string> trnToColor = new Dictionary<string, string>();
                    Edges edges = new Edges();

                    //identifico gli archi
                    foreach (MissioneTreno missione in missioni)
                    {
                        Color missioneColor = GeneratoreGrafo.NextColor();
                        trnToColor.Add(missione.NomeTreno, HexConverter(missioneColor));

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


                    int idArea = 0;
                    foreach (IAreaCritica area in datiAree.AreeCritiche)
                    {
                        IEnumerable<string> nodiList = area.ListaCdb.Select(cdb => string.Format("n{0};",cdb));
                        string nodi = string.Join(string.Empty, nodiList);
                        
                        process.StandardInput.WriteLine("subgraph cluster_"+ idArea + "{" +
                                                        "label=\""+ idArea + "\";" +
                                                        "graph[style=dotted];" +
                                                        nodi +
                                                        "}");
                        idArea++;
                    }

                    process.StandardInput.WriteLine("}");
                    process.StandardInput.Close();

                    //Leggo l'output di Graphviz
                    string svgText = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();
                    process.Dispose();

                    //--------------------
                    //Genero la risposta JSON
                    Missioni m = new Missioni();
                    m.DatiMissioni = missioni.Select(missione => new Missione() { trn = missione.NomeTreno, cdblist = missione.CdbList.ToArray(), color = trnToColor[missione.NomeTreno] }).ToArray();
                    m.MissioniStr = missioniStr;
                    m.DatiAree = datiAree.AreeCritiche.Select(area => new AreaCritica() { limite = area.Limite, cdblist = area.ListaCdb.ToArray() }).ToArray();

                    Data data = new Data();
                    data.Svg = svgText;
                    data.Missioni = m;

                    string json = JsonConvert.SerializeObject(data);

                    Console.SetOut(outWriter);
                    Console.Write("Content-Type: text/json\n\n");
                    Console.Write(json);
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
