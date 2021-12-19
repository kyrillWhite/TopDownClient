using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownLibrary;

namespace TopDownGrpcClient.EventArgs
{
    public class PlayerInputsEventArgs : System.EventArgs
    {
        public Dictionary<int, Input> Inputs { get; set; }
    }
}
