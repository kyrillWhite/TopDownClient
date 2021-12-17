﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using System.IO;
using System.Xml.Serialization;

namespace TopDown
{
    public class SceneGame : Scene
    {

        private Player _player;
        private Map _map; // To Server
        private Dictionary<string, Bullet> _bullets = new Dictionary<string, Bullet>(); // To Server
        private Dictionary<string, Player> _players = new Dictionary<string, Player>(); // To Server
        private List<int> _rounds = new List<int>(new[]{0, 0}); // Only to server
        private int _allRounds = 0; // Only to server
        private Label _score = new Label(new Vector2(20, 20), "0 : 0", Color.Black, 0.99f, true);
        private Label _timer = new Label(new Vector2(600, 20), "2:00", Color.Black, 0.99f, true);
        private Label _capacity = new Label(new Vector2(20, 600), "", Color.Black, 0.99f, true);
        private bool _pause = false;
        private bool _start = true;
        private bool _dead = false;
        private bool _finalScore = false;
        private bool _reload = false;
        private double _roundTime = 0.0f; // To server
        private double _startReloadTime = 0.0f; // Only to server
        private double _lastShotTime = 0.0f; // Only to server
        private Gun _gun;

        public Player Player { get => _player; set => _player = value; }
        public Map Map { get => _map; set => _map = value; }

        public override void Initialize(MainGame game)
        {
            LoadMap("map1.xml");
            //_map = new Map("C:/Users/kirill/Desktop/Map/map.txt", 2);
            //SaveMap("map1.xml");

            UiObjects.Add("score_plane", new UIObject("score_plane", true, 0.91f, new RectangleF(8, 20, 100, 50)));
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

            InitializeRound(false);
            base.Initialize(game);
        }

        public override void Update(MainGame game)
        {

            // Server code: time count
            if (!_finalScore)
            {
                _roundTime += game.TargetElapsedTime.TotalSeconds;
            }
            if (_start && _roundTime >= Constants.StartTime)
            {
                _roundTime = 0;
                _start = false;
                _gun = new Gun(_player.GunType);
                _player.CurBulletsCount = _gun.Capacity;
                _capacity.Text = $"{_player.CurBulletsCount}/{_gun.Capacity}";
                CloseWeaponChoose();
            }
            //////////////////////////
            // Get round time from server // get end of start

            if (_start)
            {
                _timer.Text = TimeSpan.FromSeconds(Constants.StartTime - _roundTime).ToString("m\\:ss");
            }
            else
            {
                _timer.Text = TimeSpan.FromSeconds(Constants.RoundTime - _roundTime).ToString("m\\:ss");
            }

            if (!_pause && !_start)
            {
                MoveControl();
            }

            // Send "change position" message to server (send speed)
            // Server code: check intersection //
            Player.Move();
            var moveToCorrect = Vector2.Zero;
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
                    moveToCorrect += -rayToNearest * overlap;
                    Player.Translate(-rayToNearest * overlap);
                }
            }
            // Send change position from server (send moveToCorrect)
            ///////////////////////////////
            // Get positions of players

            var mPos = Control.GetMousePosition();
            var cmPos = mPos - GameData.WindowSize / 2;
            GameData.Camera = Player.Rectangle.Center + cmPos * Constants.MaxCameraOffset;

            if (!_pause && !_dead && !_start)
            {
                var newBullets = ShootControl();
                foreach(var newBullet in newBullets)
                {
                    _bullets.Add(Guid.NewGuid().ToString(), newBullet);
                    // Send new bullets to server (id, speed, start position, end position, wall)
                }
            }
            // Test shooting from bot
            if (!_pause && !_start)
            {
                var shotingPlayer = _players.FirstOrDefault(p => p.Value.Team == 2).Value;
                if (shotingPlayer != null)
                {
                    var _newBullet = Shoot(shotingPlayer.Rectangle.Center, shotingPlayer.Rectangle.Center - Vector2.UnitX, 0, true);
                    _newBullet.Team = 2;
                    _bullets.Add(Guid.NewGuid().ToString(), _newBullet);
                }
            }
            // Get new bullets from server

            // Server code: find remove bullets
            var removedBulletsId = new List<string>( // End distance or hit to wall
                _bullets.Where(b => b.Value.HitCircle.Intersects(b.Value.IntersectingWall) ||
                Vector2.Dot(Vector2.Normalize(b.Value.HitCircle.Location - b.Value.EndPoint),
                Vector2.Normalize(b.Value.StartPoint - b.Value.EndPoint)) <= 0.0F)
                .ToDictionary(b => b.Key).Keys);


            var interPlayersAndBullets = (from p in _players
                                          from b in _bullets
                                          where p.Value.Team != b.Value.Team &&
                                          p.Value.HitCircle.Intersects(b.Value.HitCircle)
                                          group (p.Key, b.Key) by b.Key into pb
                                          select pb.First()).ToList();
            interPlayersAndBullets.ForEach(pb => { _players[pb.Item1].Hp -= _bullets[pb.Item2].Damage;});

            removedBulletsId.AddRange(interPlayersAndBullets.Select(pb => pb.Item2));
            var deadPlayersId = new List<string>(_players.Where(p => p.Value.Hp <= 0).ToDictionary(p => p.Key).Keys);
            ///////////////////
            // Get removed bullets from server
            removedBulletsId = removedBulletsId.Distinct().ToList();
            removedBulletsId.ForEach(bId => { GameData.GameObjects.Remove(_bullets[bId]); _bullets.Remove(bId); });
            // Get dead players from server
            deadPlayersId.ForEach(pId => { GameData.GameObjects.Remove(_players[pId]); _players.Remove(pId); });
            if (!_players.ContainsValue(Player))
            {

                _dead = true;
                // Set to Player texture of Ghost
            }
            // Get new HP from server
            var hpRect = UiObjects["hp_bar"].Rectangle;
            UiObjects["hp_bar"].Rectangle = new RectangleF(hpRect.Min,
                new Vector2(hpRect.Min.X + Constants.HpBarWidth * (float)_player.Hp / Constants.PlayerMaxHp, hpRect.Max.Y));

            // Server code: check team win round
            if (!_finalScore)
            {
                var roundEnd = false;
                if (!_players.Any(p => p.Value.Team == 2))
                {
                    _rounds[0]++;
                    roundEnd = true;
                }
                if (!_players.Any(p => p.Value.Team == 1))
                {
                    _rounds[1]++;
                    roundEnd = true;
                }
                if (_roundTime > Constants.RoundTime)
                {
                    roundEnd = true;
                    var t1Count = _players.Where(p => p.Value.Team == 1).Count();
                    var t2Count = _players.Where(p => p.Value.Team == 2).Count();
                    if (t1Count != t2Count)
                    {
                        _rounds[t1Count > t2Count ? 0 : 1]++;
                    }
                }
                if (roundEnd)
                {
                    _allRounds++;
                    _score.Text = $"{_rounds[0]} : {_rounds[1]}";
                    if (_allRounds < Constants.RoundsCount)
                    {
                        InitializeRound(false);
                    }
                    else
                    {
                        InitializeRound(true);
                        _pause = true;
                        _finalScore = true;
                        FinalScore();
                    }
                }
            }
            
            ////////////////////////
            ///Send reinitialize command + new score

            foreach (var bullet in _bullets)
            {
                bullet.Value.Move();
            }

            ESCPress();
            base.Update(game);
        }

        private void InitializeRound(bool onlyClear)
        {
            // Get players from server (id, position, command)
            // Server code: create random position on spawn zone
            _start = true;
            _roundTime = 0;
            foreach (var bullet in _bullets)
            {
                GameData.GameObjects.Remove(bullet.Value);
            }
            _bullets.Clear();
            foreach (var player in _players)
            {
                GameData.GameObjects.Remove(player.Value);
            }
            _players.Clear(); // This also for client
            if (onlyClear)
            {
                Player.Speed = Vector2.Zero;
                return;
            }

            for (int i = 0; i < 1; i++)
            {
                var rand = new Random();
                var fZone = Map._spawnZones[0];
                var sZone = Map._spawnZones[1];
                var x = (float)(fZone.X + rand.NextDouble() * fZone.Width);
                var y = (float)(fZone.Y + rand.NextDouble() * fZone.Height);

                _players.Add(Guid.NewGuid().ToString(), CreatePlayer(new Vector2(x, y), 1));
                x = (float)(sZone.X + rand.NextDouble() * sZone.Width);
                y = (float)(sZone.Y + rand.NextDouble() * sZone.Height);
                _players.Add(Guid.NewGuid().ToString(), CreatePlayer(new Vector2(x, y), 2));
            }
            //////////////////////////
            // Get players from server
            Player = _players.First().Value;
            /////////////////////////
            ShowWeaponChoose();
            _lastShotTime = 0;
            _startReloadTime = 0;
            _capacity.Text = "";
            _dead = false;
        }

        private Player CreatePlayer(Vector2 position, int team)
        {
            return new Player(team == 2 ? "enemy_entity" : "friendly_entity",
                new Circle(new Vector2(Constants.EntitySize.X / 2, Constants.EntitySize.Y - Constants.EntitySize.X / 2),
                Constants.EntitySize.X / 2),
                new RectangleF(Vector2.Zero, Constants.EntitySize) + position)
            { Team = team};
        }

        public void LoadMap(string path)
        {
            var formatter = new XmlSerializer(typeof(Map));
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Map = (Map)formatter.Deserialize(fs);
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
                    if (_start)
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
                    if (_start)
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
            if (dirrection.X != 0 && dirrection.Y != 0) {
                dirrection.Normalize();
            }
            Player.Speed = dirrection * Constants.MaxMoveSpeed;
        }
    
        private List<Bullet> ShootControl()
        {
            var bullets = new List<Bullet>();
            // Get count of bullet
            if (_player.CurBulletsCount != 0 && Mouse.GetState().LeftButton == ButtonState.Pressed) 
            {
                if (_roundTime - _lastShotTime >= _gun.ShootDelay)
                {
                    var mPos = Control.GetMousePosition();
                    _player.CurBulletsCount--;
                    _capacity.Text = $"{_player.CurBulletsCount}/{_gun.Capacity}";
                    _lastShotTime = _roundTime;
                    var angle = 0.0f;
                    var bCount = _player.GunType == 3 ? 8 : 1;
                    var rand = new Random();
                    for (int i = 0; i < bCount; i++)
                    {
                        if (_player.GunType == 3)
                        {
                            angle = ((float)rand.NextDouble() - 0.5f) / 2.0f;
                        }
                        bullets.Add(Shoot(Player.Rectangle.Center, mPos + GameData.Camera / GameData.Scale, angle, false));
                    }
                    return bullets;
                }
            }
            if (!_reload && _player.CurBulletsCount == 0)
            {
                UiObjects["reload_bar"].Visible = true;
                _reload = true;
                _startReloadTime = _roundTime;
            }
            if (_reload && _roundTime - _startReloadTime > _gun.ReloadTime)
            {
                UiObjects["reload_bar"].Visible = false;
                _reload = false;
                _player.CurBulletsCount = _gun.Capacity;
                _capacity.Text = $"{_player.CurBulletsCount}/{_gun.Capacity}";
            }
            if (_reload)
            {
                var rRect = UiObjects["reload_bar"].Rectangle;
                UiObjects["reload_bar"].Rectangle = new RectangleF(rRect.Min,
                    new Vector2(rRect.Min.X + Constants.HpBarWidth * 
                    (float)(_roundTime - _startReloadTime) / (float)_gun.ReloadTime, rRect.Max.Y));
            }
            return bullets;
        }

        private Bullet Shoot(Vector2 _from, Vector2 _to, float angle, bool isEnemy)
        {
            var shootDir = _to - _from;
            shootDir.Normalize();
            if (float.IsNaN(shootDir.X) || float.IsNaN(shootDir.Y))
            {
                shootDir = Vector2.One;
            }
            var cs = (float)Math.Cos(angle);
            var sn = (float)Math.Sin(angle);
            var tempSD = shootDir;
            shootDir = new Vector2(tempSD.X * cs - tempSD.Y * sn, tempSD.X * sn + tempSD.Y * cs);
            var startShootPos = _from + shootDir * Constants.WeaponLength;
            var endShootPos = startShootPos + shootDir * _gun.MaxDistance;
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
            var bullet = new Bullet(isEnemy ? "enemy_bullet" : "friendly_bullet",
                new Circle(new Vector2(Constants.BulletSize / 2), Constants.BulletSize / 2),
                _gun.BulletDamage,
                intersectedWall,
                new RectangleF(startShootPos, startShootPos + new Vector2(Constants.BulletSize)) - new Vector2(Constants.BulletSize / 2),
                shootDir * _gun.BulletSpeed);
            bullet.StartPoint = startShootPos;
            bullet.EndPoint = endShootPos;
            bullet.Team = Player.Team;
            return bullet;
        }

        private void FinalScore()
        {
            if (_rounds[0] > _rounds[1])
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
            _gun = new Gun(1);
            CloseWeaponChoose();
            // Send to server
        }

        private void ChooseGun2()
        {
            _player.GunType = 2;
            _gun = new Gun(2);
            CloseWeaponChoose();
            // Send to server
        }

        private void ChooseGun3()
        {
            _player.GunType = 3;
            _gun = new Gun(3);
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
        private void Exit()
        {
            GameData.Clear();
            _game._scene = new SceneMenu();
            _game._scene.Initialize(_game);
        }
        private void SetHandCursor()
        {
            Mouse.SetCursor(MouseCursor.Hand);
        }
    }
}