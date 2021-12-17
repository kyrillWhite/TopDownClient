using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Wall : GameObject
    {
        public Wall(string texture, RectangleF rectangle = new RectangleF()) : base(texture, rectangle)
        {
            _layerDepth = 0.8f;
        }
        public Wall() : base() 
        {
            _layerDepth = 0.8f;
        }
    }
}
