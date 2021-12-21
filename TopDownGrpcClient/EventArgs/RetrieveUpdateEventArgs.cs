using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownLibrary;

namespace TopDownGrpcClient
{
    public class RetrieveUpdateEventArgs : EventArgs
    {
        public List<(string, int, float, float)> EntityPositions { get; set; }
        public List<BulletData> Bullets { get; set; }
    }
}
