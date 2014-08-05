using GestioneAreeCritiche;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerSimulatorCGI
{
    public class GeneratoreGrafo
    {
        private static Random r = new Random();
        private static int colorIdx = 1;

        public static Color NextColor()
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

        public static string GeneraLayout(List<MissioneTreno> missioni)
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
    }
}
