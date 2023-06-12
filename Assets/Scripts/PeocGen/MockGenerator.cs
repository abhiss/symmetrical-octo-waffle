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

  public enum Block : int
  {
    None = 0,
    Front = 1,
    Left = 2,
    Back = 4,
    Right = 8,
    IsWall = 16,
    Ground = 32,
    Exit = 64,
    Inner = 128,
    Outer = 256,
    Inner2 = 512,
    Pit = 1024,
    Key = 2048,
    Light = 4096,
  }

  enum Prefab
  {
    Wall,
    LitWall,
    Ground,
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
    public Vector3Int EmptyPos = new Vector3Int();
    public Vector3Int EmptySize = new Vector3Int();

    public Room(Vector3Int size)
    {
      Size = size;
      Blocks = new Block[size.x, size.z];
    }
    static Block s_bnf = Block.Front | Block.Back;
    static Block s_rnl = Block.Right | Block.Left;

    public int CountGround() {
      int sum = 0;
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if (Blocks[i, j] == Block.Ground) {
            ++sum;
          }
        }
      }
      return sum;
    }
    public Vector3 PosGroundWorld(int index, float scale) {
      int i = 0;
      var limit = 100;
      Debug.Log(index);
      while(i != index && -- limit != 0) {
        if(Blocks[i / Size.x, i % Size.z] == Block.Ground) {
          ++i;
        }
      }
      return new Vector3(i / Size.x, 0, i % Size.z) * scale + Pos;
    }

    public void ComputeEmpty()
    {
      EmptySize = new Vector3Int();
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if (Blocks[i, j] != Block.Ground)
          {
            continue;
          }
          Vector3Int size = GroundSize(i, j);
          if (size.x > 1 && size.z > 1 && size.magnitude > EmptySize.magnitude)
          {
            EmptyPos = new Vector3Int(i, 0, j);
            EmptySize = size;
          }
        }
      }

    }

    private Vector3Int GroundSize(int x, int z)
    {
      var res = new Vector3Int(1, 0, 1);
      bool expanded = true;
      while (expanded )
      {
        expanded = false;
        if (TestGroundExpansion(x, z, res.x, res.z, true))
        {
          res.x += 1;
          expanded = true;
        }
        if (TestGroundExpansion(x, z, res.x, res.z, false))
        {
          res.z += 1;
          expanded = true;
        }
      }
      return res;
    }

    private bool TestGroundExpansion(int x, int z, int w, int d, bool onX)
    {
      for (int i = 0; i < (onX ? d : w); ++i)
      {
        Block b = Blocks[x + (onX ? w : i), z + (onX ? i : d)] ;
        if (b != Block.Ground || (b & Block.Key) == Block.Key)
        {
          return false;
        }
      }
      return true;
    }

    public void WallFilter(int i, int j, Block corner)
    {
      if ((Blocks[i, j] & s_bnf) == s_bnf)
      {
        Blocks[i, j] &= ~(s_bnf);
      }
      if ((Blocks[i, j] & s_rnl) == s_rnl)
      {
        Blocks[i, j] &= ~(s_rnl);
      }
      Blocks[i, j] = Blocks[i, j] switch
      {
        Block b when ((b & (Block.Back | Block.Left)) == (Block.Back | Block.Left)
            || (b & (Block.Back | Block.Right)) == (Block.Back | Block.Right)
            || (b & (Block.Front | Block.Left)) == (Block.Front | Block.Left)
            || (b & (Block.Front | Block.Right)) == (Block.Front | Block.Right)) => b | corner,
        _ => Blocks[i, j],
      };
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
          else if (j == z + d - 1)
          {
            Blocks[i, j] |= Block.Front;
          }
          if (i == x)
          {
            Blocks[i, j] |= Block.Right;
            WallFilter(i, j, Block.Inner);
          }
          else if (i == x + w - 1)
          {
            Blocks[i, j] |= Block.Left;
            WallFilter(i, j, Block.Inner);
          }
          if (Blocks[i, j] == Block.None)
          {
            Blocks[i, j] = Block.Ground;
          }
        }
      }
    }

    public void SubRect(int x, int z, int w, int d)
    {
      for (int i = x; i < x + w; i++)
      {
        for (int j = z; j < z + d; j++)
        {
          if (Blocks[i, j] == Block.None)
          {
            continue;
          }
          Block modified = Block.None;
          if (j == z)
          {
            modified = Blocks[i, j];
            Blocks[i, j] &= Block.IsWall - 1;
            Blocks[i, j] |= Block.Front;
          }
          else if (j == z + d - 1)
          {
            modified = Blocks[i, j];
            Blocks[i, j] &= Block.IsWall - 1;
            Blocks[i, j] |= Block.Back;
          }
          if (i == x)
          {
            modified = Blocks[i, j];
            Blocks[i, j] &= Block.IsWall - 1;
            Blocks[i, j] |= Block.Left;
          }
          else if (i == x + w - 1)
          {
            modified = Blocks[i, j];
            Blocks[i, j] &= Block.IsWall - 1;
            Blocks[i, j] |= Block.Right;
          }
          if (modified == Block.None)
          {
            Blocks[i, j] = Block.None;
          }
          else
          {
            WallFilter(i, j, Block.Outer);
          }
        }
      }
    }

    Block[] GetAdjacent(int x, int z)
    {
      var adj = new Block[4];
      adj[0] = (x != 0) ? Blocks[x - 1, z] : Block.None;
      adj[1] = (x != Size.x - 1) ? Blocks[x + 1, z] : Block.None;
      adj[2] = (z != 0) ? Blocks[x, z - 1] : Block.None;
      adj[3] = (z != Size.z - 1) ? Blocks[x, z + 1] : Block.None;
      return adj;
    }

    bool IsNone(Block[] adj)
    {
      return (adj[0] == Block.None || adj[0] == Block.Left)
          && (adj[1] == Block.None || adj[1] == Block.Right)
          && (adj[2] == Block.None || adj[2] == Block.Front)
          && (adj[3] == Block.None || adj[3] == Block.Back);
    }

    Block CollapseWall(Block[] adj)
    {
      return Block.None;
    }
    public void Lights(System.Random random)
    {
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if ((Blocks[i, j] & Block.IsWall - 1) != Block.None && random.Next(0, 10) == 0)
          {
            Blocks[i, j] |= Block.Light;
          }
        }
      }
    }

    public void FillCorners()
    {
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if (Blocks[i, j] != Block.IsWall)
          {
            continue;
          }
          Blocks[i, j] = CollapseWall(GetAdjacent(i, j));
        }
      }
    }
    public void MoveAfter(Room prev)
    {
      Pos -= Exits[0];
      Pos += prev.Pos + prev.Exits[1];
    }
    float CornerAngle(Block corner)
    {
      return corner switch
      {
        Block b when ((b & (Block.Back | Block.Left)) == (Block.Back | Block.Left)) => 180,
        Block b when ((b & (Block.Back | Block.Right)) == (Block.Back | Block.Right)) => 270,
        Block b when ((b & (Block.Front | Block.Left)) == (Block.Front | Block.Left)) => 90,
        Block b when ((b & (Block.Front | Block.Right)) == (Block.Front | Block.Right)) => 0,
      };
    }
    public void BuildGO(GameObject[] blocks, float scale, System.Random random, System.Func<GameObject, Vector3, Quaternion, GameObject> instantiate)
    {
      for (int i = 0; i < Size.x; ++i)
      {
        for (int j = 0; j < Size.z; ++j)
        {
          bool isLit = random.Next(0, 10) == 0;
          var block = Blocks[i, j] switch
          {
            Block b when ((b & Block.Ground) == Block.Ground) =>
                instantiate(blocks[(int)Prefab.Ground], new Vector3(i, 0, j) * scale, Quaternion.identity),

            Block.Front or Block.Left or Block.Back or Block.Right =>
                instantiate(blocks[isLit ? (int)Prefab.LitWall : (int)Prefab.Wall], new Vector3(i, 0, j) * scale, Quaternion.Euler(0, (float)System.Math.Log((int)Blocks[i, j], 2) * 90, 0)),
            Block b when ((b & Block.Outer) == Block.Outer) =>
                instantiate(blocks[(int)Prefab.Outer], new Vector3(i, 0, j) * scale, Quaternion.AngleAxis(CornerAngle(b), Vector3.up)),
            Block b when ((b & Block.Inner) == Block.Inner) =>
                instantiate(blocks[(int)Prefab.Inner], new Vector3(i, 0, j) * scale, Quaternion.AngleAxis(CornerAngle(b), Vector3.up)),
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
    public int InnerLimit = 3;
    public int InnerSizeMin = 2;
    public int InnerSizeMax = 6;
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
    public List<Vector3> EnemySpawns = new List<Vector3>();

    public void Generate(Config config)
    {
      _random = new System.Random(config.Seed);
      _totalDir = _currentDir = (Dir)(_random.Next(0, 4));

      _cursorX = MapSize / 2;
      _cursorZ = MapSize / 2;

      var spawn = new Room(new Vector3Int(_random.Next(RoomSizeMin + 2, RoomSizeMax + 2), 0, _random.Next(RoomSizeMin + 2, RoomSizeMax + 2)));
      spawn.AddRect(1, 1, spawn.Size.x - 2, spawn.Size.z - 2);
      spawn.Exits.Add(new Vector3Int());
      GenerateExit(spawn);
      _rooms.Add(spawn);

      for (int i = 0; i < RoomAmt; ++i)
      {
        var prev = _rooms.Last();
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
        // var variant = _random.Next(0, 8);
        // if (variant < 4)
        // {
        //     // corner variant
        //     int xOff = room.Size.x / 2 + _random.Next(-1, 2);
        //     int zOff = room.Size.z / 2 + _random.Next(-1, 2);
        //     switch (variant)
        //     {
        //         case 0:
        //             room.SubRect(0, 0, xOff, zOff);
        //             break;
        //         case 1:
        //             room.SubRect(xOff, 0, room.Size.x - xOff, zOff);
        //             break;
        //         case 2:
        //             room.SubRect(xOff, zOff, room.Size.x - xOff, room.Size.z - zOff);
        //             break;
        //         case 3:
        //             room.SubRect(0, zOff, xOff, room.Size.z - zOff);
        //             break;
        //     }
        // }
        // else
          // interior variant
          for (int j = 0; j < 5; ++j)
          {
            room.ComputeEmpty();
            if (room.EmptySize.x < 2 || room.EmptySize.z < 2) {
              break;
            }
            int w = _random.Next(InnerSizeMin, System.Math.Min(room.EmptySize.x + 1, InnerSizeMax));
            int d = _random.Next(InnerSizeMin, System.Math.Min(room.EmptySize.z + 1, InnerSizeMax));
            int x = _random.Next(room.EmptyPos.x, room.EmptyPos.x + room.EmptySize.x + 1 - w);
            int z = _random.Next(room.EmptyPos.z, room.EmptyPos.z + room.EmptySize.z + 1 - d);
            room.SubRect( x, z, w, d);
        }
        GenerateExit(room, range);
        AdvanceDir();
        GenerateExit(room);
        room.MoveAfter(_rooms.Last());
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
        room.BuildGO(blocks0, BlockSize, _random, config.instantiate);
        room.GO.transform.parent = config.GO.transform;
        int groundAmt = room.CountGround();
        for (int i = 0; i < groundAmt; ++i) {
          EnemySpawns.Add(room.PosGroundWorld(_random.Next(0, groundAmt--), BlockSize));
        }
      }
      var surface = config.GO.GetComponent<NavMeshSurface>();
      // surface.BuildNavMesh();

    }

    int PrevRange()
    {
      if (_rooms.Count() == 0)
      {
        return 0;
      }
      Room prev = _rooms.Last();
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
      Block target = Block.None;
      int x0 = 0;
      int xn = room.Size.x;
      int z0 = 0;
      int zn = room.Size.z;
      // i would not do it like this if i had more time
      switch (dir)
      {
        case Dir.Up:
          target = Block.Front;
          z0 = room.Size.z - 2;
          zn = z0 + 1;
          break;
        case Dir.Right:
          target = Block.Left;
          x0 = room.Size.x - 2;
          xn = x0 + 1;
          break;
        case Dir.Down:
          target = Block.Back;
          z0 = 1;
          zn = 2;
          break;
        case Dir.Left:
          target = Block.Right;
          x0 = 1;
          xn = 2;
          break;
      }
      var cans = new List<Vector3Int>();
      Dir adjDir = (dir + 1) & Dir.Left;
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
        cans.Sort((Vector3Int a, Vector3Int b) =>
        {
          return (int)(100 * ((b - room.Exits.Last()).magnitude - (a - room.Exits.Last()).magnitude));
        });
        randRange = System.Math.Min(4, randRange);
      }
      Vector3Int pos = cans[_random.Next(0, randRange)];
      room.Exits.Add(pos);
      pos = DirMove(adjDir, pos, -1);

      // write blocks
      for (var i = 0; i < ExitStretch * 2 + 1; i++)
      {
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