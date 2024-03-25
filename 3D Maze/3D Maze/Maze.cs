using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    internal sealed class Maze
    {
        private const int GeneratorStep = 2;
        private const int WallsThickness = GeneratorStep - 1;
        private readonly static Random randomizer = new();
        private readonly Point start = new(WallsThickness, WallsThickness);

        public bool[,] Walls { get; }

        public Point Finish { get; private set; }

        public Maze(ref int size)
        {
            if (size % 2 == 0)
            {
                size++;
            }

            Walls = new bool[size + WallsThickness * 2, size + WallsThickness * 2];
            size += WallsThickness * 2;
            InitWalls();
            Generate(start);
            CreateFinish();
        }

        private void Generate(Point from)
        {
            //Make start
            Walls[start.Y, start.X] = false;
            var toVisitList = GetNeighbors(from);
            while (toVisitList.Count > 0)
            {
                var cellToVisit = toVisitList[randomizer.Next(toVisitList.Count)];
                if (Walls[cellToVisit.Y, cellToVisit.X])
                {
                    Visit(cellToVisit, from);
                    Generate(cellToVisit);
                }
                toVisitList.Remove(cellToVisit);
            }
        }

        private void CreateFinish()
        {
            if (randomizer.Next(2) == 0)
            {
                var x = randomizer.Next((Walls.GetLength(0) / 2 - 1)) * 2 + 1;
                Walls[Walls.GetLength(1) - 1, x] = false;
                Finish = new Point(x, Walls.GetLength(1) - 1);
            }
            else
            {
                var y = randomizer.Next((Walls.GetLength(1) / 2 - 1)) * 2 + 1;
                Walls[y, Walls.GetLength(0) - 1] = false;
                Finish = new Point(Walls.GetLength(0) - 1, y);
            }
        }

        private void InitWalls()
        {
            for (var row = 0; row < Walls.GetLength(0); row++)
            {
                for (var col = 0; col < Walls.GetLength(1); col++)
                {
                    Walls[row, col] = true;
                }
            }
        }

        private List<Point> GetNeighbors(Point point)
        {
            var length = Walls.GetLength(0);
            var list = new List<Point>();
            if (point.X - GeneratorStep >= 0)
            {
                list.Add(point with { X = point.X - GeneratorStep });
            }

            if (point.X + GeneratorStep < length)
            {
                list.Add(point with { X = point.X + GeneratorStep });
            }

            if (point.Y - GeneratorStep >= 0)
            {
                list.Add(point with { Y = point.Y - GeneratorStep });
            }

            if (point.Y + GeneratorStep < length)
            {
                list.Add(point with { Y = point.Y + GeneratorStep });
            }

            return list;
        }

        private void Visit(Point point, Point from)
        {
            var rowMultiplier = from.Y < point.Y ? 1 : -1;
            var colMultiplier = from.X < point.X ? 1 : -1;
            for (var row = from.Y; row != point.Y; row += 1 * rowMultiplier)
            {
                Walls[row, from.X] = false;
            }

            for (var col = from.X; col != point.X; col += 1 * colMultiplier)
            {
                Walls[from.Y, col] = false;
            }

            Walls[point.Y, point.X] = false;
        }
    }
}
