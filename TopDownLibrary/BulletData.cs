using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLibrary
{
    public class BulletData
    {
        public DateTime CreationTime { get; set; }
        public float StartPosX { get; set; }
        public float StartPosY { get; set; }
        public float EndPosX { get; set; }
        public float EndPosY { get; set; }
        public int Team { get; set; }
        public float Speed { get; set; }
        public int Id { get; set; }
    }
}
