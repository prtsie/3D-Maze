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
        private readonly Pen bordersPen = Pens.Black;
        private readonly Brush wallBrush = Brushes.Orange;
        private readonly Brush floorBrush = Brushes.Gray;
        private readonly Maze maze;
        private readonly Player player = new();
        private readonly Size playerSize = new(15, 15);
        private readonly Block[,] blocks;
        private BufferedGraphics buffer;
        private const int BlockSize = 25;
        private const int PlayerSpeed = 3;
        private const double Sensitivity = 0.008;

        private readonly int size = 9;
        private int previousHorizontalMousePos;
        private readonly List<Keys> pressedKeys = new();
        private readonly Dictionary<Keys, Size> controls = new()
        {
            {Keys.W, new Size(0, -PlayerSpeed) },
            {Keys.A, new Size(-PlayerSpeed, 0) },
            {Keys.S, new Size(0, PlayerSpeed) },
            {Keys.D, new Size(PlayerSpeed, 0) }
        };

        public MainForm()
        {
            InitializeComponent();
            maze = new Maze(ref size);
            blocks = new Block[size, size];
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    blocks[row, col] = new Block(new Rectangle(BlockSize * col, BlockSize * row, BlockSize, BlockSize), maze.Walls[row, col] ? wallBrush : floorBrush);
                }
            }
            player.CenterLocation = CenterOfRect(blocks[1, 1].Rect);
            buffer = null!;
        }

        private static Point CenterOfRect(Rectangle rect)
        {
            return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        private static Rectangle RectFromCenter(Point center, Size size)
        {
            return new Rectangle(center.X - size.Width / 2, center.Y - size.Height / 2, size.Width, size.Height);
        }

        private void Redraw()
        {
            buffer.Graphics.Clear(Color.White);
            foreach (var block in blocks)
            {
                buffer.Graphics.FillRectangle(block.Brush, block.Rect);
            }
            buffer.Graphics.FillEllipse(wallBrush, RectFromCenter(player.CenterLocation, playerSize));
            var normalized = new Vector(0, -1).Normalize().Rotate(player.Angle) * (playerSize.Width / 2);
            buffer.Graphics.FillEllipse(Brushes.Red, RectFromCenter(new Point(player.CenterLocation.X + (int)Math.Round(normalized.X), player.CenterLocation.Y + (int)Math.Round(normalized.Y)), new Size(5, 5)));
            buffer.Render();
        }

        private void MainForm_Shown(object _, EventArgs __)
        {
            buffer = BufferedGraphicsManager.Current.Allocate(CreateGraphics(), DisplayRectangle);
            buffer.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            previousHorizontalMousePos = Cursor.Position.X;
        }

        private void MainForm_Paint(object _, PaintEventArgs __)
        {
            Redraw();
        }

        private void MainForm_KeyDown(object _, KeyEventArgs e)
        {
            if (controls.ContainsKey(e.KeyCode) && !pressedKeys.Contains(e.KeyCode))
            {
                pressedKeys.Add(e.KeyCode);
                moveDelay.Start();
            }
        }

        private void moveDelay_Tick(object _, EventArgs __)
        {
            var horizontalMove = 0;
            var verticalMove = 0;
            foreach (var key in pressedKeys)
            {
                var moveDistance = controls[key];
                horizontalMove += moveDistance.Width;
                verticalMove += moveDistance.Height;
            }
            var normalized = new Vector(horizontalMove, verticalMove).Normalize().Rotate(player.Angle) * PlayerSpeed;
            player.CenterLocation = new Point(player.CenterLocation.X + (int)Math.Round(normalized.X), player.CenterLocation.Y + (int)Math.Round(normalized.Y));
            Redraw();
        }

        private void MainForm_KeyUp(object _, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
            if (pressedKeys.Count == 0)
            {
                moveDelay.Stop();
            }
        }

        private void MainForm_MouseMove(object _, MouseEventArgs e)
        {
            player.Angle += (e.X - previousHorizontalMousePos) * Sensitivity;
            previousHorizontalMousePos = e.X;
            Redraw();
        }
    }
}
