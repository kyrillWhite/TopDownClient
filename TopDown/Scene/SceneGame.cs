using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using System.IO;
using System.Xml.Serialization;
using TopDownGrpcClient;
using TopDownLibrary;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TopDown
{
    public class SceneGame : Scene
    {
        private Player _player = null;
        private string _playerId = "";
        private Map _map;
        private Dictionary<int, Bullet> _bullets = new Dictionary<int, Bullet>();
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private static Dictionary<string, List<(DateTime, Vector2)>> Positions = new Dictionary<string, List<(DateTime, Vector2)>>();
        private InfoLabel _infoLabel = new InfoLabel(new Vector2(10, 10), "0 : 0", Color.Red, 0.99f, true);
        private Label _score = new Label(new Vector2(20, 20), "0 : 0", Color.Black, 0.99f, true);
        private Label _timer = new Label(new Vector2(600, 20), "2:00", Color.Black, 0.99f, true);
        private Label _capacity = new Label(new Vector2(20, 600), "", Color.Black, 0.99f, true);
        private bool _pause = false;
        private bool _startRound = true;
        private bool _exit = false;
        private bool _endGame = false;
        private Dictionary<int, Input> _inputDict = new Dictionary<int, Input>();
        private int _inputId = 0;
        private int _lastSendedInputId = 0;
        private int _lastBulletId = 0;
        private int _currentRound = -1;

        public Player Player { get => _player; set => _player = value; }
        public Map Map { get => _map; set => _map = value; }

        public override void Initialize(MainGame game)
        {
            base.Initialize(game);
        }

        public void InitializeUI()
        {
            UiObjects.Add("score_plane", new UIObject("score_plane", true, 0.91f, new RectangleF(8, 20, 100, 50)));
            UiObjects.Add("time_plane", new UIObject("time_plane", true, 0.91f, new RectangleF(565, 20, 150, 50)));
            UiObjects.Add("menu_plane", new UIObject("menu_plane", false, 0.91f, new RectangleF(440, 210, 400, 300)));
            UiObjects.Add("button_continue", new UIObject("button_continue", false, 0.93f, new RectangleF(520, 230, 240, 60)));
            UiObjects.Add("button_settings", new UIObject("button_settings", false, 0.93f, new RectangleF(520, 330, 240, 60)));
            UiObjects.Add("button_exit", new UIObject("button_exit", false, 0.93f, new RectangleF(520, 430, 240, 60)));
            UiObjects.Add("label_1_win", new UIObject("label_1_win", false, 0.93f, new RectangleF(520, 230, 240, 60)));
            UiObjects.Add("label_2_win", new UIObject("label_2_win", false, 0.93f, new RectangleF(520, 230, 240, 60)));
            UiObjects.Add("button_ok", new UIObject("button_ok", false, 0.93f, new RectangleF(520, 430, 240, 60)));

            UiObjects.Add("sniper_rifle_image", new UIObject("sniper_rifle_image", false, 0.92f, new RectangleF(470, 310, 100, 100)));
            UiObjects.Add("rifle_image", new UIObject("rifle_image", false, 0.92f, new RectangleF(590, 310, 100, 100)));
            UiObjects.Add("shotgun_image", new UIObject("shotgun_image", false, 0.92f, new RectangleF(710, 310, 100, 100)));

            UiObjects.Add("hp_bar_mask", new UIObject("hp_bar_mask", true, 0.93f, new RectangleF(20, 670, 300, 30)));
            UiObjects.Add("hp_bar", new UIObject("hp_bar", true, 0.92f, new RectangleF(20, 670, 300, 30)));
            UiObjects.Add("reload_bar", new UIObject("reload_bar", false, 0.92f, new RectangleF(20, 640, 300, 10)));

            UiObjects["button_continue"].isPressed += Continue;
            UiObjects["button_exit"].isPressed += Exit;
            UiObjects["button_ok"].isPressed += Exit;
            UiObjects["sniper_rifle_image"].isPressed += ChooseGun1;
            UiObjects["rifle_image"].isPressed += ChooseGun2;
            UiObjects["shotgun_image"].isPressed += ChooseGun3;

            UiObjects["button_continue"].isHovered += SetHandCursor;
            UiObjects["button_settings"].isHovered += SetHandCursor;
            UiObjects["button_exit"].isHovered += SetHandCursor;
            UiObjects["button_ok"].isHovered += SetHandCursor;
            UiObjects["sniper_rifle_image"].isHovered += SetHandCursor;
            UiObjects["rifle_image"].isHovered += SetHandCursor;
            UiObjects["shotgun_image"].isHovered += SetHandCursor;

            _infoLabel.Text = "";
        }

        public void InitializeMap(string map, string playerId, List<(string, int, float, float)> entities)
        {
            //_map = new Map("C:/Users/kirill/Desktop/Map/map.txt", 2);
            //SaveMap("map1.xml");
            LoadMap(map);
            _playerId = playerId;
            InitialiseEntities(entities);
            Messages.RetrieveUpdateEvent += ServerUpdate;
            Messages.PlayerDataEvent += UpdatePlayerState;

            Messages.CanUpdate = true;
            Messages.CanUpdateToken.Cancel();
        }

        public override void Update(MainGame game)
        {
            if (_exit)
            {
                game.Exit();
            }

            if (Messages.Exception != null)
            {
                if (string.IsNullOrEmpty(_infoLabel.Text))
                {
                    File.AppendAllText("log.txt", $"{DateTime.Now.ToString()}\n{Messages.Exception.Message}\n\n");
                }
                _infoLabel.Text = "Connection error. See log.txt";

                CloseWithError("Connection error. See log.txt");
            }
            else
            {

                if (!_endGame && !_pause && !_startRound)
                {
                    MoveControl();
                }

                lock (_inputDict)
                {
                    if (!_pause)
                    {
                        Messages.SendControlState(_inputDict.Where(i => i.Key > _lastSendedInputId)
                            .ToDictionary(i => i.Key, i => i.Value), _playerId);
                    }
                    else
                    {
                        Messages.SendControlState(new Dictionary<int, Input>(), _playerId);
                    }
                    _lastSendedInputId = _inputDict.Count == 0 ? 0 : _inputDict.Max(i => i.Key);
                }
                lock (Positions)
                {
                    UpdateEntitiesPositionsLocal();
                }
                lock (_bullets)
                {
                    foreach (var bullet in _bullets)
                    {
                        bullet.Value.Move();
                    }
                    DeleteBulletsLocal();
                }


                lock (Player)
                {
                    var mPos = Control.GetMousePosition();
                    var cmPos = mPos - GameData.WindowSize / 2;
                    cmPos.X = cmPos.X > GameData.WindowSize.X / 2 ? GameData.WindowSize.X / 2 : cmPos.X;
                    cmPos.X = cmPos.X < -GameData.WindowSize.X / 2 ? -GameData.WindowSize.X / 2 : cmPos.X;
                    cmPos.Y = cmPos.Y > GameData.WindowSize.Y / 2 ? GameData.WindowSize.Y / 2 : cmPos.Y;
                    cmPos.Y = cmPos.Y < -GameData.WindowSize.Y / 2 ? -GameData.WindowSize.Y / 2 : cmPos.Y;
                    GameData.Camera = Player.Rectangle.Center + cmPos * Constants.MaxCameraOffset;
                }


                // Server code: check team win round
                //if (!_finalScore)
                //{
                //    var roundEnd = false;
                //    if (!_players.Any(p => p.Value.Team == 2))
                //    {
                //        _rounds[0]++;
                //        roundEnd = true;
                //    }
                //    if (!_players.Any(p => p.Value.Team == 1))
                //    {
                //        _rounds[1]++;
                //        roundEnd = true;
                //    }
                //    if (_roundTime > Constants.RoundTime)
                //    {
                //        roundEnd = true;
                //        var t1Count = _players.Where(p => p.Value.Team == 1).Count();
                //        var t2Count = _players.Where(p => p.Value.Team == 2).Count();
                //        if (t1Count != t2Count)
                //        {
                //            _rounds[t1Count > t2Count ? 0 : 1]++;
                //        }
                //    }
                //    if (roundEnd)
                //    {
                //        _allRounds++;
                //        _score.Text = $"{_rounds[0]} : {_rounds[1]}";
                //        if (_allRounds < Constants.RoundsCount)
                //        {
                //            InitializeRound(false);
                //        }
                //        else
                //        {
                //            InitializeRound(true);
                //            _pause = true;
                //            _finalScore = true;
                //            FinalScore();
                //        }
                //    }
                //}

                ////////////////////////
                ///Send reinitialize command + new score
            }

            ESCPress();

            base.Update(game);
        }
        private void InitializeRound(bool onlyClear)
        {
            Player = _players[_playerId];
            _startRound = true;
            foreach (var bullet in _bullets)
            {
                GameData.GameObjects.Remove(bullet.Value);
            }
            _bullets.Clear();
            if (onlyClear)
            {
                Player.Speed = Vector2.Zero;
                return;
            }
            _players.ToList().ForEach(p => p.Value.IsDead = false);
            ShowWeaponChoose();
        }

        private void InitialiseEntities(List<(string, int, float, float)> entities)
        {
            foreach (var entityPos in entities)
            {
                Player entity = CreatePlayer(new Vector2(entityPos.Item3, entityPos.Item4), entityPos.Item2);
                _players.Add(entityPos.Item1, entity);
                Positions.Add(entityPos.Item1, new List<(DateTime, Vector2)>() { (DateTime.Now, entity.Rectangle.Min) });
            }
            InitializeRound(false);
        }

        private void ServerUpdate(RetrieveUpdateEventArgs e)
        {
            UpdateEntitiesPositions(e);
            lock (_bullets)
            {
                CreateBullets(e);
            }
            UpdateRoundData(e);

        }
        private void UpdateRoundData(RetrieveUpdateEventArgs e)
        {
            _score.Text = $"{e.FirstTeamScore} : {e.SecondTeamScore}";
            if (e.IsEndGame)
            {
                FinalScore(e.FirstTeamScore, e.SecondTeamScore);
                _endGame = true;
                return;
            }
            if (e.CurrentRound != _currentRound)
            {
                // New round
                _currentRound++;
                _startRound = true;
                InitializeRound(false);
            }
            if (e.RoundTimeLeft.TotalSeconds >= Constants.RoundTime - 5)
            {
                _timer.Text = (TimeSpan.FromSeconds(5 - Constants.RoundTime) + e.RoundTimeLeft).ToString("m\\:ss");
            }
            else
            {
                if (_startRound)
                {
                    _startRound = false;
                    CloseWeaponChoose();
                }
                _timer.Text = e.RoundTimeLeft.ToString("m\\:ss");
            }
        }

        private void UpdateEntitiesPositions(RetrieveUpdateEventArgs e)
        {
            lock (Positions)
            {
                lock (_players)
                {
                    foreach (var entityPos in e.EntityPositions)
                    {
                        if (_players.ContainsKey(entityPos.Item1))
                        {
                            Player entity = _players[entityPos.Item1];
                            if (entity != Player)
                            {
                                lock (GameData.GameObjects)
                                {
                                    _players[entityPos.Item1].IsDead = entityPos.Item5;
                                }
                                if (entityPos.Item5)
                                {
                                    continue;
                                }
                                Positions[entityPos.Item1].Add((DateTime.Now, new Vector2(entityPos.Item3, entityPos.Item4)));
                                Positions[entityPos.Item1].RemoveAll(p => (DateTime.Now - p.Item1).Seconds > 10);
                            }
                            else if (_startRound)
                            {
                                entity.Rectangle = entity.Rectangle + new Vector2(entityPos.Item3, entityPos.Item4) - entity.Rectangle.Min;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateEntitiesPositionsLocal()
        {
            foreach (var player in _players)
            {
                if (player.Value != Player)
                {
                    var showTime = DateTime.Now.AddMilliseconds(-Constants.BufferDelayInterval);
                    var fP = Positions[player.Key].LastOrDefault(p => p.Item1 < showTime);
                    var sP = Positions[player.Key].FirstOrDefault(p => p.Item1 > showTime);

                    if (fP.Item1 == DateTime.MinValue)
                    {
                        fP = sP;
                    }
                    if (sP.Item1 == DateTime.MinValue)
                    {
                        sP = fP;
                    }

                    var ratio = (float)(sP.Item1 - fP.Item1).Ticks / (float)(showTime - fP.Item1).Ticks;
                    if (ratio != 0)
                    {
                        var ttt = 0;
                    }
                    var newPosInter = ratio == 0 ? fP.Item2 : (sP.Item2 - fP.Item2) / ratio + fP.Item2;
                    player.Value.Rectangle = player.Value.Rectangle + newPosInter - player.Value.Rectangle.Min;
                }
            }
        }

        private void UpdatePlayerState(PlayerDataEventArgs e)
        {
            lock (Player)
            {
                if (Player == null)
                {
                    if (!_players.ContainsKey(e.Id))
                    {
                        return;
                    }
                    Player = _players[e.Id];
                }
                Player.Rectangle = Player.Rectangle + new Vector2(e.X, e.Y) - Player.Rectangle.Min;
            }
            lock (_inputDict)
            {
                var deletedInputs = new List<int>();
                foreach (var input in _inputDict.ToList().OrderBy(i => i.Key))
                {
                    if (input.Key <= e.LastId)
                    {
                        deletedInputs.Add(input.Key);
                    }
                    else
                    {
                        var dirrection = Vector2.Zero;
                        dirrection.X += input.Value.DirX;
                        dirrection.Y += input.Value.DirY;
                        if (dirrection.X != 0 && dirrection.Y != 0)
                        {
                            dirrection.Normalize();
                        }
                        lock (Player)
                        {
                            Player.Rectangle += dirrection * Constants.MaxMoveSpeed;
                            FixCollision();
                        }
                    }
                }
                deletedInputs.ForEach(inid => _inputDict.Remove(inid));
            }
            var hpRect = UiObjects["hp_bar"].Rectangle;
            UiObjects["hp_bar"].Rectangle = new RectangleF(hpRect.Min,
                new Vector2(hpRect.Min.X + Constants.HpBarWidth * e.HpPercent, hpRect.Max.Y));

            _capacity.Text = $"{e.BulletsCount}/{e.Capacity}";
            if (e.ReloadPercent != 0)
            {
                UiObjects["reload_bar"].Visible = true;
                var rRect = UiObjects["reload_bar"].Rectangle;
                UiObjects["reload_bar"].Rectangle = new RectangleF(rRect.Min,
                    new Vector2(rRect.Min.X + Constants.HpBarWidth * Math.Min(e.ReloadPercent, 1), rRect.Max.Y));
                if (e.ReloadPercent >= 1)
                {
                    UiObjects["reload_bar"].Visible = false;
                    _capacity.Text = $"{e.Capacity}/{e.Capacity}";
                }
            }
            else
            {
                UiObjects["reload_bar"].Visible = false;
            }
        }

        private void FixCollision()
        {
            var interWalls = Map._walls.FindAll(w => Player.HitCircle.Intersects(w.Rectangle));
            foreach (var interWall in interWalls)
            {
                var potentCircPos = Player.HitCircle.Location;
                var nearestPoint = new Vector2(
                    Math.Max(interWall.Rectangle.Min.X, Math.Min(potentCircPos.X, interWall.Rectangle.Max.X)),
                    Math.Max(interWall.Rectangle.Min.Y, Math.Min(potentCircPos.Y, interWall.Rectangle.Max.Y)));
                var rayToNearest = nearestPoint - potentCircPos;
                var overlap = Player.HitCircle.Radius - rayToNearest.Length();
                if (float.IsNaN(overlap))
                {
                    overlap = 0;
                }
                if (overlap > 0)
                {
                    rayToNearest.Normalize();
                    if (float.IsNaN(rayToNearest.X) || float.IsNaN(rayToNearest.Y))
                    {
                        rayToNearest = Vector2.Zero;
                    }
                    Player.Rectangle += (-rayToNearest * overlap);
                }
            }
        }

        private Player CreatePlayer(Vector2 position, int team)
        {
            return new Player(team == 2 ? "enemy_entity" : "friendly_entity",
                new Circle(new Vector2(Constants.EntitySize.X / 2, Constants.EntitySize.Y - Constants.EntitySize.X / 2),
                Constants.EntitySize.X / 2),
                new RectangleF(Vector2.Zero, Constants.EntitySize) + position)
            { Team = team };
        }

        private void CreateBullets(RetrieveUpdateEventArgs e)
        {
            if (e.Bullets.Count == 0)
            {
                return;
            }
            var newBullets = e.Bullets.Where(b => b.Id >= _lastBulletId).ToList();
            _lastBulletId = e.Bullets.Max(b => b.Id) + 1;
            foreach (var bulletData in newBullets)
            {
                var _from = new Vector2(bulletData.StartPosX, bulletData.StartPosY);
                var _to = new Vector2(bulletData.EndPosX, bulletData.EndPosY);
                var shootDir = Vector2.Normalize(_to - _from);
                var startShootPos = _from; //+ shootDir * bulletData.Speed * (float)(DateTime.Now - bulletData.CreationTime).TotalSeconds;
                var bullet = new Bullet(
                    Player.Team == bulletData.Team ? "friendly_bullet" : "enemy_bullet",
                    new Circle(new Vector2(Constants.BulletSize / 2), Constants.BulletSize / 2),
                    new RectangleF(startShootPos, startShootPos + new Vector2(Constants.BulletSize)) - new Vector2(Constants.BulletSize / 2),
                    shootDir * bulletData.Speed
                    );
                bullet.StartPoint = startShootPos;
                bullet.EndPoint = _to;
                bullet.IntersectingWall = GetIntersectingWall(startShootPos, _to);
                bullet.Team = bulletData.Team;
                _bullets.Add(bulletData.Id, bullet);
            }
        }

        private RectangleF GetIntersectingWall(Vector2 startShootPos, Vector2 endShootPos)
        {
            var intersectedWall = RectangleF.Empty;
            var intersectedWalls = Map._walls.FindAll(wall => wall.Rectangle.Intersects(startShootPos, endShootPos)).ToList();
            if (intersectedWalls.Count != 0)
            {
                float lsMin = float.MaxValue;
                foreach (var interWall in intersectedWalls)
                {
                    var ls = (interWall.Rectangle.Location - startShootPos).LengthSquared();
                    if (ls < lsMin)
                    {
                        lsMin = ls;
                        intersectedWall = interWall.Rectangle;
                    }
                }
            }
            return intersectedWall;
        }

        private void DeleteBulletsLocal()
        {
            var removedBulletsId = new List<int>(
                _bullets.Where(b => b.Value.HitCircle.Intersects(b.Value.IntersectingWall) ||
                Vector2.Dot(Vector2.Normalize(b.Value.HitCircle.Location - b.Value.EndPoint),
                Vector2.Normalize(b.Value.StartPoint - b.Value.EndPoint)) <= 0.0F)
                .ToDictionary(b => b.Key).Keys);


            var interPlayersAndBullets = (from p in _players
                                          from b in _bullets
                                          where p.Value.Team != b.Value.Team && !p.Value.IsDead &&
                                          p.Value.HitCircle.Intersects(b.Value.HitCircle)
                                          select b.Key).ToList();

            removedBulletsId.AddRange(interPlayersAndBullets);
            removedBulletsId = removedBulletsId.Distinct().ToList();
            removedBulletsId.ForEach(bId => { GameData.GameObjects.Remove(_bullets[bId]); _bullets.Remove(bId); });
        }

        public void LoadMap(string mapXml)
        {
            var formatter = new XmlSerializer(typeof(Map));
            using (var reader = new StringReader(mapXml))
            {
                Map = (Map)formatter.Deserialize(reader);
            }
            GameData.GameObjects.AddRange(Map._grounds);
            GameData.GameObjects.AddRange(Map._walls);
        }

        public void SaveMap(string name)
        {
            var formatter = new XmlSerializer(typeof(Map));
            using (FileStream fs = new FileStream(name, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Map);
            }
        }

        private void ESCPress()
        {
            if (!Control.PrevEscState && Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                _pause = !_pause;
                if (_pause)
                {
                    if (_startRound)
                    {
                        CloseWeaponChoose();
                    }
                    Player.Speed = Vector2.Zero;
                    UiObjects["menu_plane"].Visible = true;
                    UiObjects["button_continue"].Visible = true;
                    UiObjects["button_settings"].Visible = true;
                    UiObjects["button_exit"].Visible = true;
                }
                else
                {
                    UiObjects["menu_plane"].Visible = false;
                    UiObjects["button_continue"].Visible = false;
                    UiObjects["button_settings"].Visible = false;
                    UiObjects["button_exit"].Visible = false;
                    if (_startRound)
                    {
                        ShowWeaponChoose();
                    }
                }
            }
        }

        private void MoveControl()
        {
            var dirrection = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.W) ||
                Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                dirrection.Y -= 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) ||
                Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                dirrection.Y += 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A) ||
                Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                dirrection.X -= 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) ||
                Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                dirrection.X += 1.0f;
            }
            if (dirrection.X != 0 && dirrection.Y != 0)
            {
                dirrection.Normalize();
            }

            lock (Player)
            {
                var mgPos = Control.GetMousePosition() + GameData.Camera;
                lock (_inputDict)
                {
                    _inputDict.Add(_inputId, new Input()
                    {
                        DirX = dirrection.X == 0 ? 0 : (dirrection.X > 0 ? 1 : -1),
                        DirY = dirrection.Y == 0 ? 0 : (dirrection.Y > 0 ? 1 : -1),
                        GlobalMousePosX = mgPos.X,
                        GlobalMousePosY = mgPos.Y,
                        LeftMouse = Mouse.GetState().LeftButton == ButtonState.Pressed,
                        RightMouse = Mouse.GetState().RightButton == ButtonState.Pressed,
                    });
                }
            }
            _inputId++;
            lock (Player)
            {
                Player.Rectangle += dirrection * Constants.MaxMoveSpeed;
            }
            //Player.Speed = dirrection * Constants.MaxMoveSpeed;
        }



        private void FinalScore(int score1, int score2)
        {
            if (score1 > score2)
            {
                UiObjects["label_1_win"].Visible = true;
            }
            else
            {
                UiObjects["label_2_win"].Visible = true;
            }
            UiObjects["menu_plane"].Visible = true;
            UiObjects["button_ok"].Visible = true;
        }

        private void ShowWeaponChoose()
        {
            if (!_pause)
            {
                UiObjects["menu_plane"].Visible = true;
                UiObjects["rifle_image"].Visible = true;
                UiObjects["sniper_rifle_image"].Visible = true;
                UiObjects["shotgun_image"].Visible = true;
            }
        }

        private void CloseWeaponChoose()
        {
            if (!_pause)
            {
                UiObjects["menu_plane"].Visible = false;
            }
            UiObjects["rifle_image"].Visible = false;
            UiObjects["sniper_rifle_image"].Visible = false;
            UiObjects["shotgun_image"].Visible = false;
        }

        private void ChooseGun1()
        {
            _player.GunType = 1;
            Messages.SendGun(_playerId, 1);
            CloseWeaponChoose();
            // Send to server
        }

        private void ChooseGun2()
        {
            _player.GunType = 2;
            Messages.SendGun(_playerId, 2);
            CloseWeaponChoose();
            // Send to server
        }

        private void ChooseGun3()
        {
            _player.GunType = 3;
            Messages.SendGun(_playerId, 3);
            CloseWeaponChoose();
            // Send to server
        }

        private void Continue()
        {
            UiObjects["menu_plane"].Visible = false;
            UiObjects["button_continue"].Visible = false;
            UiObjects["button_settings"].Visible = false;
            UiObjects["button_exit"].Visible = false;
            _pause = false;
        }

        private void CloseWithError(string err)
        {
            Messages.RetrieveUpdateEvent -= ServerUpdate;
            Messages.PlayerDataEvent -= UpdatePlayerState;

            GameData.Clear();
            Positions.Clear();
            _players.Clear();
            _bullets.Clear();
            _inputDict.Clear();
            _game._scene = new SceneMenu();
            _game._scene.Initialize(_game);
            ((SceneMenu)_game._scene).GameErrorLabel.Text = err;
            ((SceneMenu)_game._scene).playerId = _playerId;
        }

        private void Exit()
        {
            Messages.RetrieveUpdateEvent -= ServerUpdate;
            Messages.PlayerDataEvent -= UpdatePlayerState;

            GameData.Clear();
            Positions.Clear();
            _players.Clear();
            _bullets.Clear();
            _inputDict.Clear();
            GameData.Clear();
            _exit = true;
        }
        private void SetHandCursor()
        {
            Mouse.SetCursor(MouseCursor.Hand);
        }
    }
}
