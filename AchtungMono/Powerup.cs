﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AchtungXNA
{
    public class Powerup
    {
        public static Texture2D Textures;

        public enum PowerupType { Confuse = 0, Speed = 1, Slow = 2, Wallhack = 3, SlowTurn = 4, Satan = 5, Party = 6 }

        public const int Radius = 10, LifeTime = 20;

        public bool ShouldBeRemoved;
        public int X, Y, Time;
        public PowerupType Type;

        public Powerup(int x, int y, PowerupType type)
        {
            ShouldBeRemoved = false;
            Type = type;
            X = x;
            Y = y;
            Time = 0;
        }

        public bool IntersectsPlayer(Player p)
        {
            return !ShouldBeRemoved && (X - p.Position.X) * (X - p.Position.X) + (Y - p.Position.Y) * (Y - p.Position.Y) <= Radius * Radius;
        }

        public void Update()
        {
            Time++;
            //if (Time > LifeTime * 60)
            //    ShouldBeRemoved = true;
        }

        public void Draw(Game1 game, SpriteBatch sb)
        {
            Color color = Color.White;
            if (game.Paused)
            {
                color.R >>= 2;
                color.G >>= 2;
                color.B >>= 2;
                color.A >>= 2;
            }

            if (true)//Time < LifeTime * 60 - 240 || ((Time >> 4) & 1) == 0)
                sb.Draw(Textures, new Rectangle(X - Radius, Y - Radius, Radius * 2 + 1, Radius * 2 + 1),
                    new Rectangle((int)(Type) * 21, 0, 21, 21), color);
        }

        public void ApplyOnPlayer(Game1 game, Player player)
        {
            ShouldBeRemoved = true;

            switch (Type)
            {
                case PowerupType.Confuse:
                    for (int i = 0; i < game.Players.Count; i++)
                        if (game.Players[i].Team != player.Team)
                            game.Players[i].ConfuseTimer = 300;
                    //player.ConfuseTimer = 300;
                    break;
                case PowerupType.Slow:
                    player.SlowTimer = 420;
                    player.SpeedTimer = 0;
                    break;
                case PowerupType.Speed:
                    player.SlowTimer = 0;
                    player.SpeedTimer = 420;
                    break;
                case PowerupType.Wallhack:
                    player.WallhackTimer = 300;
                    break;
                case PowerupType.SlowTurn:
                    for (int i = 0; i < game.Players.Count; i++)
                        if (game.Players[i].Team != player.Team)
                            game.Players[i].SlowTurnTimer = 300;
                    break;
                case PowerupType.Party:
                    for (int i = 0; i < game.Players.Count; i++)
                    {
                        game.Players[i].PartyTimer = 300;
                        game.Players[i].SpeedTimer = 300;
                    }
                    break;
                case PowerupType.Satan:
                    List<Player> players = game.Players.Where(p => !p.Dead).ToList();

                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].WallhackTimer = 15;
                        players[i].HideIcons = 16;
                        Wall wall = new Wall(players[i].LastPos, players[i].Position, players[i].Color, game.Ticks, players[i]);
                        game.Walls.Add(wall);
                        players[i].DropParticle(game, (wall.p1 + wall.p2) / 2);
                        //game.Particles.Add(new Particle((wall.p1 + wall.p2) / 2, game.GetVector(0.1f), players[i].Color));
                        if (players[i].LastWallDeployed != null)
                            game.Walls.Add(new Wall((wall.p1 + wall.p2) / 2, (players[i].LastWallDeployed.p1 + players[i].LastWallDeployed.p2) / 2, players[i].Color, game.Ticks, players[i]));
                        players[i].LastWallDeployed = null;// wall;
                        players[i].OlderPos = players[i].LastPos;
                        players[i].LastPos = players[i].Position;
                        players[i].LayWallDelay = 0;

                    }

                    for (int i = 1; i < players.Count; i++)
                    {
                        int k = game.rnd.Next(i);
                        Vector2 pos = players[i].Position;
                        players[i].Position = players[k].Position;
                        players[k].Position = pos;
                        pos = players[i].LastPos;
                        players[i].LastPos = players[k].LastPos;
                        players[k].LastPos = pos;
                        float rot = players[i].Angle;
                        players[i].Angle = players[k].Angle;
                        players[k].Angle = rot;
                    }


                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].OlderPos = players[i].LastPos;
                        players[i].LastPos = players[i].Position;
                    }
                    break;
            }
        }
    }
}
