using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Bullet : Entity
    {
        private int _damage;
        private RectangleF _intersectingWall;
        private Vector2 _endPoint;
        private Vector2 _startPoint;
        public RectangleF IntersectingWall { get => _intersectingWall; set => _intersectingWall = value; }
        public Vector2 EndPoint { get => _endPoint; set => _endPoint = value; }
        public Vector2 StartPoint { get => _startPoint; set => _startPoint = value; }
        public int Damage { get => _damage; set => _damage = value; }

        public Bullet(
            string texture,
            Circle hitCircle,
            int damage,
            RectangleF intersectingWall = new RectangleF(),
            RectangleF rectangle = new RectangleF(),
            Vector2 speed = new Vector2()) : base(texture, hitCircle, rectangle, speed)
        {
            _damage = damage;
            _intersectingWall = intersectingWall;
            _layerDepth = 0.5f;
        }        
    }
}
