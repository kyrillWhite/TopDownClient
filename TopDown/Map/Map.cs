using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TopDown
{
    public class Map
    {
        public List<Ground> _grounds = new List<Ground>();
        public List<Wall> _walls = new List<Wall>();
        public int _commandsCount = 0;
        public List<RectangleF> _spawnZones = new List<RectangleF>();

        public Map() { }

        public Map(string path, int cCount)
        {
            _commandsCount = 2;
            int[,] _map = new int[42, 42];
            string s;
            var _j = 1;
            using (var f = new StreamReader(path))
            {
                while ((s = f.ReadLine()) != null)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        _map[i + 1, _j] = s[i];
                    }
                    _j++;
                }
            }
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    _grounds.Add(new Ground($"ground{(new Random().Next(1, 7))}",
                        new RectangleF(i * 80, j * 80, 80, 80)));
                }
            }

            // 35 - wall
            // 49 - first team
            // 50 - second team
            // 32 - empty
            var fp1 = Point.Zero;
            var fp2 = Point.Zero;

            var sp1 = Point.Zero;
            var sp2 = Point.Zero;

            for (int i = 1; i < 41; i++)
            {
                for (int j = 1; j < 41; j++)
                {
                    if (_map[i, j] == 49)
                    {
                        if (fp1 == Point.Zero)
                        {
                            fp1.X = i - 1;
                            fp1.Y = j - 1;
                        }
                        else
                        {
                            fp2.X = i - 1;
                            fp2.Y = j - 1;
                        }
                    }
                    if (_map[i, j] == 50)
                    {
                        if (sp1 == Point.Zero)
                        {
                            sp1.X = i;
                            sp1.Y = j;
                        }
                        else
                        {
                            sp2.X = i;
                            sp2.Y = j;
                        }
                    }
                    if (_map[i, j] == 35)
                    {
                        var texture = "";
                        var wallType = 
                            (_map[i, j - 1] == 35 ? "1" : "0") + // up
                            (_map[i, j + 1] == 35 ? "1" : "0") + // down
                            (_map[i - 1, j] == 35 ? "1" : "0") + // left
                            (_map[i + 1, j] == 35 ? "1" : "0");  // right
                        switch(wallType) {
                            case "0000":
                                texture = "wall_w";
                                break;
                            case "0001":
                                texture = "wall_right";
                                break;
                            case "0010":
                                texture = "wall_left";
                                break;
                            case "0100":
                                texture = "wall_down";
                                break;
                            case "1000":
                                texture = "wall_up";
                                break;
                            case "0011":
                                texture = "wall_hor";
                                break;
                            case "1100":
                                texture = "wall_ver";
                                break;
                            case "1010":
                                texture = "wall_up_left";
                                break;
                            case "1001":
                                texture = "wall_up_right";
                                break;
                            case "0110":
                                texture = "wall_down_left";
                                break;
                            case "0101":
                                texture = "wall_down_right";
                                break;
                            case "0111":
                                texture = "wall_hor_down";
                                break;
                            case "1011":
                                texture = "wall_hor_up";
                                break;
                            case "1101":
                                texture = "wall_ver_right";
                                break;
                            case "1110":
                                texture = "wall_ver_left";
                                break;
                            case "1111":
                                texture = "wall_ver_hor";
                                break;
                        }
                        var wall = new Wall(texture, new RectangleF((i - 1) * 80, (j - 1) * 80, 80, 80));
                        _walls.Add(wall);
                    }
                }
            }
            _spawnZones.Add(new RectangleF(fp1.ToVector2() * 80, fp2.ToVector2() * 80));
            _spawnZones.Add(new RectangleF(sp1.ToVector2() * 80, sp2.ToVector2() * 80));
        }
    }
}
