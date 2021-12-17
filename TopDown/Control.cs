using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace TopDown
{
    public static class Control
    {
        private static ButtonState _prevLeftMouseState = ButtonState.Released;
        private static bool _prevEscState = false;

        public static ButtonState PrevLeftMouseState { get => _prevLeftMouseState; set => _prevLeftMouseState = value; }
        public static bool PrevEscState { get => _prevEscState; set => _prevEscState = value; }

        public static void Update()
        {
            PrevLeftMouseState = Mouse.GetState().LeftButton;
            PrevEscState = Keyboard.GetState().IsKeyDown(Keys.Escape);
        }

        public static Vector2 GetMousePosition()
        {
            return Mouse.GetState().Position.ToVector2() / GameData.Scale;
        }
    }
}
