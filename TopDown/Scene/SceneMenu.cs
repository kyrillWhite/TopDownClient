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
        private Task TimeoutTask;

        private Label _label = new Label(new Vector2(20, 20), "0 : 0", Color.Black, 0.99f, true) { Text = "Trying to connect to server" };

        //private Label c1;
        //private Label c2;
        public override void Initialize(MainGame game)
        {
            //UiObjects.Add("menu_plane", new UIObject("menu_plane", true, 0.91f, new RectangleF(440, 210, 400, 300)));
            UiObjects.Add("map_preview", new UIObject("map_preview", false, 0.91f, new RectangleF(20, 20, 200, 200)));
            //UiObjects.Add("button_play", new UIObject("button_play", true, 0.92f, new RectangleF(520, 230, 240, 60)));
            //UiObjects.Add("button_settings", new UIObject("button_settings", true, 0.92f, new RectangleF(520, 330, 240, 60)));
            //UiObjects.Add("button_exit", new UIObject("button_exit", true, 0.92f, new RectangleF(520, 430, 240, 60)));
            UiObjects.Add("label_searching", new UIObject("label_searching", false, 0.92f, new RectangleF(520, 280, 240, 60)));
            UiObjects.Add("button_cancel", new UIObject("button_cancel", false, 0.92f, new RectangleF(520, 430, 240, 60)));

            //UiObjects.Add("button_select1", new UIObject("button_select1", false, 0.92f, new RectangleF(480, 280, 240, 60)));
            //UiObjects.Add("button_select2", new UIObject("button_select2", false, 0.92f, new RectangleF(480, 380, 240, 60)));

            //UiObjects["button_play"].isPressed += Search;
            //UiObjects["button_cancel"].isPressed += CancelSearch;
            UiObjects["button_cancel"].isPressed += game.Exit;
            //UiObjects["button_exit"].isPressed += game.Exit;
            //UiObjects["button_select1"].isPressed += SelectTeam1;
            //UiObjects["button_select2"].isPressed += SelectTeam2;

            //UiObjects["button_play"].isHovered += SetHandCursor;
            //UiObjects["button_settings"].isHovered += SetHandCursor;
            //UiObjects["button_exit"].isHovered += SetHandCursor;
            UiObjects["button_cancel"].isHovered += SetHandCursor;
            //UiObjects["button_select1"].isHovered += SetHandCursor;
            //UiObjects["button_select2"].isHovered += SetHandCursor;

            //c1 = new Label(new Vector2(750, 285), "", Color.Black, 0.99f, false);
            //c2 = new Label(new Vector2(750, 385), "", Color.Black, 0.99f, false);

            UiObjects["label_searching"].Visible = true;
            UiObjects["button_cancel"].Visible = true;
            UiObjects["map_preview"].Visible = false;
            base.Initialize(game);
        }

        public override void Update(MainGame game)
        {
            //{
            // Ask server game session
            //if (true) // game found
            //{
            //    // Ask server players count
            //    var firstCommandCount = 0;
            //    var secondCommandCount = 0;
            //
            //    //c1.Visible = true;
            //    //c2.Visible = true;
            //    //c1.Text = $"{firstCommandCount}/5";
            //    //c2.Text = $"{secondCommandCount}/5";
            //
            //    UiObjects["label_searching"].Visible = false;
            //    UiObjects["button_cancel"].Visible = false; 
            //    UiObjects["button_select1"].Visible = true;
            //    UiObjects["button_select2"].Visible = true;
            //}
            //}
            if (_startGame > 1)
            {
                if (StartGameTask == null)
                {
                    StartGameTask = Task.Run(InitializeServerPart);
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

        //private void Search()
        //{
        //    //UiObjects["button_play"].Visible = false;
        //    //UiObjects["button_settings"].Visible = false;
        //    //UiObjects["button_exit"].Visible = false;
        //
        //    // Send request to server
        //    
        //}

        //private void CancelSearch()
        //{
        //    UiObjects["button_play"].Visible = true;
        //    UiObjects["button_settings"].Visible = true;
        //    UiObjects["button_exit"].Visible = true;
        //    UiObjects["label_searching"].Visible = false;
        //    UiObjects["button_cancel"].Visible = false;
        //}

        //private void SelectTeam1()
        //{
        //    // Send team to server
        //    StartGame();
        //}
        //
        //private void SelectTeam2()
        //{
        //    // Send team to server
        //    StartGame();
        //}

        public (string, string, List<(string, int, float, float)>) InitializeServerPart()
        {
            while (true)
            {
                string mapXml = null, playerId = null;
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
                    playerId = Messages.GetPlayerId();
                    if (string.IsNullOrEmpty(playerId))
                    {
                        _label.Text = "Cannot get player id. Probably server is full. You can now close this window";
                        Thread.Sleep(TimeSpan.MaxValue);
                        return (null, null, null);
                    }

                    entities = Messages.GetEntities();
                    if (entities.Count < 1)
                    {
                        _label.Text = "Server did not send entities. You can now close this window";
                        Thread.Sleep(TimeSpan.MaxValue);
                        return (null, null, null);
                    }

                    Task.Run(Messages.GetUpdate);
                }
                catch (Exception e)
                {
                    _label.Text = $"Error in connecting to server. See log.txt fro more detail. Trying again";
                    File.AppendAllText("log.txt", e.ToString());
                    Thread.Sleep(3000);
                    continue;
                }

                return (mapXml, playerId, entities);
            }
        }

        private void StartGame(string map, string playerId, List<(string, int, float, float)> entities)
        {
            GameData.Clear();
            var gameScene = new SceneGame();
            gameScene.InitializeUI();
            gameScene.InitializeMap(map, playerId, entities);
            _game._scene = gameScene;
            _game._scene.Initialize(_game);
            //UiObjects["label_searching"].Visible = false;
            //UiObjects["button_cancel"].Visible = false;
            //UiObjects["map_preview"].Visible = false;
        }

        private void SetHandCursor()
        {
            Mouse.SetCursor(MouseCursor.Hand);
        }
    }
}
