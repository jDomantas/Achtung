using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AchtungXNA
{
    public class Team
    {
        public List<Player> Players;
        public string Name;
        public Color Color;
        public bool Dead;
        public int Score;

        public Team(string line)
        {
            string[] splits = line.Split(' ');
            Name = splits[0];
            Color = new Color(int.Parse(splits[1]), int.Parse(splits[2]), int.Parse(splits[3]));

            Players = new List<Player>();
        }

        public Team(string name, Color color)
        {
            Name = name;
            Color = color;
            Players = new List<Player>();
        }

        public Color GetColor(int i)
        {
            int dist = (i + 1) / 2 * 35;
            int dir = (i % 2) * 2 - 1;

            float h, s, v;
            RGBtoHSV(Color.R, Color.G, Color.B, out h, out s, out v);

            return HSVtoRGB(h + dir * dist, s, v);
        }

        public static void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
        {
            r /= 255f;
            g /= 255f;
            b /= 255f;
            float min, max, delta;
            min = Math.Min(r, Math.Min(g, b));
            max = Math.Max(r, Math.Max(g, b));
            v = max;				// v
            delta = max - min;
            if (max != 0)
                s = delta / max;		// s
            else
            {
                // r = g = b = 0		// s = 0, v is undefined
                s = 0;
                h = -1;
                return;
            }
            if (r == max)
                h = (g - b) / delta;		// between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta;	// between cyan & yellow
            else
                h = 4 + (r - g) / delta;	// between magenta & cyan
            h *= 60;				// degrees
            if (h < 0)
                h += 360;
        }

        public static Color HSVtoRGB(float h, float s, float v)
        {
            while (h < 0) h += 360;
            while (h >= 360) h -= 360;
            float r, g, b;
            int i;
            float f, p, q, t;
            if (s == 0)
            {
                // achromatic (grey)
                r = g = b = v;
                return new Color(r, g, b);
            }
            h /= 60;			// sector 0 to 5
            i = (int)Math.Floor(h);
            f = h - i;			// factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:		// case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return new Color(r, g, b);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Team t = obj as Team;
            return t != null && t.Name == Name;
        }

        public static bool operator ==(Team t1, Team t2)
        {
            if (ReferenceEquals(t1, t2)) return true;
            else if (ReferenceEquals(t1, null) || ReferenceEquals(t2, null)) return false;
            else return t1.Name == t2.Name;
        }

        public static bool operator !=(Team t1, Team t2)
        {
            return !(t1 == t2);
        }

        public void UpdateAliveStatus()
        {
            Dead = Players.Count(player => !player.Dead) == 0;
        }

        public void TestIfEnded(Game1 game)
        {
            int count = game.Teams.Count(team => !team.Dead);
            if (count == 0)
                game.WinningTeam = new Team("", Color.Black);
            else if (count == 1)
                game.WinningTeam = game.Teams.Where(team => !team.Dead).First();
        }

        public void RecolorPlayers(int spacing)
        {
            Player[] recol = Players.Where(p => !p.ForceColor).ToArray();
            if (recol.Length == 0) return;

            float h, s, v;
            RGBtoHSV(Color.R, Color.G, Color.B, out h, out s, out v);

            float start = h - (recol.Length - 1) * spacing / 2f;
            for (int i = 0; i < recol.Length; i++)
                recol[i].Color = HSVtoRGB(start + spacing * i, s, v);
        }
    }
}
