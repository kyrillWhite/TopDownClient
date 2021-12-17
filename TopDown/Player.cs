﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Player : Entity
    {
        private int _hp = Constants.PlayerMaxHp;
        private int _gunType;
        private int _curBulletsCount;
        public int Hp { get => _hp; set => _hp = value; }
        public int GunType { get => _gunType; set => _gunType = value; }
        public int CurBulletsCount { get => _curBulletsCount; set => _curBulletsCount = value; }

        public Player(
            string texture,
            Circle hitCircle,
            RectangleF rectangle = new RectangleF(),
            Vector2 speed = new Vector2()) : base(texture, hitCircle, rectangle, speed)
        {
            var rand = new Random();
            _gunType = rand.Next(1, 4);
            _layerDepth = 0.9f;
        }
    }
}
