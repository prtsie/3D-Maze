using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using _3D_Maze.Properties;

namespace _3D_Maze
{
    public partial class MainForm : Form
    {
        private readonly Brush wallBrush = Brushes.Orange;
        private readonly Color actualWallColor = Color.Orange;
        private readonly Brush floorBrush = Brushes.Gray;
        private readonly Random random = new();
        private readonly Maze maze;
        private readonly Player player = new();
        private readonly Size playerSize = new(10, 10);
        private readonly Monster monster = new(Resources.monster);
        private readonly Size monsterOnScreenSize = new(200, 200);
        private readonly Block[] blocks;
        private BufferedGraphics buffer = null!;
        private Bitmap wallsBitmap = null!;
        private Bitmap background = null!;
        private const int BlockSize = 10;
        private const int PlayerSpeed = 1;
        private const int RenderDistance = 40;
        private const int PixelSize = 4;
        private const int MonsterMoveTicks = 150;
        private const double WallHeightMultiplier = 0.1;
        private const double Sensitivity = 0.01;
        private readonly int size = 10;
        private double wallHeight;
        private int previousHorizontalMousePos;
        private double windowCenter;
        private double windowCenterOffset;
        private int offsetCounter;
        private int monsterMoveTicksCounter = 0;
        private double angleStep;
        private IEnumerable<Block> blocksNearby = null!;
        private readonly List<Keys> pressedKeys = new();
        private readonly Dictionary<Keys, Size> controls = new()
        {
            {Keys.W, new Size(0, -PlayerSpeed) },
            {Keys.A, new Size(-PlayerSpeed, 0) },
            {Keys.S, new Size(0, PlayerSpeed) },
            {Keys.D, new Size(PlayerSpeed, 0) }
        };
        private int lampImageIndex;
        private readonly Bitmap[] lampImages = new Bitmap[]
        {
            Resources.lamp_1,
            Resources.lamp_2,
            Resources.lamp_3,
            Resources.lamp_4
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

        private IEnumerable<Block> GetNearbySpace()
        {
            return blocks.Where(block => !block.IsSolid
                            && block.Rect.X - player.CenterLocation.X < RenderDistance
                            && block.Rect.Y - player.CenterLocation.Y < RenderDistance);
        }

        private void Redraw()
        {
            var playerViewLeftSide = (player.Angle - player.FieldOfView / 2) % (Math.PI * 2);
            var windowCenterWithOffset = windowCenter + windowCenterOffset;
            for (var col = 0; col < DisplayRectangle.Width; col += PixelSize)
            {
                var rayEnd = new Vector();
                var step = new Vector(0, -0.4).Rotate(playerViewLeftSide + angleStep * col); //Костыль
                var blockFound = false;
                for (; rayEnd.Length < RenderDistance; rayEnd += step)
                {
                    var point = player.CenterLocation + rayEnd;
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
                for (var row = 0; row < DisplayRectangle.Height; row += PixelSize)
                {
                    Color color;
                    if (row < windowCenterWithOffset - visibleHeight / 2)
                    {
                        color = Color.Black;
                    }
                    else if (row < windowCenterWithOffset + visibleHeight / 2)
                    {
                        color = wallColor;
                    }
                    else
                    {
                        color = Color.Transparent;
                    }
                    for (var pixelRow = 0; pixelRow < PixelSize; pixelRow++)
                    {
                        for (var pixelCol = 0; pixelCol < PixelSize; pixelCol++)
                        {
                            wallsBitmap.SetPixel(col + pixelCol, row + pixelRow, color);
                        }
                    }
                }
            }
            buffer.Graphics.DrawImage(background, 0, 0);
            var vectorToMonster = new Vector(player.CenterLocation, monster.Position);
            var monsterAngle = vectorToMonster.Angle;
            buffer.Graphics.DrawImage(wallsBitmap, 0, 0);
            if (playerViewLeftSide - player.FieldOfView < monsterAngle && monsterAngle < playerViewLeftSide && vectorToMonster.Length < RenderDistance)
            {
                var screenPosition = new Point((int)((playerViewLeftSide - monsterAngle) / angleStep), (int)Math.Round(windowCenterWithOffset));
                var multiplier = vectorToMonster.Length / RenderDistance;
                var visibleSize = new Size((int)Math.Round(monsterOnScreenSize.Width - monsterOnScreenSize.Width * multiplier),
                                           (int)Math.Round(monsterOnScreenSize.Height - monsterOnScreenSize.Height * multiplier));
                buffer.Graphics.DrawImage(monster.Image, new Rectangle(screenPosition, visibleSize));
            }
            var lampRectangle = new Rectangle(400, DisplayRectangle.Height - 178 + (int)Math.Round(windowCenterOffset), 108, 179);
            if (lampImageIndex == lampImages.Length)
            {
                lampImageIndex = 0;
            }
            buffer.Graphics.DrawImage(lampImages[lampImageIndex++], lampRectangle);
            foreach (var block in blocks)
            {
                buffer.Graphics.FillRectangle(block.IsSolid ? wallBrush : floorBrush, block.Rect);
            }
            buffer.Graphics.FillEllipse(wallBrush, RectFromCenter(player.CenterLocation, playerSize));
            var normalized = new Vector(0, -1).Normalize().Rotate(player.Angle) * (playerSize.Width / 2);
            buffer.Graphics.FillEllipse(Brushes.Red, RectFromCenter(new Point(player.CenterLocation.X + (int)Math.Round(normalized.X),
                                                                              player.CenterLocation.Y + (int)Math.Round(normalized.Y)), new Size(5, 5)));
            buffer.Graphics.FillEllipse(Brushes.Red, RectFromCenter(monster.Position, playerSize));
            buffer.Render();
            buffer.Graphics.Clear(Color.White);
        }

        private void MainForm_Shown(object _, EventArgs __)
        {
            buffer = BufferedGraphicsManager.Current.Allocate(CreateGraphics(), DisplayRectangle);
            wallHeight = DisplayRectangle.Height;
            wallsBitmap = new Bitmap(DisplayRectangle.Width + PixelSize - 1, DisplayRectangle.Height + PixelSize - 1);
            background = new Bitmap(DisplayRectangle.Width, DisplayRectangle.Height);
            for (var row = 0; row < background.Height; row++)
            {
                for (var col = 0; col < background.Width; col++)
                {
                    background.SetPixel(col, row, Color.Black);
                }
            }
            angleStep = player.FieldOfView / DisplayRectangle.Width;
            windowCenter = DisplayRectangle.Height / 2;
            gameTimer.Start();
        }

        private void MainForm_KeyDown(object _, KeyEventArgs e)
        {
            if (controls.ContainsKey(e.KeyCode) && !pressedKeys.Contains(e.KeyCode))
            {
                pressedKeys.Add(e.KeyCode);
            }
        }

        private void gameTimer_Tick(object _, EventArgs __)
        {
            if (monsterMoveTicksCounter++ == MonsterMoveTicks)
            {
                var nearbySpace = GetNearbySpace().ToArray();
                monster.Position = CenterOfRect(nearbySpace[random.Next(nearbySpace.Length)].Rect);
            }
            blocksNearby = GetNearbyBlocks();
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
                    windowCenterOffset = Math.Sin(offsetCounter++);
                    player.CenterLocation = newLocation;
                }
            }
            else
            {
                windowCenterOffset = 0;
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
