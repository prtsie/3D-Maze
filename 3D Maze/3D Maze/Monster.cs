using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal sealed class Monster(Image image)
    {
        public Point Position { get; set; }

        public Image Image { get; init; } = image;
    }
}
