using System;

namespace TopDownGrpcClient
{
    public class PlayerDataEventArgs : EventArgs
    {
        public int LastId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
