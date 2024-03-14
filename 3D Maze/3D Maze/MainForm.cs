using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Maze
{
    public partial class MainForm : Form
    {
        private readonly Graphics graphics;
        private readonly Pen bordersPen = Pens.Black;
        private readonly Brush fillBrush = Brushes.Orange;
        private readonly Maze maze;
        private const int BlockSize = 25;
        private readonly int size = 9;

        public MainForm()
        {
            InitializeComponent();
            graphics = CreateGraphics();
            maze = new Maze(ref size);
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    if (maze.Walls[row, col])
                    {
                        graphics.DrawRectangle(bordersPen, new Rectangle(BlockSize * col, BlockSize * row, BlockSize, BlockSize));
                        graphics.FillRectangle(fillBrush, new Rectangle(BlockSize * col, BlockSize * row, BlockSize, BlockSize));
                    }
                }
            }
        }
    }
}
