using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerSimulatorCGI
{
    public class Missione
    {
        public string trn { get; set; }
        public int[] cdblist { get; set; }
    }

    public class AreaCritica
    {
        public int limite { get; set; }
        public int[] cdblist { get; set; }
    }

    public class Missioni
    {
        public Missione[] DatiMissioni { get; set; }
        public AreaCritica[] DatiAree { get; set; }

        public string MissioniStr { get; set; }
        public string AreeCriticheStr { get; set; }
    }
}
