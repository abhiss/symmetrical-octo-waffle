using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace ProcGen
{
    public class MockGenerator : MonoBehaviour {
        public void Start() {
            var config = new Config();
            config.Seed =  new System.Random().Next();
            var go = Instantiate(
            new Generator().Generate(config)
                , Vector3.zero, Quaternion.identity);
        }
    }

    enum Dir : int
    {
        Up,
        Right,
        Down,
        Left,
    }

    public enum Block
    {
        None = 0,
        Front = 1,
        Left = 2,
        Back = 4,
        Right = 8,
        Ground = 16,
        Exit = 32,
        Key = 65,
        Pit = 128,
        Inner = 256,
        Outer = 512,
    }

    public class Room
    {
        public Vector3Int Size;
        public Vector3Int Pos;
        public GameObject GO;
        public Block[,] Layout;

        public Room(Vector3Int size)
        {
            Size = size;
            Layout = new Block[size.x, size.z];
        }

    }
    //     // positions are ints divided by 4
    //     public class Room
    //     {
    //         void Generate(Block[,] layout, Vector3Int pos, int rot);
    //         bool TryMerge(IRoom room);
    //         Vector3Int GetEnterancePos();
    //         Vector3Int GetExitPos();
    //         int GetExitDir();

    //     }
    //     public class ProceduralRoom : IRoom
    //     {
    //         private int _width;
    //         private int _depth;
    //         private Vector3Int _enterancePos;
    //         private Vector3Int _exitPos;
    //         private Dir _exitDir;
    //         ProceduralRoom(System._random random, int min, int range)
    //         {
    //             _width = (int)(random.NextDouble() * range + min);
    //             _depth = (int)(random.NextDouble() * range + min);
    //             _enterancePos = new Vector3Int((int)(random.NextDouble() * (range - 2) + min + 1), 0, 0);
    //             _exitDir = (Dir)(random.NextDouble() * 3);
    //             _exitPos = _exitDir switch
    //             {
    //                 Dir.Left => new Vector3Int(0, 0, (int)(random.NextDouble() * (_depth / 2 - 1) + _depth / 2)),
    //                 Dir.Down => new Vector3Int((int)(random.NextDouble() * (_width - 2) + 1), 0, _depth - 1),
    //                 Dir.Right => new Vector3Int(_width - 1, 0, (int)(random.NextDouble() * (_depth / 2 - 1) + _depth / 2)),
    //             };
    //         }
    //         void Generate(Block[,] Layout, Vector3Int pos, int rot)
    //         {

    //         }



    // }

    public class Config {
        public int Seed = 0;
        public GameObject Spawn;
    }

    public class Generator
    {
        //public static DoorSize = 
        static void RotatePremade(GameObject premade, Dir dir)
        {
            premade.transform.Rotate(new Vector3(0, 90 * (float)dir, 0));
        }

        private float TileSetScale = 4.0f;

        
        public int RoomAmt = 3;
        public int RoomSizeMin = 16;
        public int RoomSizeMax = 16;
        public int HallLenMin = 1;
        public int HallLenMax = 5;
        public int HallWidth = 2;

        public float FloorProb = 0.1f;
        public float WallProb = 0.1f;
        public float InteriorProb = 0.3f;
        public float NoiseFac = 0.5f;
        public int HalfInnerMin = 1;
        public int HalfInnerMax = 2;
        public int InnerSizeMin = 2;
        public int InnerSizeMax = 5;
        private int ExitSize = 3; // must be odd
        private const int MapSize = 512;
        private System.Random _random;
        private Dir _totalDir;
        private Dir[] _dir_cans;
        private Dir _currentDir;
        private int _cursorX;
        private int _cursorZ;
        private Block[,] _layout = new Block[MapSize, MapSize];
        private List<GameObject> Floors;
        private List<GameObject> Walls;
        private List<GameObject> WallIns;
        private List<GameObject> WallOuts;
        private List<GameObject> _topsPrefabs = new List<GameObject>();
        private List<Room> _rooms = new List<Room>();
        private GameObject _exitPrefab;
        private GameObject _columnPrefab;
        private Vector3[] _bounds;

        public GameObject Generate(Config config) {
            _random = new System.Random(config.Seed);
            _totalDir = _currentDir = (Dir)(_random.Next(0, 5));

            _cursorX = MapSize / 2;
            _cursorZ = MapSize / 2;
            return new GameObject();
            // Player.transform.position = new Vector3(_cursorX * TileSetScale + 8, 5, _cursorZ * TileSetScale + 8);

            // _dir_cans = Enumerable.Range(0, 4).Where(n => n != (((int)_totalDir + 2) & 3)).Select(n => (Dir)n).ToArray();
            // var spawn = new Room(new Vector3Int(5, 0, 5));
            // WriteRoom(spawn);
            // AdvanceDir();
            // MoveRoom(spawn);
            // WriteHall(_random.Next(HallLenMin, HallLenMax));
            // for (int i = 0; i < RoomAmt; ++i)
            // {
            //     var room = new Room(new Vector3Int(
            //         _random.Next(RoomSizeMin, RoomSizeMax),
            //         0,
            //         _random.Next(RoomSizeMin, RoomSizeMax)));
            //     _rooms.Add(room);
            //     MoveRoom(room, true);
            //     WriteRoom(room);
            //     // WriteInterior(room);
            //     AdvanceDir();
            //     MoveRoom(room);
            //     WriteHall(_random.Next(HallLenMin, HallLenMax));
            // }
            // foreach (var room in _rooms)
            // {
            //     WriteInterior(room);
            // }
            // var tp = new Room(new Vector3Int(5, 0, 5));
            // MoveRoom(tp, true);
            // WriteRoom(tp);


            // _exitPrefab = (GameObject)Resources.Load("Exit");
            // _columnPrefab = (GameObject)Resources.Load("Column_1");
            // var index = 0;

            // for (int i = 0; i < MapSize; ++i)
            // {
            //     for (int j = 0; j < MapSize; ++j)
            //     {
            //         var pos = new Vector3(i, 0, j) * TileSetScale;
            //         if ((Block.Ground & _layout[i, j]) != 0)
            //         {
            //             GeneratePrefab(Floors[index], pos, 0);
            //         }
            //         for (var b = (int)Block.Front; b <= (int)Block.Right; b *= 2)
            //         {

            //             if ((b & (int)_layout[i, j]) == b)
            //             {
            //                 GeneratePrefab(Walls[index], pos, 90 * (Mathf.Log(b, 2) - 1)); // 2 = log
            //             }
            //         }
            //         if ((_layout[i, j] & Block.Outer) != 0)
            //         {
            //             var walls = Block.Right | Block.Left | Block.Back | Block.Front;
            //             switch ((_layout[i, j] & walls))
            //             {
            //                 case Block.Back | Block.Left:
            //                     GeneratePrefab(WallOuts[index], pos, 0);
            //                     break;
            //                 case Block.Back | Block.Right:
            //                     GeneratePrefab(WallOuts[index], pos, 90);
            //                     break;
            //                 case Block.Front | Block.Right:
            //                     GeneratePrefab(WallOuts[index], pos, 180);
            //                     break;
            //                 case Block.Front | Block.Left:
            //                     GeneratePrefab(WallOuts[index], pos, 270);
            //                     break;

            //             };
            //         }
            //     }
            // }
        }

        void GenerateExit(Room room)
        {
            var target = _currentDir switch {
                Dir.Up => Block.Front,
                Dir.Right => Block.Left,
                Dir.Down => Block.Back,
                Dir.Left => Block.Right,
            };
            var cans = new List<Vector3Int>();
            for (var i = 0; i < room.Size.x; i++)
            {
                for(var j = 0; j < room.Size.z; j++)
                {
                    for (var k = 0; k < ExitSize; k++)
                    {
                        if (target != room.Layout[i, j])
                        {
                            break;
                        }
                        if (k == ExitSize - 1) {
                            cans.Add(new Vector3Int(i, 0, j));
                        }
                    }
                }
            }
            var chosen = cans[_random.Next(0, cans.Count)];
            var lat = Move(chosen, (int)_currentDir + 1 % 3);
            for (var i = 0; i < ExitSize; i++)
            {
                if (lat.x >= 0 && lat.x < room.Size.x && lat.z >= 0 && lat.z < room.Size.z) { 
                    room.Layout[lat.x, lat.z] = Block.Key;
                }
                lat = Move(lat, (int)_currentDir);
            }

            room.Layout[chosen.x, chosen.z] |= Block.Exit | Block.Key;
            for (var i = 0; i < ExitSize; i++)
            {

                var pos = Move(chosen, (int)_currentDir);
                if (i == ExitSize / 2) {
                    room.Layout[pos.x, pos.z] |= Block.Key | Block.Exit;
                } else {
                    room.Layout[pos.x, pos.z] = Block.Key;
                }
            }
                
        }

        void AddRect(Room room, int x, int z, int w, int d)
        {
            for (int i = x; i < x + w; i++)
            {
                for(int j = z; j < z + d; j++)
                {
                    var b = Block.None;
                    if (i == 0)
                    {
                        b |= Block.Front;
                    }
                    if (i == room.Size.x - 1)
                    {
                        b |= Block.Back;
                    }
                    if (j == 0)
                    {
                        b |= Block.Right ;
                    }
                    if (j == room.Size.z - 1)
                    {
                        b |= Block.Back ;
                    }
                    if (b > Block.Right)
                    {
                        b |= Block.Inner;
                    }
                    if (b == Block.None)
                    {
                        b = Block.Ground;
                    }

                }
            }
            var e1d = _currentDir;
            AdvanceDir();
        }
        // GameObject GeneratePrefab(GameObject prefab, Vector3 pos, float yRot)
        // {
        //     var go = Instantiate(prefab, pos, Quaternion.identity);
        //     go.transform.parent = gameObject.transform;
        //     go.transform.Rotate(new Vector3(-90, yRot, 0));
        //     go.transform.localScale *= 2;
        //     return go;
        // }

        void WriteWall(int x, int z, Block val)
        {
            if ((_layout[x, z] & Block.Key) != 0)
            {
                return;
            }
            if ((((_layout[x, z] == Block.Right) || (_layout[x, z] == Block.Left)) && ((val == Block.Front) || (val == Block.Back)))
             || (((_layout[x, z] == Block.Front) || (_layout[x, z] == Block.Back)) && ((val == Block.Right) || (val == Block.Left))))
            {
                _layout[x, z] |= val | Block.Outer;
                return;
            }
            else if ((_layout[x, z] & (Block.Right | Block.Left | Block.Front | Block.Back)) != 0)
            {
                _layout[x, z] |= val;
                return;
            }
            _layout[x, z] = val;


        }

        void WriteWallWeak(int x, int z, Block val)
        {
            if ((_layout[x, z] & Block.Key) == Block.Key)
            {
                return;
            }
            if (val == Block.None)
            {
                _layout[x, z] = val;
                //} 
                //else if ((((_layout[x, z] == Block.Right) || (_layout[x, z] == Block.Left)) && ((val == Block.Front) || (val == Block.Back)))
                // || (((_layout[x, z] == Block.Front) || (_layout[x, z] == Block.Back)) && ((val == Block.Right) || (val == Block.Left))))
                //{
                //    _layout[x, z] |= val | Block.Outer;
                //}
                //else if ((_layout[x, z] & (Block.Right | Block.Left | Block.Front | Block.Back)) != 0)
                //{
                //    _layout[x, z] |= val;
            }
            else
            {
                _layout[x, z] |= val;
            }


        }

        void WriteWeak(int x, int z, Block val)
        {
            if (_layout[x, z] == Block.None)
            {
                _layout[x, z] = val;
            }
        }

        bool CheckRoom(int x, int z, int width, int depth)
        {
            // var foundMask = 0;
            // Vector3 minKeyDiff;
            // var recordDist = 0;

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < depth; ++j)
                {
                    if ((_layout[x + i, z + j] & Block.Key) != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
            //         var block =_layout[_cursorX + i, _cursorZ + j];
            //         switch(block) {
            //             case Block.KeyGround: {
            //                 var minX = i < width - i ? i + 1 : i - width;
            //                 var minZ = j < depth - j ? j + 1 : j - depth;
            //                 if (abs(minX) < abs(minZ)) {
            //                     if (abs(minX) < minMag) {
            //                         minMag = abs(minX);
            //                         recordKeyPos.set(minX, 0, 0);
            //                     }
            //                 } else {
            //                     if (abs(minZ) < minMag) {
            //                         minMag = abs(minZ);
            //                         recordKeyPos.set(0, 0, minZ);
            //                     }
            //                 }
            //             break;
            //             }
            //             case Block.Right:
            //             case Block.Left:
            //             case Block.Front:
            //             case Block.Back:
            //                 if (foundMask & (block + 2) & 3) {
            //                     return null;
            //                 }
            //                 foundMask |= block & 3;
            //                 break;
            //         }
            //     }
            // }
            // if (recordKeyPos != null) {
            //     return recordKeyPos;
            // }
            // return new Vector3();
        }

        //void WriteRoom(Room room)
        //{
        //    WriteWall(_cursorX, _cursorZ, Block.Shadow);
        //    for (int i = 1; i < room.Size.x - 1; ++i)
        //    {
        //        WriteWall(_cursorX + i, _cursorZ, Block.Front | Block.Shadow);
        //    }
        //    WriteWall(_cursorX + room.Size.x - 1, _cursorZ, Block.Shadow);

        //    for (int i = 1; i < room.Size.z - 1; ++i)
        //    {
        //        WriteWall(_cursorX, _cursorZ + i, Block.Left | Block.Shadow);
        //        for (int j = 1; j < room.Size.x - 1; ++j)
        //        {
        //            WriteWeak(_cursorX + j, _cursorZ + i, Block.Ground);
        //        }
        //        WriteWall(_cursorX + room.Size.x - 1, _cursorZ + i, Block.Right | Block.Shadow);
        //    }

        //    WriteWall(_cursorX, _cursorZ + room.Size.z - 1, Block.Shadow);
        //    for (int i = 1; i < room.Size.x - 1; ++i)
        //    {
        //        WriteWall(_cursorX + i, _cursorZ + room.Size.z - 1, Block.Back | Block.Shadow);
        //    }
        //    WriteWall(_cursorX + room.Size.x - 1, _cursorZ + room.Size.z - 1, Block.Shadow);
        //}

        void WriteInterior(Room room)
        {
            //        var sym = _random.NextDouble();
            //        if (sym < .4f)
            //        {
            //            var interiorAmt = (int)(_random.NextDouble() * HalfInnerRange) + HalfInnerMin;
            //            for (int i = 0; i < interiorAmt; ++i)
            //            {
            //
            //            }
            //        }
            //        else if (sym < .8f)
            //        {
            //            var interiorAmt = (int)(_random.NextDouble() * HalfInnerRange) + HalfInnerMin;
            //            for (int i = 0; i < interiorAmt; ++i)
            //            {
            //
            //            }
            //        }
            //        else
            {
                // var interiorAmt = _random.Next(HalfInnerMin, HalfInnerMax);
                var spaceX = room.Size.x - 3;
                var spaceZ = room.Size.z - 3;
                while (spaceX > InnerSizeMin && spaceZ > InnerSizeMin)
                {
                    var innerW = _random.Next(InnerSizeMin, System.Math.Min(InnerSizeMax, spaceX));
                    var innerD = _random.Next(InnerSizeMin, System.Math.Min(InnerSizeMax, spaceZ));
                    var innerX = _random.Next(room.Pos.x, room.Pos.x + room.Size.x - innerW);
                    var innerZ = _random.Next(room.Pos.z, room.Pos.z + room.Size.z - innerD);
                    if (!CheckRoom(innerX, innerZ, innerW, innerD))
                    {
                        continue;
                    }
                    spaceX -= innerW;
                    spaceZ -= innerD;
                    WriteWallWeak(innerX, innerZ, Block.Outer | Block.Back | Block.Right );
                    for (int i = 1; i < innerW - 1; ++i)
                    {
                        WriteWallWeak(innerX + i, innerZ, Block.Back );
                    }
                    WriteWallWeak(innerX + innerW - 1, innerZ, Block.Outer | Block.Back | Block.Left );

                    for (int i = 1; i < innerD - 1; ++i)
                    {
                        WriteWallWeak(innerX, innerZ + i, Block.Right);
                        for (int j = 1; j < innerW - 1; ++j)
                        {
                            _layout[innerX + j, innerZ + i] = Block.None;
                        }
                        WriteWallWeak(innerX + innerW - 1, innerZ + i, Block.Left );
                    }

                    WriteWallWeak(innerX, innerZ + innerD - 1, Block.Outer | Block.Front | Block.Right );
                    for (int i = 1; i < innerW - 1; ++i)
                    {
                        WriteWallWeak(innerX + i, innerZ + innerD - 1, Block.Front );
                    }
                    WriteWallWeak(innerX + innerW - 1, innerZ + innerD - 1, Block.Outer | Block.Front | Block.Left );
                }
            }
        }


        void MoveRoom(Room room, bool inv = false)
        {
            var xOff = 0;
            var zOff = 0;

            if ((_currentDir == Dir.Right) || _currentDir == Dir.Left)
            {
                if (_totalDir != Dir.Right)
                {
                    zOff = (int)(_random.NextDouble() * (room.Size.z / 2 - 1  - HallWidth) +
                        ((_totalDir == Dir.Up) ^ inv ? 0 : room.Size.z / 2 - 1));
                }
                else
                {
                    zOff = (int)(_random.NextDouble() * (room.Size.z - 1 - HallWidth));
                }
                if ((_currentDir == Dir.Left && !inv) || (_currentDir == Dir.Right && inv))
                {
                    xOff = room.Size.x - 1;
                }
            }
            else
            {
                if (_totalDir == Dir.Right)
                {
                    xOff = (int)(_random.NextDouble() * (room.Size.x / 2 - 1 - HallWidth) +
                        ((_totalDir == Dir.Right) ^ inv ? 0 : room.Size.x / 2 - 1));
                }
                else
                {
                    xOff = (int)(_random.NextDouble() * (room.Size.x - 1 - HallWidth));
                }
                if ((_currentDir == Dir.Down && !inv) || (_currentDir == Dir.Up && inv))
                {
                    zOff = room.Size.z - 1;
                }
            }
            Debug.Log("x off" + xOff);
            Debug.Log(zOff);

            if (inv)
            {
                _cursorX -= xOff;
                _cursorZ -= zOff;
                room.Pos = new Vector3Int(
                    _cursorX, 0, _cursorZ
                );
            }
            else
            {
                _cursorX += xOff;
                _cursorZ += zOff;
            }
        }

        Vector3Int Move(Vector3Int pos, int dir)
        {
            switch ((Dir)dir)
            {
                case Dir.Up:
                    pos.z -= 1;
                    break;
                case Dir.Right:
                    pos.x -= 1;
                    break;
                case Dir.Down:
                    pos.z += 1;
                    break;
                case Dir.Left:
                    pos.x += 1;
                    break;
            }
            return pos;
        }

        // void WriteHall(int len)
        // {

        //     bool isHor = _currentDir == Dir.Right || _currentDir == Dir.Left ;
        //     int xOff = isHor ? 0 : 1;
        //     int zOff = isHor ? 1 : 0;
        //     var wall = _currentDir switch
        //     {
        //         Dir.Up => Block.Right,
        //         Dir.Right => Block.Front,
        //         Dir.Down => Block.Left,
        //         Dir.Left => Block.Back,
        //     };
        //     MoveDir(((int)_currentDir + 2) & 3);
        //     var x0 = _cursorX;
        //     var z0 = _cursorZ;
        //     len += 2;
        //     for (int i = 0; i < len; i++)
        //     {
        //         WriteWall(_cursorX, _cursorZ, wall);
        //         MoveDir((int)_currentDir);
        //     }

        //     for (int i = 0; i < HallWidth; i++)
        //     {
        //         if (isHor)
        //         {
        //             _cursorX = x0;
        //         }
        //         else
        //         {
        //             _cursorZ = z0;
        //         }
        //         MoveDir(((int)_currentDir + 1) & 3);
        //         for (int j = 0; j < len; j++)
        //         {
        //             _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        //             MoveDir((int)_currentDir);
        //         }
        //     }
        //     if (isHor)
        //     {
        //         _cursorX = x0;
        //     }
        //     else
        //     {
        //         _cursorZ = z0;
        //     }
        //     MoveDir(((int)_currentDir + 1) & 3);

        //     wall = _currentDir switch
        //     {
        //         Dir.Up => Block.Left,
        //         Dir.Right => Block.Back,
        //         Dir.Down => Block.Right,
        //         Dir.Left => Block.Front,
        //     };
        //     for (int i = 0; i < len; i++)
        //     {
        //         WriteWall(_cursorX, _cursorZ, wall);
        //         MoveDir((int)_currentDir);
        //     }
        //     //_layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        //     //MoveDir((int)_currentDir);
        //     //_layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        //     //MoveDir((int)_currentDir);
        //     //_layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        //     //WriteWall(_cursorX - xOff, _cursorZ - zOff, b1);
        //     //for (int i = 0; i < len; ++i)
        //     //{
        //     //    MoveDir((int)_currentDir);
        //     //    _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;//isHor ? 'h' : 'v';
        //     //    _layout[_cursorX + xOff, _cursorZ + zOff] = b0;
        //     //    _layout[_cursorX - xOff, _cursorZ - zOff] = b1;
        //     //}
        //     //MoveDir((int)_currentDir);
        //     //_layout[_cursorX, _cursorZ] = Block.Key | Block.Ground; //isHor ? 'h' : 'v';
        //     //_layout[_cursorX + xOff, _cursorZ + zOff] = b0;
        //     //_layout[_cursorX - xOff, _cursorZ - zOff] = b1;
        //     //MoveDir((int)_currentDir);
        //     //_layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        //     //_layout[_cursorX + xOff, _cursorZ + zOff] = b0;
        //     //_layout[_cursorX - xOff, _cursorZ - zOff] = b1;
        //     //MoveDir((int)_currentDir);
        //     //_layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        //     MoveDir(((int)_currentDir + 2) & 3);
        //     MoveDir(((int)_currentDir + 2) & 3);
        // }

        void AdvanceDir()
        {
            if (_currentDir == _totalDir)
            {
                _currentDir = _dir_cans[(int)(_random.NextDouble() * 3)];
            }
            else
            {
                var dir_cans = _dir_cans.Where(n => n != (Dir)(((int)_currentDir + 2) & 3)).ToArray();
                _currentDir = dir_cans[(int)(_random.NextDouble() * 2)];
            }
        }

    }
}
