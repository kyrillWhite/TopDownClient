using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public class Scene
    {
        protected MainGame _game;
        private Dictionary<string, UIObject> _uiObjects = new Dictionary<string, UIObject>();

        public Dictionary<string, UIObject> UiObjects { get => _uiObjects; set => _uiObjects = value; }

        public virtual void Initialize(MainGame game) 
        {
            _game = game;
        }

        public virtual void Update(MainGame game)
        {
            Mouse.SetCursor(MouseCursor.Arrow);
            foreach (var uiObject in UiObjects)
            {
                if (uiObject.Value.Visible && uiObject.Value.Rectangle.Contains(Control.GetMousePosition()))
                {
                    uiObject.Value.Hover();
                    if (Control.PrevLeftMouseState == ButtonState.Released &&
                        Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        uiObject.Value.Press();
                    }
                }
            }
            Control.Update();
        }
    }
}
