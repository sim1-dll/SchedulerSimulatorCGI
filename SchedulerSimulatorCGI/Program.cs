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

namespace SchedulerSimulatorCGI
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

            NameValueCollection query = HttpUtility.ParseQueryString(queryString);
            string missioniStr = query["Missions"];
            if (missioniStr != null)
            {
                List<MissioneTreno> missioni = GestioneAreeCritiche.TrovaAree.TrovaAreeCritiche.CaricaMissioni(missioniStr);
                DatiAree dati = GestioneAreeCritiche.TrovaAree.TrovaAreeCritiche.Trova(missioni, true, false);


                Missioni m = new Missioni();

                m.DatiMissioni = missioni.Select(missione => new Missione() { trn = missione.NomeTreno, cdblist = missione.CdbList.ToArray() }).ToArray();
                m.MissioniStr = missioniStr;
                m.DatiAree = dati.AreeCritiche.Select(area => new AreaCritica() { limite = area.Limite, cdblist = area.ListaCdb.ToArray() }).ToArray();

                string json = JsonConvert.SerializeObject(m);

                Console.SetOut(outWriter);
                Console.Write("Content-Type: text/json\n\n");
                Console.Write(json);
            }

            Console.Out.Flush();
        }
    }
}
