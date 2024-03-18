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
        private readonly Color actualWallColor = Color.Orange;
        private readonly Brush floorBrush = Brushes.Gray;
        private readonly Maze maze;
        private readonly Player player = new();
        private readonly Size playerSize = new(10, 10);
        private readonly Block[] blocks;
        private BufferedGraphics buffer;
        private const int BlockSize = 10;
        private const int PlayerSpeed = 2;
        private const int RenderDistance = 50;
        private const double WallHeightMultiplier = 0.1;
        private const double Sensitivity = 0.01;
        private readonly int size = 10;
        private double wallHeight;
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
            blocks = new Block[size * size];
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    blocks[row * size + col] = new Block(new Rectangle(BlockSize * col, BlockSize * row, BlockSize, BlockSize), maze.Walls[row, col]);
                }
            }
            player.CenterLocation = CenterOfRect(blocks[size + 1].Rect);
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

        private IEnumerable<Block> GetNearbyBlocks()
        {
            return blocks.Where(block => block.IsSolid
                            && block.Rect.X - player.CenterLocation.X < RenderDistance
                            && block.Rect.Y - player.CenterLocation.Y < RenderDistance);
        }

        private void Redraw()
        {
            var playerViewLeftSide = player.Angle - player.FieldOfView / 2;
            var angleStep = player.FieldOfView / DisplayRectangle.Width;
            var windowCenter = DisplayRectangle.Height / 2;
            var bitmap = new Bitmap(DisplayRectangle.Width, DisplayRectangle.Height);
            var blocksNearby = GetNearbyBlocks();
            for (var col = 0; col < DisplayRectangle.Width; col++)
            {
                var rayEnd = new Vector();
                var step = new Vector(0, -1).Rotate(playerViewLeftSide + angleStep * col); //Костыль
                for (; rayEnd.Length < RenderDistance; rayEnd += step)
                {
                    var point = (Point)(new Vector(player.CenterLocation) + rayEnd);
                    var blockFound = false;
                    foreach (var block in blocksNearby)
                    {
                        if (block.Rect.Contains(point))
                        {
                            blockFound = true;
                            break;
                        }
                    }
                    if (blockFound)
                    {
                        break;
                    }
                }
                var distance = rayEnd.Length > RenderDistance ? RenderDistance : rayEnd.Length;
                var visibleHeight = wallHeight / (distance * WallHeightMultiplier);
                var distancePercent = distance / RenderDistance;
                var wallColor = Color.FromArgb(actualWallColor.R - (int)(actualWallColor.R * distancePercent),
                                               actualWallColor.G - (int)(actualWallColor.G * distancePercent),
                                               actualWallColor.B - (int)(actualWallColor.B * distancePercent));
                for (var row = 0; row < DisplayRectangle.Height; row++)
                {
                    Color color;
                    if (row < windowCenter - visibleHeight / 2)
                    {
                        color = Color.Black;
                    }
                    else if (row < windowCenter + visibleHeight / 2)
                    {
                        color = wallColor;
                    }
                    else
                    {
                        color = Color.Gray;
                    }
                    bitmap.SetPixel(col, row, color);
                }
            }
            buffer.Graphics.DrawImage(bitmap, new Point());
            foreach (var block in blocks)
            {
                buffer.Graphics.FillRectangle(block.IsSolid ? wallBrush : floorBrush, block.Rect);
            }
            buffer.Graphics.FillEllipse(wallBrush, RectFromCenter(player.CenterLocation, playerSize));
            var normalized = new Vector(0, -1).Normalize().Rotate(player.Angle) * (playerSize.Width / 2);
            buffer.Graphics.FillEllipse(Brushes.Red, RectFromCenter(new Point(player.CenterLocation.X + (int)Math.Round(normalized.X), player.CenterLocation.Y + (int)Math.Round(normalized.Y)), new Size(5, 5)));
            buffer.Render();
            buffer.Graphics.Clear(Color.White);
        }

        private void MainForm_Shown(object _, EventArgs __)
        {
            buffer = BufferedGraphicsManager.Current.Allocate(CreateGraphics(), DisplayRectangle);
            buffer.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            wallHeight = DisplayRectangle.Height;
            moveDelay.Start();
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
            if (horizontalMove != 0 || verticalMove != 0)
            {
                var normalized = new Vector(horizontalMove, verticalMove).Normalize().Rotate(player.Angle) * PlayerSpeed;
                var blocksNearby = GetNearbyBlocks();
                var newLocation = new Point(player.CenterLocation.X + (int)Math.Round(normalized.X), player.CenterLocation.Y + (int)Math.Round(normalized.Y));
                var collide = false;
                foreach (var block in blocksNearby)
                {
                    if (block.Rect.Contains(newLocation))
                    {
                        collide = true;
                        break;
                    }
                }
                if (!collide)
                {
                    player.CenterLocation = new Point(player.CenterLocation.X + (int)Math.Round(normalized.X), player.CenterLocation.Y + (int)Math.Round(normalized.Y));
                }
            }
            Redraw();
        }

        private void MainForm_KeyUp(object _, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
        }

        private void MainForm_MouseMove(object _, MouseEventArgs e)
        {
            player.Angle += (e.X - previousHorizontalMousePos) * Sensitivity;
            previousHorizontalMousePos = e.X;
        }
    }
}
