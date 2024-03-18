using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal sealed class Block(Rectangle rect, bool isSolid)
    {
        public Rectangle Rect { get; set; } = rect;

        public bool IsSolid { get; set; } = isSolid;
    }
}
