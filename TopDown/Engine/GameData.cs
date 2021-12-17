using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TopDown
{
    public static class GameData
    {
        private static Dictionary<string, Texture2D> _textures;
        private static SpriteFont _font;
        private static List<GameObject> _gameObjects;
        private static List<Label> _labels;
        private static Vector2 _windowSize;
        private static Vector2 _camera;
        private static float _scale = 1.0f;

        public static Dictionary<string, Texture2D> Textures { get => _textures; set => _textures = value; }
        public static SpriteFont Font { get => _font; set => _font = value; }
        public static List<GameObject> GameObjects { get => _gameObjects; set => _gameObjects = value; }
        public static List<Label> Labels { get => _labels; set => _labels = value; }
        public static Vector2 Camera { get => (_camera - WindowSize / 2) * GameData.Scale; set => _camera = value; }
        public static Vector2 WindowSize { get => _windowSize; set => _windowSize = value; }
        public static float Scale { get => _scale; set => _scale = value; }

        public static void Initialize(ContentManager contentManager)
        {
            _gameObjects = new List<GameObject>();
            _textures = LoadListContent<Texture2D>(contentManager, "textures");
            _labels = new List<Label>();
            _camera = Vector2.Zero; 
            _font = contentManager.Load<SpriteFont>("main");
        }

        public static void Clear()
        {
            GameObjects.Clear();
            _labels.Clear();
        }

        private static Dictionary<string, T> LoadListContent<T>(this ContentManager contentManager, string contentFolder)
        {
            var dir = new DirectoryInfo(contentManager.RootDirectory + "/" + contentFolder);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException();
            }
            var result = new Dictionary<String, T>();

            var files = dir.GetFiles("*.*");
            var directories = dir.GetDirectories();
            foreach (var directorie in directories)
            {
                var dirDic = LoadListContent<T>(contentManager, contentFolder + "/" + directorie.Name);
                result = result.Concat(dirDic).ToDictionary(x => x.Key, x => x.Value);
            }
            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file.Name);
                result[key] = contentManager.Load<T>(contentFolder + "/" + key);
            }
            return result;
        }
    }
}
