using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Bullet : Entity
    {
        private Vector2 _endPoint;
        private Vector2 _startPoint;
        public Vector2 EndPoint { get => _endPoint; set => _endPoint = value; }
        public Vector2 StartPoint { get => _startPoint; set => _startPoint = value; }
        public RectangleF IntersectingWall { get; set; }

        public Bullet(
            string texture,
            Circle hitCircle,
            RectangleF rectangle = new RectangleF(),
            Vector2 speed = new Vector2()) : base(texture, hitCircle, rectangle, speed)
        {
            _layerDepth = 0.5f;
        }        
    }
}
