using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Label
    {
        private Vector2 _position;
        private string _text;
        private Color _color;
        private bool _visible;
        public float _layerDepth = 0.0f;

        public Vector2 Position { get => _position; set => _position = value; }
        public string Text { get => _text; set => _text = value; }
        public Color Color { get => _color; set => _color = value; }
        public bool Visible { get => _visible; set => _visible = value; }

        public Label(Vector2 position, string text, Color color, float layerDepth, bool visible)
        {
            _position = position;
            _text = text;
            _color = color;
            _layerDepth = layerDepth;
            GameData.Labels.Add(this);
            _visible = visible;
        }

        public void Draw(SpriteBatch spriteBranch)
        {
            if (_visible)
            {
                spriteBranch.DrawString(GameData.Font, _text, _position * GameData.Scale, _color, 0, Vector2.Zero, 1.0f, SpriteEffects.None, _layerDepth);
            }
        }
    }
}
