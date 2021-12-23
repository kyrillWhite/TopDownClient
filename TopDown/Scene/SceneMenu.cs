using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TopDownGrpcClient;

namespace TopDown
{
    public class SceneMenu : Scene
    {
        private int _startGame = 0;

        private SceneGame gameScene = null;

        private Task<(string, string, List<(string, int, float, float)>)> StartGameTask = null;
        private CancellationTokenSource stopLoading;
        private Task TimeoutTask;
        public string playerId = null;

        private InfoLabel _label = new InfoLabel(new Vector2(10, 10), "0 : 0", Color.Red, 0.99f, true) { Text = "Trying to connect to server" };
        public InfoLabel GameErrorLabel = new InfoLabel(new Vector2(10, 20), "0 : 0", Color.Red, 0.99f, true) { Text = "Trying to connect to server" };

        public override void Initialize(MainGame game)
        {
            UiObjects.Add("label_loading", new UIObject("label_loading", true, 0.92f, new RectangleF(520, 280, 240, 60)));
            UiObjects.Add("button_cancel", new UIObject("button_cancel", true, 0.92f, new RectangleF(520, 430, 240, 60)));

            UiObjects["button_cancel"].isPressed += game.Exit;
            UiObjects["button_cancel"].isHovered += SetHandCursor;
            GameErrorLabel.Text = "";
            base.Initialize(game);
        }

        public override void Update(MainGame game)
        {
            if (_startGame > 1)
            {
                if (StartGameTask == null)
                {
                    stopLoading = new CancellationTokenSource();
                    stopLoading.CancelAfter(20*1000);
                    StartGameTask = Task.Run(()=> InitializeServerPart(stopLoading));
                }

                if (StartGameTask.IsCompleted)
                {
                    StartGame(StartGameTask.Result.Item1, StartGameTask.Result.Item2, StartGameTask.Result.Item3);
                    StartGameTask = null;
                }
            }
            _startGame++;
            base.Update(game);
        }
        private (string, string, List<(string, int, float, float)>) InitializeServerPart(CancellationTokenSource stopLoading)
        {
            while (stopLoading.IsCancellationRequested is false)
            {
                string mapXml = null;
                List<(string, int, float, float)> entities;
                try
                {
                    Messages.Initialize();

                    mapXml = Messages.GetMap();
                    if (string.IsNullOrEmpty(mapXml))
                    {
                        _label.Text = "Map was NullOrEmpty. Trying to get again in 3 sec";
                        Thread.Sleep(3000);
                        continue;
                    }


                    if (string.IsNullOrEmpty(playerId))
                    {
                        playerId = Messages.GetPlayerId();
                        if (string.IsNullOrEmpty(playerId))
                        {
                            _label.Text = "Cannot get player id. Probably server is full. You can now close this window";
                            Thread.Sleep(20*60*60*1000);
                            return (null, null, null);
                        }
                    }

                    entities = Messages.GetEntities(playerId);
                    if (entities.Count < 1)
                    {
                        _label.Text = "Server did not send entities. You can now close this window";
                        Thread.Sleep(20 * 60 * 60 * 1000);
                        return (null, null, null);
                    }

                    Task.Run(() => Messages.GetUpdate(playerId));
                }
                catch (Exception e)
                {
                    _label.Text = $"Error in connecting to server. See log.txt fro more detail. Trying again";
                    File.AppendAllText("log.txt", $"{DateTime.Now.ToString()}\n{e.ToString()}\n\n");
                    Thread.Sleep(3000);
                    continue;
                }

                return (mapXml, playerId, entities);
            }

            _label.Text = "Timeout trying connection. You can now close this window";
            Thread.Sleep(20 * 60 * 60 * 1000);
            return (null, null, null);
        }

        private void StartGame(string map, string playerId, List<(string, int, float, float)> entities)
        {
            GameData.Clear();
            var gameScene = new SceneGame();
            gameScene.InitializeUI();
            gameScene.InitializeMap(map, playerId, entities);
            _game._scene = gameScene;
            _game._scene.Initialize(_game);
        }

        private void SetHandCursor()
        {
            Mouse.SetCursor(MouseCursor.Hand);
        }
    }
}
