using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Ground : GameObject
    {
        public Ground(string texture, RectangleF rectangle = new RectangleF()) : base(texture, rectangle)
        {
            _layerDepth = 0.1f;
        }
        public Ground() : base() 
        {
            _layerDepth = 0.1f;
        }
    }
}
