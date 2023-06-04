using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Unity.AI.Navigation;

namespace ProcGen
{

    public enum Dir
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
        Inner = 64,
        Outer = 128,
        Pit = 256,
        Key = 512,
    }

    enum Prefab
    {
        Wall,
        LitWall,
        Ground,
        LitGround,
        Inner,
        Outer,
        Exit,
        Amt,
    }

    public class Room
    {
        public Vector3Int Size;
        public Vector3Int Pos = new Vector3Int();
        public GameObject GO = new GameObject();
        public Block[,] Blocks;
        public List<Vector3Int> Exits = new List<Vector3Int>();

        public Room(Vector3Int size)
        {
            Size = size;
            Blocks = new Block[size.x, size.z];
        }

        public void AddRect(int x, int z, int w, int d)
        {
            for (int i = x; i < x + w; i++)
            {
                for (int j = z; j < z + d; j++)
                {
                    Blocks[i, j] = Block.None;
                    if (j == z)
                    {
                        Blocks[i, j] |= Block.Back;
                    }
                    if (j == z + d - 1)
                    {
                        Blocks[i, j] |= Block.Front;
                    }
                    if (i == x)
                    {
                        Blocks[i, j] = Blocks[i, j] switch
                        {
                            Block.Back => Block.Right | Block.Inner,
                            Block.Front => Block.Front | Block.Inner,
                            _ => Block.Right,
                        };
                    }
                    if (i == x + w - 1)
                    {
                        Blocks[i, j] = Blocks[i, j] switch
                        {
                            Block.Back => Block.Back | Block.Inner,
                            Block.Front => Block.Left | Block.Inner,
                            _ => Block.Left,
                        };
                    }
                    if ((Blocks[i, j] & (Blocks[i, j] - 1)) != Block.None)
                    {
                        Blocks[i, j] |= Block.Inner;
                    }
                    if (Blocks[i, j] == Block.None)
                    {
                        Blocks[i, j] = Block.Ground;
                    }
                }
            }
        }
        public void MoveAfter(Room prev)
        {
            Pos -= Exits[0];
            Pos += prev.Pos + prev.Exits[1];
        }
        public void BuildGO(GameObject[] blocks, float scale, System.Func<GameObject, Vector3, Quaternion, GameObject> instantiate)
        {
            for (int i = 0; i < Size.x; ++i)
            {
                for (int j = 0; j < Size.z; ++j)
                {
                    var block = Blocks[i, j] switch
                    {
                        Block b when ((b & Block.Ground) == Block.Ground) =>
                            instantiate(blocks[(int)Prefab.Ground], new Vector3(i, 0, j) * scale, Quaternion.identity),
                        Block.Front or Block.Left or Block.Back or Block.Right =>
                            instantiate(blocks[(int)Prefab.Wall], new Vector3(i, 0, j) * scale, Quaternion.Euler(0, (float)System.Math.Log((int)Blocks[i, j], 2) * 90, 0)),
                        Block b when ((b & Block.Outer) == Block.Outer) =>
                            instantiate(blocks[(int)Prefab.Outer], new Vector3(i, 0, j) * scale, Quaternion.AngleAxis((float)System.Math.Log((int)(b & (Block.Ground - 1)), 2) * 90, Vector3.up)),
                        Block b when ((b & Block.Inner) == Block.Inner) =>
                            instantiate(blocks[(int)Prefab.Inner], new Vector3(i, 0, j) * scale, Quaternion.AngleAxis((float)System.Math.Log((int)(b & (Block.Ground - 1)), 2) * 90, Vector3.up)),
                        Block b when ((b & Block.Exit) == Block.Exit) =>
                            instantiate(blocks[(int)Prefab.Exit], new Vector3(i, 0, j) * scale, Quaternion.Euler(0, (float)System.Math.Log((int)(b & (Block.Ground - 1)), 2) * 90, 0)),
                        _ => null,

                    };
                    if (block)
                    {
                        block.transform.parent = GO.transform;
                    }
                }
            }
            GO.transform.position = Pos;
            GO.transform.position *= scale;
        }
    }

    public class Config
    {
        public int Seed = 0;
        public GameObject Spawn;
        public System.Func<GameObject, Vector3, Quaternion, GameObject> instantiate;
        public GameObject GO; // parent needs nav mesh surface
    }

    public class Generator
    {
        //public static DoorSize = 
        static void RotatePremade(GameObject premade, Dir dir)
        {
            premade.transform.Rotate(new Vector3(0, 90 * (float)dir, 0));
        }

        private float BlockSize = 4.0f;


        public int RoomAmt = 3;
        public int RoomSizeMin = 8;
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
        private int ExitStretch = 1; // must be odd
        private const int MapSize = 512;
        private System.Random _random;
        private Dir _totalDir;
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

        public void Generate(Config config)
        {
            _random = new System.Random(config.Seed);
            _totalDir = _currentDir = (Dir)(_random.Next(0, 4));

            _cursorX = MapSize / 2;
            _cursorZ = MapSize / 2;
            var rooms = new List<Room>();

            var spawn = new Room(new Vector3Int(_random.Next(RoomSizeMin + 2, RoomSizeMax + 2), 0, _random.Next(RoomSizeMin + 2, RoomSizeMax + 2)));
            spawn.AddRect(1, 1, spawn.Size.x - 2, spawn.Size.z - 2);
            spawn.Exits.Add(new Vector3Int());
            GenerateExit(spawn);
            rooms.Add(spawn);

            for (int i = 0; i < RoomAmt; ++i)
            {
                var prev = rooms.Last();
                var range = PrevRange();
                var xMin = RoomSizeMin + 2;
                var zMin = RoomSizeMin + 2;
                if (_currentDir != _totalDir)
                {
                    if ((_totalDir & Dir.Right) == Dir.Right)
                    {
                        xMin = prev.Size.x - range + 3;
                    }
                    else
                    {
                        zMin = prev.Size.z - range + 3;
                    }
                }
                var room = new Room(new Vector3Int(_random.Next(xMin + 2, RoomSizeMax + 2), 0, _random.Next(zMin + 2, RoomSizeMax + 2)));
                room.AddRect(1, 1, room.Size.x - 2, room.Size.z - 2);
                GenerateExit(room, range);
                AdvanceDir();
                GenerateExit(room);
                room.MoveAfter(rooms.Last());
                _rooms.Add(room);
            }

            var finish = new Room(new Vector3Int(_random.Next(RoomSizeMin + 2, RoomSizeMax + 2), 0, _random.Next(RoomSizeMin + 2, RoomSizeMax + 2)));
            finish.AddRect(1, 1, finish.Size.x - 2, finish.Size.z - 2);
            GenerateExit(finish, PrevRange());
            finish.MoveAfter(_rooms.Last());
            _rooms.Add(finish);
            var blocks0 = Enumerable.Range(0, (int)Prefab.Amt)
                            .Select(n => (GameObject)Resources.Load("Blocks0/" + ((Prefab)n).ToString())).ToArray<GameObject>();
            // var blocks1 = Enumerable.Range(0, (int)Prefab.Amt)
            //                 .Select(n => (GameObject)Resources.Load("Blocks1/" + ((Prefab)n).ToString())).ToArray<GameObject>();
            foreach (Room room in _rooms)
            {
                room.BuildGO(blocks0, BlockSize, config.instantiate);
                room.GO.transform.parent = config.GO.transform;
            }
            var surface = config.GO.GetComponent<NavMeshSurface>();
            // surface.BuildNavMesh();

        }

        int PrevRange() {
            Room prev = _rooms.Last();
            if (prev == null) {
                return 0;
            }
            return _totalDir switch
            {
                Dir.Up => prev.Exits[1].z,
                Dir.Right => prev.Exits[1].x,
                Dir.Down => prev.Size.z - prev.Exits[1].z,
                Dir.Left => prev.Size.x - prev.Exits[1].x,
            };
        }
        void GenerateExit(Room room, int invRange = 0)
        {
            var dir = invRange != 0 ? (_currentDir + 2) & Dir.Left : _currentDir;
            var target = dir switch
            {
                Dir.Up => Block.Front,
                Dir.Right => Block.Left,
                Dir.Down => Block.Back,
                Dir.Left => Block.Right,
            };
            var cans = new List<Vector3Int>();
            int x0 = 0;
            int xn = room.Size.x;
            int z0 = 0;
            int zn = room.Size.z;
            Dir adjDir = (dir + 1) & Dir.Left;
            Debug.Log(dir + " pre: " + " x0: " + x0 + " xn: " + xn + " z0: " + z0 + " zn: " + zn);
            if (invRange != 0)
            {
                if (_totalDir != _currentDir)
                {
                    // limit bounds
                    switch (_totalDir)
                    {
                        case Dir.Up:
                            zn = System.Math.Min(zn, invRange + 1);
                            break;
                        case Dir.Right:
                            xn = System.Math.Min(xn, invRange + 1);
                            break;
                        case Dir.Down:
                            z0 = System.Math.Max(z0, zn - invRange);
                            break;
                        case Dir.Left:
                            x0 = System.Math.Max(x0, xn - invRange);
                            break;
                    }
                }
            }
            Debug.Log(invRange + " x0: " + x0 + " xn: " + xn + " z0: " + z0 + " zn: " + zn);
            // filter candidates
            for (var i = x0; i < xn; i++)
            {
                for (var j = z0; j < zn; j++)
                {
                    var current = new Vector3Int(i, 0, j);
                    for (var k = 0; k < ExitStretch + 1; k++)
                    {
                        if (target != room.Blocks[current.x, current.z])
                        {
                            goto next;
                        }
                        current = DirMove(adjDir, current);
                    }
                    current = new Vector3Int(i, 0, j);
                    for (var k = 0; k < ExitStretch; k++)
                    {
                        current = DirMove(adjDir, current, -1);
                        if (target != room.Blocks[current.x, current.z])
                        {
                            goto next;
                        }
                    }
                    cans.Add(new Vector3Int(i, 0, j));
                next:
                    continue;
                }
            }
            var randRange = cans.Count();
            if (room.Exits.Count != 0)
            {
                Debug.Log("last " + room.Exits.Last().ToString());
                cans.Sort((Vector3Int a, Vector3Int b) =>
                {
                    return (int)(100 * ((b - room.Exits.Last()).magnitude - (a - room.Exits.Last()).magnitude));
                });
                randRange = System.Math.Min(4, randRange);
                foreach (var can in cans)
                {
                    Debug.Log(can);
                }
            }
            Vector3Int pos = cans[_random.Next(0, randRange)];
            room.Exits.Add(pos);
            pos = DirMove(adjDir, pos, -1);

            // write blocks
            for (var i = 0; i < ExitStretch * 2 + 1; i++)
            {
                Vector3Int adj = DirMove(dir, pos, 1);
                room.Blocks[adj.x, adj.z] |= Block.Key;
                adj = DirMove(dir, pos, -1);
                room.Blocks[adj.x, adj.z] |= Block.Key;
                if (invRange != 0 && i == ExitStretch)
                {
                    room.Blocks[pos.x, pos.z] |= Block.Key | Block.Exit;
                }
                else
                {
                    room.Blocks[pos.x, pos.z] = Block.Key;
                }
                pos = DirMove(adjDir, pos);
            }
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
                    WriteWallWeak(innerX, innerZ, Block.Outer | Block.Back | Block.Right);
                    for (int i = 1; i < innerW - 1; ++i)
                    {
                        WriteWallWeak(innerX + i, innerZ, Block.Back);
                    }
                    WriteWallWeak(innerX + innerW - 1, innerZ, Block.Outer | Block.Back | Block.Left);

                    for (int i = 1; i < innerD - 1; ++i)
                    {
                        WriteWallWeak(innerX, innerZ + i, Block.Right);
                        for (int j = 1; j < innerW - 1; ++j)
                        {
                            _layout[innerX + j, innerZ + i] = Block.None;
                        }
                        WriteWallWeak(innerX + innerW - 1, innerZ + i, Block.Left);
                    }

                    WriteWallWeak(innerX, innerZ + innerD - 1, Block.Outer | Block.Front | Block.Right);
                    for (int i = 1; i < innerW - 1; ++i)
                    {
                        WriteWallWeak(innerX + i, innerZ + innerD - 1, Block.Front);
                    }
                    WriteWallWeak(innerX + innerW - 1, innerZ + innerD - 1, Block.Outer | Block.Front | Block.Left);
                }
            }
        }


        static Vector3Int DirMove(Dir dir, Vector3Int pos, int amt = 1)
        {
            switch (dir)
            {
                case Dir.Up:
                    pos.z += amt;
                    break;
                case Dir.Right:
                    pos.x += amt;
                    break;
                case Dir.Down:
                    pos.z -= amt;
                    break;
                case Dir.Left:
                    pos.x -= amt;
                    break;
            }
            return pos;
        }

        void AdvanceDir()
        {
            if (_currentDir == _totalDir)
            {
                switch (_random.Next(0, 3))
                {
                    case 0:
                        _currentDir = (_currentDir + 1) & Dir.Left;
                        break;
                    case 1:
                        _currentDir = (_currentDir - 1) & Dir.Left;
                        break;
                }
            }
            else
            {
                switch (_random.Next(0, 2))
                {
                    case 0:
                        _currentDir = _totalDir;
                        break;
                }
            }
        }

    }


    public class MockGenerator : MonoBehaviour
    {
        public void Start()
        {
            var config = new Config();
            config.Seed = new System.Random().Next();
            config.instantiate = Instantiate;
            config.GO = gameObject;
            // var go = Instantiate(
            new Generator().Generate(config);
            // , Vector3.zero, Quaternion.identity);
        }
    }
}