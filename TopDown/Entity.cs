using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Entity : GameObject
    {
        private int _team;
        private Vector2 _speed;
        private Circle _hitCircle;
        public Vector2 Speed { get => _speed; set => _speed = value; }
        public Circle HitCircle { get => _hitCircle + Rectangle.Location; set => _hitCircle = value; }
        public int Team { get => _team; set => _team = value; }

        public Entity(string texture, Circle hitCircle, RectangleF rectangle = new RectangleF(), Vector2 speed = new Vector2()) : base(texture, rectangle)
        {
            _hitCircle = hitCircle;
            _speed = speed;
        }

        public RectangleF MovedRectangle()
        {
            return Rectangle + _speed;
        }

        public void Move()
        {
            Translate(_speed / 60);
        }
    }
}
