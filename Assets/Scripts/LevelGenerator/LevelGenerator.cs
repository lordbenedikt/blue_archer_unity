using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethods
{
    public enum RoomTile
    {
        Empty = -1,
        Wall = 0,
        Ground = 1,
        Exit = 2,
        Torch = 3,
        Chest = 4,
        Spikes = 5,
    }
    public class Level
    {
        public RoomTile[,] tiles;
        public RoomTile[,] objects;
        public float[,] objectAngles;
        List<Room> rooms = new List<Room>();
        List<Vector2Int> entrances = new List<Vector2Int>();
        public Level(int width, int height)
        {
            tiles = new RoomTile[width, height];
            objects = new RoomTile[width, height];
            for (int i = 0; i < objects.GetLength(0); i++)
            {
                for (int j = 0; j < objects.GetLength(1); j++)
                {
                    objects[i, j] = RoomTile.Empty;
                }
            }
            objectAngles = new float[width, height];
        }
        public Room FromPos(int x, int y)
        {
            foreach (Room room in rooms)
            {
                if (x >= room.MinX && x <= room.MaxX && y >= room.MinY && y <= room.MaxY)
                {
                    return room;
                }
            }
            return null;
        }
        public void FindRooms()
        {
            for (int i = 0; i < tiles.GetLength(1); i++)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    if (LevelGenerator.IsEntrance(i, j))
                    {
                        entrances.Add(new Vector2Int(i, j));
                    }
                    else if (tiles[i, j] == RoomTile.Ground && FromPos(i, j) == null)
                    {
                        int minX = i;
                        int minY = j;
                        int maxX = i;
                        int maxY = j;
                        while (tiles[maxX + 1, maxY] != RoomTile.Wall && !LevelGenerator.IsEntrance(maxX + 1, maxY))
                        {
                            maxX++;
                        }
                        while (tiles[maxX, maxY + 1] != RoomTile.Wall && !LevelGenerator.IsEntrance(maxX, maxY + 1))
                        {
                            maxY++;
                        }
                        rooms.Add(new Room(minX, minY, maxX, maxY));
                    }
                }
            }
        }
        public void PlaceTorch(List<Vector2Int> torchPositions, Room room)
        {
            int count = 30;
            while (true)
            {
                int _x = UnityEngine.Random.Range(room.MinX, room.MaxX);
                int _y = UnityEngine.Random.Range(room.MinY, room.MaxY);
                float objectAngle = 0;

                int closest = 0;
                float minDist = Math.Abs(_y - room.MaxY);
                if (Math.Abs(_x - room.MaxX) < minDist)
                {
                    minDist = Math.Abs(_x - room.MaxX);
                    closest = 1;
                }
                if (Math.Abs(_y - room.MinY) < minDist)
                {
                    minDist = Math.Abs(_y - room.MinY);
                    closest = 2;
                }
                if (Math.Abs(_x - room.MinX) < minDist)
                {
                    minDist = Math.Abs(_x - room.MinX);
                    closest = 3;
                }

                if (closest == 0)
                {
                    _y = room.MaxY + 1;
                    objectAngle = 270;
                }
                else if (closest == 1)
                {
                    _x = room.MaxX + 1;
                    objectAngle = 0;
                }
                else if (closest == 2)
                {
                    _y = room.MinY - 1; ;
                    objectAngle = 90;
                }
                else if (closest == 3)
                {
                    _x = room.MinX - 1; ;
                    objectAngle = 180;
                }

                minDist = 9999;
                foreach (Vector2Int pos in torchPositions)
                {
                    float curDist = Vector2.Distance(new Vector2(_x, _y), pos);
                    if (curDist < minDist)
                    {
                        minDist = curDist;
                    }
                }
                if (minDist < 5 || tiles[_x, _y] != RoomTile.Wall || LevelGenerator.CountAdjacent(RoomTile.Wall, _x, _y) < 2)
                {
                    if (count == 0)
                    {
                        break;
                    }
                    else
                    {
                        count--;
                        continue;
                    }
                }
                else
                {
                    objectAngles[_x, _y] = objectAngle;
                    objects[_x, _y] = RoomTile.Torch;
                    torchPositions.Add(new Vector2Int(_x, _y));
                    break;
                }
            }
        }
        public void PlaceTorches(int frequency)
        {
            List<Vector2Int> torchPositions = new List<Vector2Int>();
            foreach (Room room in rooms)
            {
                for (int i = 0; i < room.GetSurfaceArea() / 16; i++)
                {
                    PlaceTorch(torchPositions, room);
                }
            }
        }
        public void PlaceSpikes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int free = LevelGenerator.RandomFreePos();
                objects[free.x, free.y] = RoomTile.Spikes;
            }
        }
    }
    public class Room
    {
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        List<Vector2Int> exit = new List<Vector2Int>();
        public Room(int minX, int minY, int maxX, int maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }
        public int GetSurfaceArea()
        {
            return (MaxX - MinX) * (MaxY - MinY);
        }
    }
    public static class LevelGenerator
    {
        static System.Random random = new System.Random();
        static int levelWidth;
        static int levelHeight;
        public static Level level;

        public static Level Generate(int width, int height)
        {
            Level generatedLevel = new Level(width, height);
            levelWidth = width;
            levelHeight = height;

            ResetCells(generatedLevel);
            RandomDivider(generatedLevel);
            CleanMap(generatedLevel);

            level = generatedLevel;
            level.FindRooms();

            return level;
        }

        static void PrintLevel(Level level)
        {
            for (int i = 0; i < level.tiles.GetLength(1); i++)
            {
                for (int j = 0; j < level.tiles.GetLength(0); j++)
                {
                    Debug.Log((level.tiles[j, i] == 0 ? "E" : (level.tiles[j, i] == RoomTile.Ground ? "_" : " ")) + " ");
                }
                Console.Write("\n");
            }
        }

        static void CleanMap(Level level)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int count = 0;
                    if (j != 0 && level.tiles[i, j - 1] == RoomTile.Ground) count++;
                    if (i != 0 && level.tiles[i - 1, j] == RoomTile.Ground) count++;
                    if (level.tiles[i, j] == RoomTile.Ground) count++;
                    if (i != w - 1 && level.tiles[i + 1, j] == RoomTile.Ground) count++;
                    if (j != h - 1 && level.tiles[i, j + 1] == RoomTile.Ground) count++;
                    if (level.tiles[i, j] == RoomTile.Ground && count == 1 || count == 2)
                    {
                        level.tiles[i, j] = 0;
                    }
                }
            }
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int count = 0;
                    if (j != 0 && level.tiles[i, j - 1] == RoomTile.Ground) count++;
                    if (i != 0 && level.tiles[i - 1, j] == RoomTile.Ground) count++;
                    if (level.tiles[i, j] == RoomTile.Ground) count++;
                    if (i != w - 1 && level.tiles[i + 1, j] == RoomTile.Ground) count++;
                    if (j != h - 1 && level.tiles[i, j + 1] == RoomTile.Ground) count++;
                    if (i != 0 && j != 0 && level.tiles[i - 1, j - 1] == RoomTile.Ground) count++;
                    if (i != w - 1 && j != 0 && level.tiles[i + 1, j - 1] == RoomTile.Ground) count++;
                    if (i != 0 && j != h - 1 && level.tiles[i - 1, j + 1] == RoomTile.Ground) count++;
                    if (i != w - 1 && j != h - 1 && level.tiles[i + 1, j + 1] == RoomTile.Ground) count++;
                    if (count == 0)
                    {
                        level.tiles[i, j] = RoomTile.Empty;
                    }
                }
            }
        }

        static void DFS(Level level, int pos, List<int> visited, List<Vector2Int> notFree)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            if (visited.Contains(pos))
            {
                return;
            }
            foreach (Vector2Int v in notFree)
            {
                if (v.x == pos % w && v.y == pos / w)
                {
                    return;
                }
            }
            if (level.tiles[pos % w, pos / w] == RoomTile.Ground)
            {
                visited.Add(pos);
                if (pos % w != w - 1) DFS(level, pos + 1, visited, notFree);
                if (pos / w != h - 1) DFS(level, pos + w, visited, notFree);
                if (pos % w != 0) DFS(level, pos - 1, visited, notFree);
                if (pos / w != 0) DFS(level, pos - w, visited, notFree);
            }
            else
            {
                return;
            }
        }
        
        static bool IsConnected(Level level)
        {
            return IsConnected(level, new List<Vector2Int>());
        }

        static bool IsConnected(Level level, List<Vector2Int> notFree)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            List<int> visited = new List<int>();
            int pos = -1;
            for (int i = 0; i < w * h; i++)
            {
                if (level.tiles[i % w, i / w] == RoomTile.Ground)
                {
                    pos = i;
                    break;
                }
            }
            if (pos != -1)
            {
                DFS(level, pos, visited, notFree);
            }
            int free = 0;
            for (int i = 0; i < w * h; i++)
            {
                if (level.tiles[i % w, i / w] == RoomTile.Ground)
                {
                    free++;
                }
            }
            if (free == visited.Count - notFree.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Randomly divides the room into smaller rooms by inserting walls.
        /// </summary>
        /// <param name="level"></param>
        static void RandomDivider(Level level)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            DivideRoom(level, 5, 1, w - 2, 1, h - 2, 0, 3, true);
        }

        /// <summary>
        ///     Cuts out a rectangle of tiles in the level, i.e. sets them to 'Empty' tiles.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        static void CutoutRect(RoomTile[,] level, int x1, int x2, int y1, int y2)
        {
            for (int i = x1; i <= x2; i++)
            {
                for (int j = y1; j <= y2; j++)
                {
                    level[i, j] = RoomTile.Empty;
                }
            }
        }

        /// <summary>
        ///     Frees a rectangle of tiles in the level, i.e. sets them to 'Ground' tiles.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        static void FreeRect(Level level, int x1, int x2, int y1, int y2)
        {
            for (int i = x1; i <= x2; i++)
            {
                for (int j = y1; j <= y2; j++)
                {
                    level.tiles[i, j] = RoomTile.Ground;
                }
            }
        }

        /// <summary>
        ///     Divides the room into smaller rooms recursively.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="iter">A measure for how many times the room should be divided, though it varies from the actual number of splits.</param>
        /// <param name="x1">The rooms leftmost position</param>
        /// <param name="x2">The rooms rightmost position</param>
        /// <param name="y1">The rooms bottommost position</param>
        /// <param name="y2">The rooms topmost position</param>
        /// <param name="omit">The position of the entrance. The room shouldn't be split here.</param>
        /// <param name="minRoomSize">A measure for the minimum room size (not the actual width or height)</param>
        /// <param name="horizontal">Whether the room should be split horizontally.</param>
        static void DivideRoom(Level level, int iter, int x1, int x2, int y1, int y2, int omit, int minRoomSize, bool horizontal)
        {
            Boolean cutout = (x2 - x1 > 15 || y2 - y1 > 15) ? false : random.NextDouble() < 0.7;
            iter -= random.Next(2);
            if (iter <= 0) return;
            int at = 0;
            if (horizontal)
            {
                if (y2 - y1 < minRoomSize * 2 + 2) return;
                do
                {
                    at = y1 + minRoomSize + random.Next(y2 - y1 - minRoomSize * 2);
                } while (at == omit);
                int nextOmit = random.Next(x2 - x1) + x1;
                for (int i = x1; i <= x2; i++)
                {
                    if (i == nextOmit) continue;
                    level.tiles[i, at] = 0;
                }
                if (at < omit && cutout)
                {
                    CutoutRect(level.tiles, x1, x2, y1, at - 1);
                    if (!IsConnected(level))
                    {
                        FreeRect(level, x1, x2, y1, at - 1);
                        DivideRoom(level, iter - 1, x1, x2, y1, at - 1, nextOmit, minRoomSize, false);
                    }
                }
                else
                {
                    DivideRoom(level, iter - 1, x1, x2, y1, at - 1, nextOmit, minRoomSize, false);

                }
                if (at > omit && cutout)
                {
                    CutoutRect(level.tiles, x1, x2, at + 1, y2);
                    if (!IsConnected(level))
                    {
                        FreeRect(level, x1, x2, at + 1, y2);
                        DivideRoom(level, iter - 1, x1, x2, at + 1, y2, nextOmit, minRoomSize, false);
                    }
                }
                else
                {
                    DivideRoom(level, iter - 1, x1, x2, at + 1, y2, nextOmit, minRoomSize, false);
                }
            }
            else
            {
                if (x2 - x1 < minRoomSize * 2 + 2) return;
                //print(random(3) + "\n");
                do
                {
                    at = x1 + minRoomSize + random.Next(x2 - x1 - minRoomSize * 2);
                } while (at == omit);
                int nextOmit = random.Next(y2 - y1) + y1;
                for (int i = y1; i <= y2; i++)
                {
                    if (i == nextOmit) continue;
                    level.tiles[at, i] = 0;
                }
                DivideRoom(level, iter - 1, x1, at - 1, y1, y2, nextOmit, minRoomSize, true);
                DivideRoom(level, iter - 1, at + 1, x2, y1, y2, nextOmit, minRoomSize, true);
            }
        }

        static void AddRandomWalls(Level level)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            int iterations = 5000;
            for (int i = 0; i < iterations; i++)
            {
                int cell = random.Next(levelWidth * levelHeight);
                int col = cell % w;
                int row = cell / w;
                if (col == 0 || col == w - 1 || row == 0 || row == h - 1)
                {
                    continue;
                }
                RoomTile beforeChange = level.tiles[col, row];
                level.tiles[col, row] = 0;
                int count = 0;
                if (level.tiles[col - 1, row] == RoomTile.Ground) count++;
                if (level.tiles[col + 1, row] == RoomTile.Ground) count++;
                if (level.tiles[col, row - 1] == RoomTile.Ground) count++;
                if (level.tiles[col, row + 1] == RoomTile.Ground) count++;

                if (!IsConnected(level) || count == 4 || count == 2 || count == 1 || count == 0)
                {
                    level.tiles[col, row] = beforeChange;
                }
            }
        }

        /// <summary>
        ///     Resets the level to a rectangle of ground tiles surrounded by walls.
        /// </summary>
        /// <param name="level"></param>
        static void ResetCells(Level level)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    level.tiles[i, j] = RoomTile.Wall;
                }
            }
            for (int j = 1; j < h - 1; j++)
            {
                for (int i = 1; i < w - 1; i++)
                {
                    level.tiles[i, j] = RoomTile.Ground;
                }
            }
        }

        static void RandomAutomaton(Level level)
        {
            int w = level.tiles.GetLength(0);
            int h = level.tiles.GetLength(1);
            int[] cells = new int[(w - 2) * (h - 2)];
            for (int j = 1; j < h - 1; j++)
            {
                for (int i = 1; i < w - 1; i++)
                {
                    level.tiles[i, j] = random.NextDouble() < 0.5 ? RoomTile.Ground : RoomTile.Wall;
                }
            }

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = i;
            }
            ShuffleArray(cells);

            for (int c = 0; c < cells.Length; c++)
            {
                int i = cells[c] % (w - 2) + 1;
                int j = cells[c] / (w - 2) + 1;

                int count = 0;
                if (level.tiles[i - 1, j - 1] == RoomTile.Ground) count++;
                if (level.tiles[i, j - 1] == RoomTile.Ground) count++;
                if (level.tiles[i + 1, j - 1] == RoomTile.Ground) count++;
                if (level.tiles[i - 1, j] == RoomTile.Ground) count++;
                if (level.tiles[i, j] == RoomTile.Ground) count++;
                if (level.tiles[i + 1, j] == RoomTile.Ground) count++;
                if (level.tiles[i - 1, j + 1] == RoomTile.Ground) count++;
                if (level.tiles[i, j + 1] == RoomTile.Ground) count++;
                if (level.tiles[i + 1, j + 1] == RoomTile.Ground) count++;

                if (count >= 5)
                {
                    level.tiles[i, j] = RoomTile.Ground;
                }
                else
                {
                    level.tiles[i, j] = RoomTile.Wall;
                }
            }
        }

        /// <summary>
        ///     Generates a list of free positions on the map that are not at an entrance. (because those areas should stay clear)
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static List<Vector2Int> GetFreePositionsNotAtEntrance(int w, int h)
        {
            List<Vector2Int> freePositions = new List<Vector2Int>();
            for (int i = 0; i < level.tiles.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < level.tiles.GetLength(1) - 1; j++)
                {
                    // wxh area is free
                    for (int k = i; k < i + w; k++)
                    {
                        for (int l = j; l < j + h; l++)
                        {
                            if (GetTile(k, l) != RoomTile.Ground)
                            {
                                goto end_of_loop;
                            }
                        }
                    }
                    for (int k = i; k < i + w; k++)
                    {
                        if (IsEntrance(k, j - 1)
                        || IsEntrance(k, j + h))
                        {
                            goto end_of_loop;
                        }
                    }
                    for (int l = j; l < j + h; l++)
                    {
                        if (IsEntrance(i - 1, l)
                        || IsEntrance(i + w, l))
                        {
                            goto end_of_loop;
                        }
                    }
                    freePositions.Add(new Vector2Int(i, j));
                end_of_loop: { }
                }
            }
            return freePositions;
        }

        /// <summary>
        ///     Generates a random position that is free (i.e. a random empty 'Ground' Tile on the map).
        /// </summary>
        /// <returns></returns>
        public static Vector2Int RandomFreePos()
        {
            List<Vector2Int> freeSpots = new List<Vector2Int>();
            for (int i = 0; i < level.tiles.GetLength(0); i++)
            {
                for (int j = 0; j < level.tiles.GetLength(1); j++)
                {
                    if (level.tiles[i, j] == RoomTile.Ground)
                    {
                        freeSpots.Add(new Vector2Int(i, j));
                    }
                }
            }
            return freeSpots[random.Next(freeSpots.Count)];
        }

        /// <summary>
        ///     Gets the tile at the given coordinates (i, j).
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static RoomTile GetTile(int i, int j)
        {
            if (i < 0 || j < 0 || i >= level.tiles.GetLength(0) || j >= level.tiles.GetLength(1))
            {
                return RoomTile.Empty;
            }

            if (level.objects[i, j] != RoomTile.Empty)
            {
                return level.objects[i, j];
            }
            else
            {
                return level.tiles[i, j];
            }
        }

        static RoomTile GetLeft(int i, int j)
        {
            return GetTile(i - 1, j);
        }

        static RoomTile GetRight(int i, int j)
        {
            return GetTile(i + 1, j);
        }

        static RoomTile GetAbove(int i, int j)
        {
            return GetTile(i, j + 1);
        }

        static RoomTile GetBelow(int i, int j)
        {
            return GetTile(i, j - 1);
        }

        /// <summary>
        ///     Counts the number of adjacent tiles that match the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static int CountAdjacent(RoomTile value, int i, int j)
        {
            int res = 0;
            if (GetLeft(i, j) == value) res++;
            if (GetRight(i, j) == value) res++;
            if (GetAbove(i, j) == value) res++;
            if (GetBelow(i, j) == value) res++;
            return res;
        }

        /// <summary>
        ///     Checks if the tile at (i, j) is an entrance.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public static bool IsEntrance(int i, int j)
        {
            if (CountAdjacent(RoomTile.Wall, i, j) == 2)
            {
                if (GetLeft(i, j) == RoomTile.Wall && GetRight(i, j) == RoomTile.Wall) return true;
                if (GetAbove(i, j) == RoomTile.Wall && GetBelow(i, j) == RoomTile.Wall) return true;
            }
            return false;
        }

        /// <summary>
        ///     Shuffles the array in place.
        /// </summary>
        /// <param name="ar"></param>
        static void ShuffleArray(int[] ar)
        {
            for (int i = ar.Length - 1; i > 0; i--)
            {
                int index = random.Next(i + 1);
                // Simple swap
                int a = ar[index];
                ar[index] = ar[i];
                ar[i] = a;
            }
        }
    }
}