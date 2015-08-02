#region Using Statements
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
#endregion

namespace AchtungXNA
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static int ScreenWidth, ScreenHeight;

        public List<Team> Teams;
        public List<Player> Players;
        public List<Wall> Walls;
        public List<Particle> Particles;
        public List<Powerup> Powerups;
        public ulong Ticks;
        //public Player Winner;
        public Team WinningTeam;
        public bool ShouldFlash;
        int borders = 100;
        private int TeamColorSpacing = 30;

        //public Color ColorToChange, NewColor;
        public int WinAnimationTimer;
        private int PowerupDelayValue = 210;

        public Random rnd;

        public int PowerUpDelay;

        private Texture2D Font;
        private SpriteFont SpriteFont;

        public bool Paused;
        bool JustPaused;
        bool oldEscape;

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            ScreenWidth = graphics.PreferredBackBufferWidth =  /*1000;*/ (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
            ScreenHeight = graphics.PreferredBackBufferHeight = /*600;*/ (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();

            rnd = new Random((int)DateTime.Now.Ticks);

            Particles = new List<Particle>();
            Players = new List<Player>();
            Teams = new List<Team>();

            string[] settings = File.ReadAllLines("settings.txt").Where(line => line.Length > 0 && !line.StartsWith("#")).ToArray();

            string[] teams = settings.Where(line => line.ToLower().StartsWith("team ")).Select(line => line.Substring(5)).ToArray();
            foreach (var team in teams)
                Teams.Add(new Team(team));

            string[] players = settings.Where(line => line.ToLower().StartsWith("player ")).Select(line => line.Substring(7)).ToArray();
            foreach (var p in players)
            {
                //string[] splits = p.Split(' ');
                Players.Add(Player.FromLine(this, p));
                //Players.Add(new Player(GetPlayerPosition(), (float)rnd.NextDouble() * MathHelper.TwoPi, 
                //    Player.ControlsFromString(splits[0]), new Color(int.Parse(splits[1]), int.Parse(splits[2]), int.Parse(splits[3]))));

            }

            PowerupDelayValue = Setting(settings, "powerupdelay", PowerupDelayValue);
            TeamColorSpacing = Setting(settings, "teamcolorspacing", TeamColorSpacing);

            string name = "Player ";
            int index = 1;
            foreach (Player p in Players)
            {
                if (p.Team == null)
                {
                    p.Team = new Team(name + index, p.Color);
                    index++;
                    Teams.Add(p.Team);
                    p.Team.Players.Add(p);
                }
            }

            for (int i = 0; i < Teams.Count; i++)
                Teams[i].RecolorPlayers(TeamColorSpacing);

            InitParticleTexture();
            ResetGame();
        }

        private int Setting(string[] lines, string name, int defaultValue)
        {
            name += " ";

            string[] delay = lines.Where(line => line.StartsWith(name)).ToArray();
            if (delay.Length > 0)
                return int.Parse(delay[0].Substring(name.Length));
            else
                return defaultValue;
        }

        private void ResetGame()
        {
            Walls = new List<Wall>();
            Powerups = new List<Powerup>();

            for (int i = 0; i < Players.Count; i++)
                Players[i].Position = new Vector2(-10000, -10000);

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].ReversePos = Players[i].OlderPos = Players[i].LastPos = Players[i].Position = GetPlayerPosition();
                Players[i].Angle = (float)rnd.NextDouble() * MathHelper.TwoPi;
                Players[i].ReverseAngle = Players[i].Angle + (float)Math.PI;
                Players[i].LayWallDelay = 0;
                Players[i].LastWallDeployed = null;
                Players[i].Dead = false;
                Players[i].GapTimer = rnd.Next(800) + 80;
                Players[i].SpeedTimer = 0;
                Players[i].SlowTimer = 0;
                Players[i].ConfuseTimer = 0;
                Players[i].WallhackTimer = 0;
                Players[i].SlowTurnTimer = 0;
                Players[i].HideIcons = 0;
                Players[i].PartyTimer = 0;
                Players[i].OneDirectionTimer = 0;
                Players[i].OneDirectionMask = 0;
            }

            for (int i = 0; i < Teams.Count; i++)
            {
                Teams[i].UpdateAliveStatus();
                //Teams[i].Score = 0;
            }

            Walls.Add(new Wall(new Vector2(0, 0), new Vector2(ScreenWidth, 0), Color.Black, 0, null));
            Walls.Add(new Wall(new Vector2(0, ScreenHeight), new Vector2(ScreenWidth, ScreenHeight), Color.Black, 0, null));

            Walls.Add(new Wall(new Vector2(0, 0), new Vector2(0, ScreenHeight), Color.Black, 0, null));
            Walls.Add(new Wall(new Vector2(ScreenWidth, 0), new Vector2(ScreenWidth, ScreenHeight), Color.Black, 0, null));

            //ColorToChange = Color.White;
            //NewColor = Color.White;

            WinAnimationTimer = 0;

            //Winner = null;
            WinningTeam = null;

            PowerUpDelay = PowerupDelayValue * 3 + rnd.Next(PowerupDelayValue / 2);
            PowerUpDelay /= 2;
        }

        public void AddScore(Team team)
        {
            //for (int i = 0; i < Players.Count; i++)
            //    if (!Players[i].Dead)
            //        Players[i].Score++;
            for (int i = 0; i < Teams.Count; i++)
            {
                if (!Teams[i].Dead && Teams[i] != team)
                    Teams[i].Score++;
            }
        }

        public Vector2 GetVector(float length)
        {
            float angle = (float)rnd.NextDouble() * MathHelper.TwoPi;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;
        }

        private void InitParticleTexture()
        {
            int size = 10;
            int dist;
            Color[] data = new Color[size * size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    dist = (int)Math.Sqrt((x - size / 2) * (x - size / 2) + (y - size / 2) * (y - size / 2));
                    dist = Math.Min(25, Math.Max(0, 25 - dist * 5));
                    data[x + y * size] = new Color(dist * 10, dist * 10, dist * 10, dist * 10);
                }

            Particle.Texture = new Texture2D(GraphicsDevice, size, size);
            Particle.Texture.SetData(data);
        }

        private Vector2 GetPlayerPosition()
        {
            Vector2 vec = new Vector2(rnd.Next(ScreenWidth - borders * 2) + borders, rnd.Next(ScreenHeight - borders * 2) + borders);
            bool good = false;
            while (!good)
            {
                good = true;
                vec = new Vector2(rnd.Next(ScreenWidth - borders * 2) + borders, rnd.Next(ScreenHeight - borders * 2) + borders);
                for (int i = 0; i < Players.Count; i++)
                    if ((Players[i].Position - vec).LengthSquared() < 150 * 150)
                    {
                        good = false;
                        break;
                    }
            }
            return vec;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Wall.Texture = new Texture2D(GraphicsDevice, 1, 1);
            Wall.Texture.SetData(new Color[] { Color.White });

            FileStream stream = File.OpenRead("Content/powerups.png");
            Powerup.Textures = Texture2D.FromStream(GraphicsDevice, stream); //Content.Load<Texture2D>("powerups");
            stream.Close();

            stream = File.OpenRead("Content/font.png");
            Font = Texture2D.FromStream(GraphicsDevice, stream);
            stream.Close();

            SpriteFont = Content.Load<SpriteFont>("Arial");
        }

        protected override void Update(GameTime gameTime)
        {
            //if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.))
            //    Exit();

            KeyboardState keys = Keyboard.GetState();

            if (!Paused)
            {
                Ticks++;

                if (PowerUpDelay > 0)
                    PowerUpDelay--;
                else
                {
                    Vector2 pos = GetPlayerPosition();
                    int x = (int)pos.X;
                    int y = (int)pos.Y;
                    int type = rnd.Next(9); //(int)Powerup.PowerupType.OneDirection; 
                    Powerups.Add(new Powerup(x, y, (Powerup.PowerupType)type));

                    PowerUpDelay = PowerupDelayValue + rnd.Next(PowerupDelayValue);
                }

                for (int i = 0; i < Particles.Count; i++)
                {
                    Particles[i].Update();
                    if (Particles[i].Deleted)
                    {
                        Particles.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < Powerups.Count; i++)
                {
                    Powerups[i].Update();
                    if (Powerups[i].ShouldBeRemoved)
                    {
                        Powerups.RemoveAt(i);
                        i--;
                    }
                }

                if (WinningTeam == null)
                {
                    for (int i = 0; i < Players.Count; i++)
                    {
                        if (!Players[i].Dead)
                            Players[i].Update(this);
                    }
                }
                else
                {
                    //ColorToChange = Winner.Color;
                    WinAnimationTimer++;
                    if (WinAnimationTimer < 180)
                    {
                        if (((WinAnimationTimer / 30) & 1) == 1)
                            //NewColor = new Color(ColorToChange.R >> 1, ColorToChange.G >> 1, ColorToChange.B >> 1);
                            ShouldFlash = true;
                        else
                            //NewColor = ColorToChange;
                            ShouldFlash = false;
                    }
                    else
                    {
                        ResetGame();
                    }
                }

                if (keys.IsKeyDown(Keys.Escape) && !oldEscape)
                    JustPaused = Paused = true;
            }
            else
            {
                if (!keys.IsKeyDown(Keys.Escape) && oldEscape && !JustPaused)
                    Paused = false;
            }

            if (Paused && !keys.IsKeyDown(Keys.Escape))
                JustPaused = false;

            oldEscape = keys.IsKeyDown(Keys.Escape);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(60, 60, 60));

            spriteBatch.Begin();

            for (int i = 0; i < Walls.Count; i++)
                Walls[i].Draw(spriteBatch, this);
            for (int i = 0; i < Powerups.Count; i++)
                Powerups[i].Draw(this, spriteBatch);

            int y = 2;
            for (int i = 0; i < Players.Count; i++)
            {    
                Players[i].Draw(spriteBatch, this);

                //spriteBatch.DrawString(font, Players[i].Score.ToString(), new Vector2(5, y), Players[i].Color);
                //y += 15;
                //DrawString(Players[i].Score.ToString(), 2, y, Players[i].Color);
                //y += 26;
            }

            if (Paused)
            {
                y = graphics.PreferredBackBufferHeight / 3;
                for (int i = 0; i < Teams.Count; i++)
                {
                    //Players[i].Draw(spriteBatch, this);

                    //spriteBatch.DrawString(font, Players[i].Score.ToString(), new Vector2(5, y), Players[i].Color);
                    //y += 15;
                    spriteBatch.DrawString(SpriteFont, Teams[i].Score.ToString(), new Vector2(graphics.PreferredBackBufferWidth / 2 + 15, y), Teams[i].Color);
                    spriteBatch.DrawString(SpriteFont, Teams[i].Name, 
                        new Vector2(graphics.PreferredBackBufferWidth / 2 - 15 - SpriteFont.MeasureString(Teams[i].Name).X, y), Teams[i].Color);
                    //DrawString(Teams[i].Score.ToString(), 2, y, Teams[i].Color);
                    y += 42;
                }
            }
            else
            {
                for (int i = 0; i < Teams.Count; i++)
                {
                    //Players[i].Draw(spriteBatch, this);

                    //spriteBatch.DrawString(font, Players[i].Score.ToString(), new Vector2(5, y), Players[i].Color);
                    //y += 15;
                    DrawString(Teams[i].Score.ToString(), 2, y, Teams[i].Color);
                    y += 26;
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            for (int i = 0; i < Particles.Count; i++)
                Particles[i].Draw(this, spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawString(string s, int x, int y, Color color)
        {
            string chars = "0123456789";
            for (int i = 0; i < s.Length; i++)
            {
                int xx = chars.IndexOf(s[i]);
                spriteBatch.Draw(Font, new Rectangle(x + i * 16, y, 16, 24), new Rectangle(xx * 16, 0, 16, 24), color);
            }
        }
    }
            
}
