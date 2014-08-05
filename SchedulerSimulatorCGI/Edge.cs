using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratoreAreeCritiche
{
    internal class Edges
    {
        public readonly Dictionary<KeyValuePair<int, int>, List<Color>> archi = new Dictionary<KeyValuePair<int, int>, List<Color>>();

        public void Add(int s, int d, Color color)
        {
            int source = s;
            int dest = d;
            if (d < s)
            {
                source = d;
                dest = s;
            }

            KeyValuePair<int, int> arco = new KeyValuePair<int, int>(source, dest);
            if (!archi.ContainsKey(arco))
            {
                archi.Add(arco, new List<Color>());
            }

            archi[arco].Add(color);
        }
    }
}
