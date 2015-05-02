using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AchtungXNA
{
    public class Particle
    {
        public static Texture2D Texture;
        public const int Size = 10;

        public Vector2 Position, Velocity;
        public Color Color;
        public int Age;
        public bool Deleted;
        public float R, G, B;

        public Particle(Vector2 pos, Vector2 velocity, Color color)
        {
            Position = pos;
            Velocity = velocity;
            Color = color;
            R = (float)color.R / 255.0f;
            G = (float)color.G / 255.0f;
            B = (float)color.B / 255.0f;

        }

        public void Update()
        {
            Age++;
            if (Age > 255)
                Deleted = true;
            Position += Velocity;
            Velocity *= 0.99f;
            float value = (255 - Age);
            Color.A = (byte)value;
            Color.R = (byte)(value * R);
            Color.G = (byte)(value * G);
            Color.B = (byte)(value * B);

        }

        public void Draw(Game1 game, SpriteBatch sb)
        {
            Color color = Color;
            if (game.Paused)
            {
                color.R >>= 2;
                color.G >>= 2;
                color.B >>= 2;
                color.A >>= 2;
            }

            sb.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), null, color, 0, new Vector2(5, 5), SpriteEffects.None, 0);
        }
    }
}
