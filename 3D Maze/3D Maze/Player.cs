using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal sealed class Player
    {
        private double angle;

        public Point CenterLocation { get; set; }

        public double FieldOfView { get; private set; } = Math.PI / 3;

        public double Angle
        {
            get => angle;
            set => angle = value % ((double)Math.PI * 2);
        }
    }
}
