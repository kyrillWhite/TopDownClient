﻿using System;

namespace TopDownGrpcClient
{
    public class PlayerDataEventArgs : EventArgs
    {
        public string Id { get; set; }
        public int LastId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
