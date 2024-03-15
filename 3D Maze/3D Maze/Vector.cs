using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal struct Vector(double x, double y)
    {
        public double X { get; set; } = x;

        public double Y { get; set; } = y;

        public readonly double Length => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));

        public readonly Vector Normalize()
        {
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
    }
}
