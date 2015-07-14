using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace AchtungXNA
{
    public class Player
    {
        static int Counter;

        public enum ControlsType 
        {
            WASD, // a - left, d - right
            Arrows, // left and right
            Numpad69, // 6 and 9
            Numpad02, // 0 and 2
            AZ, // a and z
            BN, // b and n
            Arrows2, // left and down arrows
            Gamepad1,
            Gamepad2,
            Gamepad12,
            Gamepad22,
            Mouse,
            Undefined
        };

        public static ControlsType ControlsFromString(string s)
        {
            switch (s.ToLower())
            {
                case "wasd": return ControlsType.WASD;
                case "arrows": return ControlsType.Arrows;
                case "arrows2": return ControlsType.Arrows2;
                case "az": return ControlsType.AZ;
                case "bn": return ControlsType.BN;
                case "69": return ControlsType.Numpad69;
                case "02": return ControlsType.Numpad02;
                case "gamepad1": return ControlsType.Gamepad1;
                case "gamepad2": return ControlsType.Gamepad2;
                case "gamepad12": return ControlsType.Gamepad12;
                case "gamepad22": return ControlsType.Gamepad22;
                case "mouse": return ControlsType.Mouse;
                default: return ControlsType.Undefined;
            }
        }

        public const float 
            Speed = 1.5f, 
            RotationSpeed = 0.07f, 
            FastSpeed = 2.2f, 
            SlowSpeed = 1f, 
            SpeedDelta = 0.05f;

        public Vector2 Position;
        public float Angle;
        public Vector2 ReversePos;
        public float ReverseAngle;
        public ControlsType Controls;
        public Color Color;
        public int LayWallDelay;
        public Vector2 LastPos;
        public Vector2 OlderPos;
        public bool Dead;
        public Wall LastWallDeployed;
        public int Score;
        public int GapTimer;
        public double CurrentSpeed, TargetSpeed;
        public int ConfuseTimer;
        public int SlowTimer;
        public int SpeedTimer;
        public int WallhackTimer;
        public int SlowTurnTimer;
        public int HideIcons;
        public int PartyTimer;
        public int ID;
        public Team Team;
        public bool ForceColor = false;

        public static Player FromLine(Game1 game, string line)
        {
            string[] splits = line.Split(' ');
            ControlsType controls = ControlsFromString(splits[0]);
            if (splits.Length == 2) // controls and team
            {
                string team = splits[1];
                Team t = game.Teams.First(tm => tm.Name == team);
                Player p = new Player(Vector2.Zero, 0, controls, t.GetColor(t.Players.Count));
                p.Team = t;
                t.Players.Add(p);
                return p;
            }
            else if (splits.Length == 4) // controls and color
            {
                return new Player(Vector2.Zero, 0, controls, new Color(int.Parse(splits[1]), int.Parse(splits[2]), int.Parse(splits[3]))) { ForceColor = true };
            }
            else// controls, team and color
            {
                string team = splits[1];
                Team t = game.Teams.First(tm => tm.Name == team);
                Player p = new Player(Vector2.Zero, 0, controls, new Color(int.Parse(splits[2]), int.Parse(splits[3]), int.Parse(splits[4])));
                p.Team = t;
                t.Players.Add(p);
                p.ForceColor = true;
                return p;
            }
        }

        public Player(Vector2 pos, float angle, ControlsType controls, Color color)
        {
            Counter++;
            ID = Counter;

            OlderPos = LastPos = Position = ReversePos = pos;
            Angle = angle;
            ReverseAngle = angle + (float)Math.PI;
            Controls = controls;
            Color = color;
            CurrentSpeed = TargetSpeed = Speed;
        }

        private int GetInput()
        {
            int input = 0;
            KeyboardState keys = Keyboard.GetState();
            switch (Controls)
            {
                case ControlsType.Arrows:
                    if (keys.IsKeyDown(Keys.Left)) input |= 2;
                    if (keys.IsKeyDown(Keys.Right)) input |= 1;
                    break;
                case ControlsType.WASD:
                    if (keys.IsKeyDown(Keys.A)) input |= 2;
                    if (keys.IsKeyDown(Keys.D)) input |= 1;
                    break;
                case ControlsType.Numpad69:
                    if (keys.IsKeyDown(Keys.NumPad6)) input |= 2;
                    if (keys.IsKeyDown(Keys.NumPad9)) input |= 1;
                    break;
                case ControlsType.AZ:
                    if (keys.IsKeyDown(Keys.A)) input |= 2;
                    if (keys.IsKeyDown(Keys.Z)) input |= 1;
                    break;
                case ControlsType.BN:
                    if (keys.IsKeyDown(Keys.B)) input |= 2;
                    if (keys.IsKeyDown(Keys.N)) input |= 1;
                    break;
                case ControlsType.Arrows2:
                    if (keys.IsKeyDown(Keys.Left)) input |= 2;
                    if (keys.IsKeyDown(Keys.Down)) input |= 1;
                    break;
                case ControlsType.Numpad02:
                    if (keys.IsKeyDown(Keys.NumPad0)) input |= 2;
                    if (keys.IsKeyDown(Keys.NumPad2)) input |= 1;
                    break;
                case ControlsType.Gamepad1:
                    GamePadState gamepad = GamePad.GetState(PlayerIndex.One);
                    if (gamepad.DPad.Left == ButtonState.Pressed) input |= 2;
                    if (gamepad.DPad.Right == ButtonState.Pressed) input |= 1;
                    break;
                case ControlsType.Gamepad2:
                    GamePadState gamepad2 = GamePad.GetState(PlayerIndex.Two);
                    if (gamepad2.DPad.Left == ButtonState.Pressed) input |= 2;
                    if (gamepad2.DPad.Right == ButtonState.Pressed) input |= 1;
                    break;
                case ControlsType.Gamepad12:
                    GamePadState gamepad3 = GamePad.GetState(PlayerIndex.One);
                    if (gamepad3.Buttons.A == ButtonState.Pressed) input |= 2;
                    if (gamepad3.Buttons.B == ButtonState.Pressed) input |= 1;
                    break;
                case ControlsType.Gamepad22:
                    GamePadState gamepad4 = GamePad.GetState(PlayerIndex.Two);
                    if (gamepad4.Buttons.A == ButtonState.Pressed) input |= 2;
                    if (gamepad4.Buttons.B == ButtonState.Pressed) input |= 1;
                    break;
                case ControlsType.Mouse:
                    MouseState mouse = Mouse.GetState();
                    if (mouse.LeftButton == ButtonState.Pressed) input |= 2;
                    if (mouse.RightButton == ButtonState.Pressed) input |= 1;
                    break;
            }

            if (IsConfused())
                input = ((input & 1) << 1) | ((input & 2) >> 1);

            return input;
        }

        public bool IsConfused()
        {
            return ConfuseTimer > 0;
        }

        public void UpdatePowerUps(Game1 game)
        {
            if (ConfuseTimer > 0)
                ConfuseTimer--;
            if (WallhackTimer > 0)
                WallhackTimer--;
            if (SlowTurnTimer > 0)
                SlowTurnTimer--;
            if (PartyTimer > 0)
                PartyTimer--;
            if (HideIcons > 0)
                HideIcons--;

            if (SpeedTimer > 0)
            { SpeedTimer--; TargetSpeed = FastSpeed; }
            else if (SlowTimer > 0)
            { SlowTimer--; TargetSpeed = SlowSpeed; }
            else TargetSpeed = Speed;

            if (Math.Abs(TargetSpeed - CurrentSpeed) < SpeedDelta)
                CurrentSpeed = TargetSpeed;
            else if (CurrentSpeed < TargetSpeed)
                CurrentSpeed += SpeedDelta;
            else
                CurrentSpeed -= SpeedDelta;

            for (int i = 0; i < game.Powerups.Count; i++)
            {
                if (game.Powerups[i].IntersectsPlayer(this))
                    game.Powerups[i].ApplyOnPlayer(game, this);
            }
        }

        public void Update(Game1 game)
        {
            int input = GetInput();

            UpdatePowerUps(game);

            float rot = RotationSpeed;
            if (SlowTurnTimer > 0) rot /= 3;

            if ((input & 0x2) != 0) // turn left
                Angle -= rot;
            if ((input & 0x1) != 0) // turn right
                Angle += rot;

            Vector2 speed = new Vector2(0, 0);
            speed.X = (float)(Math.Cos(Angle) * CurrentSpeed);
            speed.Y = (float)(Math.Sin(Angle) * CurrentSpeed);

            Position += speed;
            
            if (WallhackTimer > 0)
            {
                Vector2 deltaPos = new Vector2(0, 0);
                if (Position.X > Game1.ScreenWidth)
                    deltaPos.X = -Game1.ScreenWidth;
                if (Position.X < 0)
                    deltaPos.X = +Game1.ScreenWidth;
                if (Position.Y > Game1.ScreenHeight)
                    deltaPos.Y = -Game1.ScreenHeight;
                if (Position.Y < 0)
                    deltaPos.Y = +Game1.ScreenHeight;
                
                if (deltaPos != Vector2.Zero)
                {
                    LayWall(game, true);                                        
                    Position += deltaPos;
                    LastPos += deltaPos;
                    OlderPos += deltaPos;
                }
            }

            foreach (Wall wall in game.Walls)
            {
                if ((WallhackTimer > 0) && (wall.Owner == null))
                    continue;

                if ((game.Ticks - wall.LayTime < 30 && wall.IsOwner(this)) || (WallhackTimer > 0))
                    continue;

                Vector2 e, f, g;
                float l, skait, var;
                f = wall.p2 - wall.p1;
                e = Position - wall.p1;
                g = OlderPos - Position;

                skait = f.X * e.Y - e.X * f.Y;
                var = g.X * f.Y - f.X * g.Y;

                if (Math.Abs(var) > 0.001)
                {
                    l = skait / var;
                    float l1 = (e.X * g.Y - g.X * e.Y) / -var;

                    if (l1 >= 0 && l1 <= 1 && l >= 0 && l <= 1 && game.WinningTeam == null)
                    {
                        Position = wall.p1 + e + g * l;
                        game.Walls.Add(new Wall(LastPos, Position, Color, game.Ticks, this));
                        this.Dead = true;
                        Team.UpdateAliveStatus();
                        game.AddScore(Team);
                        Team.TestIfEnded(game);
                        return;
                    }
                }
            }

            if (LayWallDelay < 0)
            {
                LastPos = OlderPos = Position;
            }

            LayWallDelay++;
            GapTimer--;
            if (LayWallDelay > 6)
            {
                LayWall(game, false);                
                OlderPos = LastPos;
                LastPos = Position;
                LayWallDelay = 0;
            }
            else if (GapTimer < 0)
            {
                GapTimer = game.rnd.Next(800) + 80;
                LayWall(game, true);
                OlderPos = LastPos = Position;
                LayWallDelay = -8;
            }

            if (PartyTimer > 0)
                for (int i = 0; i < 30 / game.Players.Count; i++)
                    DropParticle(game, Position);
        }

        public void Draw(SpriteBatch sb, Game1 game)
        {
            float Lenght = (float)Math.Sqrt((Position.X - LastPos.X) * (Position.X - LastPos.X) + (Position.Y - LastPos.Y) * (Position.Y - LastPos.Y));
            float Angle = (float)Math.Atan2(Position.Y - LastPos.Y, Position.X - LastPos.X) - (float)Math.PI;
            Vector2 origin = new Vector2(0f, 0.5f);

            Color color = Color;
            //if (color == game.ColorToChange)
            //    color = game.NewColor;
            if (game.WinningTeam == Team && game.ShouldFlash && !Dead)
                color = new Color(Color.R / 2, Color.G / 2, Color.B / 2);

            if (Dead)
            {
                color.R >>= 2;
                color.G >>= 2;
                color.B >>= 2;
                color.A >>= 2;
            }

            if (game.Paused)
            {
                color.R >>= 2;
                color.G >>= 2;
                color.B >>= 2;
                color.A >>= 2;
            }

            sb.Draw(Wall.Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)Lenght, 2), null, color, Angle, origin, SpriteEffects.None, 1);
            sb.Draw(Wall.Texture, new Rectangle((int)Position.X, (int)Position.Y, 5, 5), null, color, Angle, new Vector2(0.5f, 0.5f), SpriteEffects.None, 1);

            if (LastWallDeployed != null)
            {
                Vector2 p1 = (LastWallDeployed.p1 + LastWallDeployed.p2) / 2, p2 = (Position + LastPos) / 2;

                Lenght = (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
                Angle = (float)Math.Atan2(p1.Y - p2.Y, p1.X - p2.X) - (float)Math.PI;

                sb.Draw(Wall.Texture, new Rectangle((int)p1.X, (int)p1.Y, (int)Lenght, 2), null, color, Angle, origin, SpriteEffects.None, 1);
            }

            if (HideIcons == 0)
            {
                if (ConfuseTimer > 180 || (ConfuseTimer / 15) % 2 == 1)
                {
                    sb.Draw(Powerup.Textures, new Rectangle((int)Position.X - Powerup.Radius, (int)Position.Y - 20, 21, 21),
                        new Rectangle(0, 21, 21, 21), Color.White);
                }
                if (SpeedTimer > 180 || (SpeedTimer / 15) % 2 == 1)
                {
                    sb.Draw(Powerup.Textures, new Rectangle((int)Position.X - Powerup.Radius, (int)Position.Y - 20, 21, 21),
                        new Rectangle(21, 21, 21, 21), Color.White);
                }
                if (SlowTimer > 180 || (SlowTimer / 15) % 2 == 1)
                {
                    sb.Draw(Powerup.Textures, new Rectangle((int)Position.X - Powerup.Radius, (int)Position.Y - 20, 21, 21),
                        new Rectangle(42, 21, 21, 21), Color.White);
                }
                if (WallhackTimer > 180 || (WallhackTimer / 15) % 2 == 1)
                {
                    sb.Draw(Powerup.Textures, new Rectangle((int)Position.X - Powerup.Radius, (int)Position.Y - 20, 21, 21),
                        new Rectangle(63, 21, 21, 21), Color.White);
                }
                if (SlowTurnTimer > 180 || (SlowTurnTimer / 15) % 2 == 1)
                {
                    sb.Draw(Powerup.Textures, new Rectangle((int)Position.X - Powerup.Radius, (int)Position.Y - 20, 21, 21),
                        new Rectangle(84, 21, 21, 21), Color.White);
                }
            }
        }
        
        public void LayWall(Game1 game, bool discontinuity) {
            Wall wall = new Wall(LastPos, Position, Color, game.Ticks, this);
            game.Walls.Add(wall);
            DropParticle(game, (wall.p1 + wall.p2) / 2);
            if (LastWallDeployed != null)
                game.Walls.Add(new Wall((wall.p1 + wall.p2) / 2, (LastWallDeployed.p1 + LastWallDeployed.p2) / 2, Color, game.Ticks, this));
            LastWallDeployed = !discontinuity ? wall : null;
        }

        public void DropParticle(Game1 game, Vector2 pos, Color c)
        {
            float speed = 0.1f;
            if (PartyTimer > 0)
            {
                speed += (float)game.rnd.NextDouble() * 2;
                switch (game.rnd.Next(10))
                {
                    case 0: c = Color.White; break;
                    case 1: c = Color.Black; break;
                    case 2: c = Color.Green; break;
                    case 3: c = Color.Blue; break;
                    case 4: c = Color.Yellow; break;
                    case 5: c = Color.Pink; break;
                    case 6: c = Color.Red; break;
                    case 7: c = Color.Orange; break;
                    case 8: c = Color.Magenta; break;
                    case 9: c = Color.Coral; break;
                }
            }
            game.Particles.Add(new Particle(pos, game.GetVector(speed), c));
        }
        
        public void DropParticle(Game1 game, Vector2 pos) {
            DropParticle(game, pos, Color);
        }
    }
}
