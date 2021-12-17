using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class UIObject : GameObject
    {
        private bool _visible;
        public delegate void UIHandler();
        public event UIHandler isPressed;
        public event UIHandler isHovered;

        public bool Visible { get => _visible; set => _visible = value; }

        public UIObject(string texture, bool visible, float layerDepth, RectangleF rectangle = new RectangleF()) : base(texture, rectangle)
        {
            _visible = visible;
            _layerDepth = layerDepth;
        }

        public void Press()
        {
            isPressed?.Invoke();
        }

        public void Hover()
        {
            isHovered?.Invoke();
        }
    }
}
