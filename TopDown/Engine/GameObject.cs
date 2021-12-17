using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TopDown
{
    public class GameObject
    {
        RectangleF _rectangle;
        string _texture = "";
        public float _layerDepth = 0.0f;

        public RectangleF Rectangle { get => _rectangle; set => _rectangle = value; }
        public string Texture { get => _texture; set => _texture = value; }

        public GameObject() { }
        public GameObject(string texture, RectangleF rectangle = new RectangleF())
        {
            _rectangle = rectangle;
            _texture = texture;
            GameData.GameObjects.Add(this);
        }

        public virtual void Draw(SpriteBatch spriteBranch)
        {
            var rect = (_rectangle * GameData.Scale).GetRectangle();
            if (!(this is UIObject)) {
                rect.Location -= GameData.Camera.ToPoint();
            }
            if (!(this is UIObject) || ((UIObject)this).Visible)
            {
                spriteBranch.Draw(GameData.Textures[_texture], rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, _layerDepth);
            }
        }

        public void Translate(Vector2 moveVector)
        {
            _rectangle += moveVector;
        }
    }
}
