using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AchtungXNA
{
    public class Wall
    {
        public static Texture2D Texture;

        public Vector2 p1, p2;
        public Color Color;
        public ulong LayTime;
        public Player Owner;

        public Wall(Vector2 point1, Vector2 point2, Color color, ulong time, Player owner)
        {
            p1 = point1;
            p2 = point2;
            Color = color;
            LayTime = time;
            Owner = owner;
        }

        public bool IsOwner(Player p)
        {
            return Owner != null && p.ID == Owner.ID;
        }

        public void Draw(SpriteBatch sb, Game1 game)
        {
            float Lenght = (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            float Angle = (float)Math.Atan2(p1.Y - p2.Y, p1.X - p2.X) - (float)Math.PI;
            Rectangle sourceRectangle = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0f, 0.5f);

            Color color = Color;
            //if (color == game.ColorToChange)
            //    color = game.NewColor;
            if (Owner != null && (game.WinningTeam == Owner.Team && game.ShouldFlash && !Owner.Dead))
                color = new Color(Color.R / 2, Color.G / 2, Color.B / 2);

            if (Owner == null || Owner.Dead)
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

            sb.Draw(Texture, new Rectangle((int)p1.X, (int)p1.Y, (int)Lenght, 2), null, color, Angle, origin, SpriteEffects.None, 1);
        }
    }
}
