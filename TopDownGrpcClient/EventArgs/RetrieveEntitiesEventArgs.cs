using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownGrpcClient
{
    public class RetrieveEntitiesEventArgs : System.EventArgs
    {
        public List<(string, int, float, float)> EntityPositions { get; set; }
    }
}
