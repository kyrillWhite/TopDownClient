using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownGrpcClient
{
    public class RetrieveEntitiesEventArgs : EventArgs
    {
        public List<(float, float)> EntityPositions { get; set; }
    }
}
