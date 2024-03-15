using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal sealed class Block(Rectangle rect, Brush brush)
    {
        public Rectangle Rect { get; set; } = rect;

        public Brush Brush { get; set; } = brush;
    }
}
