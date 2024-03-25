using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal struct Vector
    {
        public double X { get; set; }

        public double Y { get; set; }

        public readonly double Length => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));

        public readonly double Angle => Math.Atan2(Y, X);

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public Vector(Point start, Point end)
        {
            X = end.X - start.X;
            Y = end.Y - start.Y;
        }

        public readonly Vector Normalize()
        {
            if (Length == 0)
            {
                throw new InvalidOperationException();
            }
            return new Vector(X / Length, Y / Length);
        }
        public readonly Vector Rotate(double angle)
        {
            return new Vector(X * Math.Cos(angle) - Y * Math.Sin(angle), X * Math.Sin(angle) + Y * Math.Cos(angle));
        }

        public static Vector operator *(Vector vector, double num)
        {
            return new Vector(vector.X * num, vector.Y * num);
        }

        public static Vector operator +(Vector left, Vector right)
        {
            return new Vector(left.X + right.X, left.Y + right.Y);
        }

        public static Point operator +(Point point, Vector vector)
        {
            return new Point(point.X + (int)Math.Round(vector.X), point.Y + (int)Math.Round(vector.Y));
        }

        public static Vector operator -(Vector left, Vector right)
        {
            return new Vector(left.X - right.X, left.Y - right.Y);
        }

        public static explicit operator Point(Vector vector)
        {
            return new Point((int)Math.Round(vector.X), (int)Math.Round(vector.Y));
        }
    }
}
