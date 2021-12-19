using System;

namespace TopDownGrpcClient
{
    public class PlayerDataEventArgs : System.EventArgs
    {
        public string Id { get; set; }
        public long Time { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
