using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

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
    Prop = 4096,
  }

  enum Prefab
  {
    Wall,
    LitWall,
    Ground,
    Inner,
    Outer,
    Exit,
    FloorLight,
    Drum,
    Crate,
    Turret,
    ExplosiveBarrel,
    Bot,
    Pit,
    Amt,
  }

  public class RoomProcData
  {
    public Block[,] Blocks;
    public Vector3Int EmptyPos = new Vector3Int();
    public Vector3Int EmptySize = new Vector3Int();
    public RoomProcData(Vector3Int size)
    {
      Blocks = new Block[size.x, size.z];
    }
  }

  public class Room
  {
    static Block s_bnf = Block.Front | Block.Back;
    static Block s_rnl = Block.Right | Block.Left;

    public Vector3Int Size;
    public Vector3Int Pos = new Vector3Int();
    public GameObject GO = new GameObject();
    public Vector3Int Entrance;
    public Vector3Int Exit;
    public RoomProcData Proc;

    public Room(Vector3Int size)
    {
      Size = size;
    }
    public Room InitProc()
    {
      Proc = new RoomProcData(Size);
      AddWall(1, 1, Size.x - 2, Size.z - 2);
      return this;
    }

    public int CountGround()
    {
      int sum = 0;
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if (Proc.Blocks[i, j] == Block.Ground)
          {
            ++sum;
          }
        }
      }
      return sum;
    }
    public Vector3Int TakeGround(int index)
    {
      int grounds = 0;
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if (Proc.Blocks[i, j] == Block.Ground && grounds++ == index)
          {
            Proc.Blocks[i, j] |= Block.Prop;
            return new Vector3Int(i, 0, j);
          }

        }
      }
      return new Vector3Int();
    }
    public Vector3 PosGroundWorld(int index, float scale)
    {
      return (Vector3)TakeGround(index) * scale + Pos;
    }

    public void ComputeEmpty()
    {
      Proc.EmptySize = new Vector3Int();
      for (int i = 0; i < Size.x; i++)
      {
        for (int j = 0; j < Size.z; j++)
        {
          if (Proc.Blocks[i, j] != Block.Ground)
          {
            continue;
          }
          Vector3Int size = GroundSize(i, j);
          if (size.x > 1 && size.z > 1 && size.magnitude > Proc.EmptySize.magnitude)
          {
            Proc.EmptyPos = new Vector3Int(i, 0, j);
            Proc.EmptySize = size;
          }
        }
      }

    }

    private Vector3Int GroundSize(int x, int z)
    {
      var res = new Vector3Int(1, 0, 1);
      bool expanded = true;
      while (expanded)
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
        Block b = Proc.Blocks[x + (onX ? w : i), z + (onX ? i : d)];
        if (b != Block.Ground || (b & Block.Key) == Block.Key)
        {
          return false;
        }
      }
      return true;
    }

    public void WallFilter(int i, int j, Block corner)
    {
      if ((Proc.Blocks[i, j] & s_bnf) == s_bnf)
      {
        Proc.Blocks[i, j] &= ~(s_bnf);
      }
      if ((Proc.Blocks[i, j] & s_rnl) == s_rnl)
      {
        Proc.Blocks[i, j] &= ~(s_rnl);
      }
      Proc.Blocks[i, j] = Proc.Blocks[i, j] switch
      {
        Block b when ((b & (Block.Back | Block.Left)) == (Block.Back | Block.Left)
            || (b & (Block.Back | Block.Right)) == (Block.Back | Block.Right)
            || (b & (Block.Front | Block.Left)) == (Block.Front | Block.Left)
            || (b & (Block.Front | Block.Right)) == (Block.Front | Block.Right)) => b | corner,
        _ => Proc.Blocks[i, j],
      };
    }

    void AddWall(int x, int z, int w, int d)
    {
      for (int i = x; i < x + w; i++)
      {
        for (int j = z; j < z + d; j++)
        {
          Proc.Blocks[i, j] = Block.None;
          if (j == z)
          {
            Proc.Blocks[i, j] |= Block.Back;
          }
          else if (j == z + d - 1)
          {
            Proc.Blocks[i, j] |= Block.Front;
          }
          if (i == x)
          {
            Proc.Blocks[i, j] |= Block.Right;
            WallFilter(i, j, Block.Inner);
          }
          else if (i == x + w - 1)
          {
            Proc.Blocks[i, j] |= Block.Left;
            WallFilter(i, j, Block.Inner);
          }
          if (Proc.Blocks[i, j] == Block.None)
          {
            Proc.Blocks[i, j] = Block.Ground;
          }
        }
      }
    }

    public bool TrySubRect(int x, int z, int w, int d)
    {
      for (int i = x; i < x + w; i++)
      {
        for (int j = z; j < z + d; j++)
        {
          if ((Proc.Blocks[i, j] & Block.Key) == Block.Key)
          {
            return false;
          }
        }
      }
      SubWall(x, z, w, d);
      return true;
    }

    public void SubWall(int x, int z, int w, int d)
    {
      for (int i = x; i < x + w; i++)
      {
        for (int j = z; j < z + d; j++)
        {
          if (Proc.Blocks[i, j] == Block.None)
          {
            continue;
          }
          Block modified = Block.None;
          if (j == z)
          {
            modified = modified == Block.None ? Proc.Blocks[i, j] : modified;
            Proc.Blocks[i, j] &= Block.IsWall - 1;
            Proc.Blocks[i, j] |= Block.Front;
          }
          else if (j == z + d - 1)
          {
            modified = modified == Block.None ? Proc.Blocks[i, j] : modified;
            Proc.Blocks[i, j] &= Block.IsWall - 1;
            Proc.Blocks[i, j] |= Block.Back;
          }
          if (i == x)
          {
            modified = modified == Block.None ? Proc.Blocks[i, j] : modified;
            Proc.Blocks[i, j] &= Block.IsWall - 1;
            Proc.Blocks[i, j] |= Block.Left;
          }
          else if (i == x + w - 1)
          {
            modified = modified == Block.None ? Proc.Blocks[i, j] : modified;
            Proc.Blocks[i, j] &= Block.IsWall - 1;
            Proc.Blocks[i, j] |= Block.Right;
          }
          if (modified == Block.None)
          {
            Proc.Blocks[i, j] = Block.None;
          }
          else
          {
            WallFilter(i, j, modified == Block.Ground ? Block.Outer : Block.Inner);
          }
        }
      }
    }

    public void SubPit(int x, int z, int w, int d)
    {
      for (int i = x; i < x + w; i++)
      {
        for (int j = z; j < z + d; j++)
        {
          Proc.Blocks[i, j] = Block.Pit;
        }
      }
    }

    Block[] GetAdjacent(int x, int z)
    {
      var adj = new Block[4];
      adj[0] = (x != 0) ? Proc.Blocks[x - 1, z] : Block.None;
      adj[1] = (x != Size.x - 1) ? Proc.Blocks[x + 1, z] : Block.None;
      adj[2] = (z != 0) ? Proc.Blocks[x, z - 1] : Block.None;
      adj[3] = (z != Size.z - 1) ? Proc.Blocks[x, z + 1] : Block.None;
      return adj;
    }

    bool IsNone(Block[] adj)
    {
      return (adj[0] == Block.None || adj[0] == Block.Left)
          && (adj[1] == Block.None || adj[1] == Block.Right)
          && (adj[2] == Block.None || adj[2] == Block.Front)
          && (adj[3] == Block.None || adj[3] == Block.Back);
    }

    public void MoveAfter(Room prev)
    {
      Pos -= Entrance;
      Pos += prev.Pos + prev.Exit;
    }

    public float CornerAngle(Block corner)
    {
      return corner switch
      {
        Block b when ((b & (Block.Back | Block.Left)) == (Block.Back | Block.Left)) => 180,
        Block b when ((b & (Block.Back | Block.Right)) == (Block.Back | Block.Right)) => 270,
        Block b when ((b & (Block.Front | Block.Left)) == (Block.Front | Block.Left)) => 90,
        Block b when ((b & (Block.Front | Block.Right)) == (Block.Front | Block.Right)) => 0,
      };
    }
  }

  public class Config
  {
    public int Seed = 0;
    public GameObject Spawn;
    public System.Func<GameObject, Vector3, Quaternion, GameObject> Instantiate;
    public GameObject GO; // parent needs nav mesh surface
    public List<PremadeRoom> PremadeRooms = new List<PremadeRoom>();
  }

  public class Generator
  {
    //public static DoorSize = 
    static void RotatePremade(GameObject premade, Dir dir)
    {
      premade.transform.Rotate(new Vector3(0, 90 * (float)dir, 0));
    }

    public static float BlockSize = 4.0f;


    public int RoomAmt = 100;
    public int RoomSizeMin = 8;
    public int RoomSizeMax = 16;
    public int HallLenMin = 1;
    public int HallLenMax = 5;
    public int HallWidth = 2;

    private List<Room> _rooms = new List<Room>();
    public int InnerLimit = 5;
    public int InnerSizeMin = 2;
    public int InnerSizeMax = 6;
    private int _exitStretch = 1; // must be odd
    private System.Random _random;
    private Dir _totalDir;
    private Dir _currentDir;
    private List<GameObject> WallOuts;
    private GameObject[] _prefabs;
    private Config _config;

    public void Generate(Config config)
    {
      _config = config;
      _random = new System.Random(_config.Seed);
      _totalDir = _currentDir = (Dir)(_random.Next(0, 4));
      _prefabs = Enumerable.Range(0, (int)Prefab.Amt)
                      .Select(n => (GameObject)Resources.Load("Blocks0/" + ((Prefab)n).ToString())).ToArray<GameObject>();

      var premadeSpawn = _config.PremadeRooms.Find(room => room.Index == 0);
      if (premadeSpawn)
      {
        _rooms.Add(premadeSpawn.GetRoom());
      }
      else
      {
        var spawn = new Room(new Vector3Int(7, 0, 7)).InitProc();
        GenerateDoor(spawn);
        _rooms.Add(spawn);
      }

      for (int i = 1; i < RoomAmt - 1; ++i)
      {
        var prev = _rooms.Last();
        var premade = _config.PremadeRooms.Find(room => room.Index == i);
        if (null != premade)
        {
          _rooms.Add(premade.GetRoom());
          continue;
        }
        var range = PrevRange();
        var xMin = RoomSizeMin + 2;
        var zMin = RoomSizeMin + 2;
        if (_currentDir != _totalDir)
        {
          Debug.Log(range);
          if ((_totalDir & Dir.Right) == Dir.Right)
          {
            xMin = prev.Size.x - range + 3;
          }
          else
          {
            zMin = prev.Size.z - range + 3;
          }
        }
        var w = _random.Next(xMin, RoomSizeMax + 2);
        var d = _random.Next(zMin, RoomSizeMax + 2);
        var room = new Room(new Vector3Int(w, 0, d)).InitProc();
        GenerateDoor(room, range);
        if (null != _config.PremadeRooms.Find(room => room.Index == i + 1))
        {
          _currentDir = _totalDir;
        }
        else
        {
          AdvanceDir();
        }
        GenerateDoor(room);
        if (0 == _random.Next(0, 2))
        {
          FillCorners(room);
        }
        // interior variant
        GenerateInterior(room);
        GenProps(room);
        room.MoveAfter(prev);
        _rooms.Add(room);
      }
      var premadeFinish = _config.PremadeRooms.Find(room => room.Index == RoomAmt + 1);
      if (premadeFinish)
      {
        _rooms.Add(premadeFinish.GetRoom());
      }
      else
      {
        var finish = new Room(new Vector3Int(7, 0, 7)).InitProc();
        GenerateDoor(finish, PrevRange());
        finish.MoveAfter(_rooms.Last());
        _rooms.Add(finish);
      }
      foreach (Room room in _rooms)
      {
        BuildRoom(room);
        room.GO.transform.parent = _config.GO.transform;
      }
      var surface = _config.GO.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
      surface.BuildNavMesh();
      // foreach (Room room in _rooms)
      // {
      //   int groundAmt = room.CountGround();
      //   if (groundAmt == 0)
      //   {
      //     continue;
      //   }
      //   var prop = _config.Instantiate(_prefabs[(int)Prefab.Bot], Vector3.zero, Quaternion.AngleAxis((float)_random.NextDouble() * 360, Vector3.up));
      //   UnityEngine.AI.NavMeshHit hit;
      //   UnityEngine.AI.NavMesh.SamplePosition((Vector3)room.TakeGround(_random.Next(0, groundAmt--)) * BlockSize + room.GO.transform.position, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas);
      //   prop.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(hit.position);
      // }

    }

    void GenProps(Room room)
    {
      int groundAmt = room.CountGround();
      int propAmt = 5;
      var iterations = groundAmt / propAmt * 0.1;
      for (int i = 0; i < iterations; ++i)
      {
        // EnemySpawns.Add(room.PosGroundWorld(_random.Next(0, groundAmt--), BlockSize));
        SpawnProp(room, _prefabs[(int)Prefab.FloorLight], _random.Next(0, groundAmt--));
        SpawnProp(room, _prefabs[(int)Prefab.Crate], _random.Next(0, groundAmt--));
        SpawnProp(room, _prefabs[(int)Prefab.ExplosiveBarrel], _random.Next(0, groundAmt--));
        SpawnProp(room, _prefabs[(int)Prefab.Turret], _random.Next(0, groundAmt--));
        SpawnProp(room, _prefabs[(int)Prefab.Drum], _random.Next(0, groundAmt--));
      }

    }
    void FillCorners(Room room)
    {
      var corners = Enumerable.Range(0, 4).OrderBy(_ => _random.Next()).ToArray();
      int i = -1;
      bool subtracted = false;
      int xOff = room.Size.x / 2 + _random.Next(-1, 2);
      int zOff = room.Size.z / 2 + _random.Next(-1, 2);
      while (!subtracted && 4 != ++i)
      {
        Debug.Log(i);
        // corner variant
        subtracted = corners[i] switch
        {
          0 => room.TrySubRect(0, 0, xOff, zOff),
          1 => room.TrySubRect(xOff, 0, room.Size.x - xOff, zOff),
          2 => room.TrySubRect(xOff, zOff, room.Size.x - xOff, room.Size.z - zOff),
          3 => room.TrySubRect(0, zOff, xOff, room.Size.z - zOff),
        };
      }

    }

    void GenerateInterior(Room room)
    {
      for (int i = 0; i < InnerLimit; ++i)
      {
        room.ComputeEmpty();
        if (2 > room.Proc.EmptySize.x || 2 > room.Proc.EmptySize.z)
        {
          return;
        }
        int w = _random.Next(InnerSizeMin, System.Math.Min(room.Proc.EmptySize.x + 1, InnerSizeMax));
        int d = _random.Next(InnerSizeMin, System.Math.Min(room.Proc.EmptySize.z + 1, InnerSizeMax));
        int x = _random.Next(room.Proc.EmptyPos.x, room.Proc.EmptyPos.x + room.Proc.EmptySize.x + 1 - w);
        int z = _random.Next(room.Proc.EmptyPos.z, room.Proc.EmptyPos.z + room.Proc.EmptySize.z + 1 - d);
        // if (_random.Next(0, 2) == 0) {
        room.SubWall(x, z, w, d);
        // }else {
        //   room.SubPit(x, z, w, d);
        // }
      }
    }

    void BuildRoom(Room room)
    {
      if (null != room.Proc)
      {
        for (int i = 0; i < room.Size.x; ++i)
        {
          for (int j = 0; j < room.Size.z; ++j)
          {
            var block = room.Proc.Blocks[i, j] switch
            {
              Block b when ((b & Block.Ground) == Block.Ground) =>
                  _config.Instantiate(_prefabs[(int)Prefab.Ground], new Vector3(i, 0, j) * BlockSize, Quaternion.identity),
              Block b when ((b & Block.Pit) == Block.Pit) =>
                  _config.Instantiate(_prefabs[(int)Prefab.Pit], new Vector3(i, 0, j) * BlockSize, Quaternion.identity),
              Block.Front or Block.Left or Block.Back or Block.Right =>
                  _config.Instantiate(_prefabs[_random.Next(0, 10) == 0 ? (int)Prefab.LitWall : (int)Prefab.Wall], new Vector3(i, 0, j) * BlockSize, Quaternion.Euler(0, (float)System.Math.Log((int)room.Proc.Blocks[i, j], 2) * 90, 0)),
              Block b when ((b & Block.Outer) == Block.Outer) =>
                  _config.Instantiate(_prefabs[(int)Prefab.Outer], new Vector3(i, 0, j) * BlockSize, Quaternion.AngleAxis(room.CornerAngle(b), Vector3.up)),
              Block b when ((b & Block.Inner) == Block.Inner) =>
                  _config.Instantiate(_prefabs[(int)Prefab.Inner], new Vector3(i, 0, j) * BlockSize, Quaternion.AngleAxis(room.CornerAngle(b), Vector3.up)),
              Block b when ((b & Block.Exit) == Block.Exit) =>
                  _config.Instantiate(_prefabs[(int)Prefab.Exit], new Vector3(i, 0, j) * BlockSize, Quaternion.Euler(0, (float)System.Math.Log((int)(b & (Block.Ground - 1)), 2) * 90, 0)),
              _ => null,

            };
            if (block)
            {
              block.transform.parent = room.GO.transform;
            }
          }
        }
      }
      room.GO.transform.position = (Vector3)room.Pos * BlockSize;
    }


    void SpawnProp(Room room, GameObject prefab, int groundIndex)
    {
      var prop = _config.Instantiate(prefab, (Vector3)room.TakeGround(groundIndex) * BlockSize,
        Quaternion.AngleAxis((float)_random.NextDouble() * 360, Vector3.up));
      prop.transform.parent = room.GO.transform;
    }

    int PrevRange()
    {
      if (0 == _rooms.Count())
      {
        return 0;
      }
      Room prev = _rooms.Last();
      return _totalDir switch
      {
        Dir.Up => prev.Exit.z,
        Dir.Right => prev.Exit.x,
        Dir.Down => prev.Size.z - prev.Exit.z - 1,
        Dir.Left => prev.Size.x - prev.Exit.x - 1,
      };
    }
    void GenerateDoor(Room room, int invRange = 0)
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
              z0 = System.Math.Max(z0, zn - invRange - 1);
              break;
            case Dir.Left:
              x0 = System.Math.Max(x0, xn - invRange - 1);
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
          for (var k = 0; k < _exitStretch + 1; k++)
          {
            if (target != room.Proc.Blocks[current.x, current.z])
            {
              goto next;
            }
            current = DirMove(adjDir, current);
          }
          current = new Vector3Int(i, 0, j);
          for (var k = 0; k < _exitStretch; k++)
          {
            current = DirMove(adjDir, current, -1);
            if (target != room.Proc.Blocks[current.x, current.z])
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
      if (0 != invRange)
      {
        cans.Sort((Vector3Int a, Vector3Int b) =>
        {
          return (int)(100 * ((b - room.Entrance).magnitude - (a - room.Entrance).magnitude));
        });
        // foreach(var can in cans) {
        //   Debug.Log(can);
        // }
        randRange = System.Math.Min(4, randRange);
      }
      var index = _random.Next(0, randRange);
      Vector3Int pos = cans[index];
      if (0 == invRange)
      {
        room.Exit = pos;
      }
      else
      {
        room.Entrance = pos;
      }
      pos = DirMove(adjDir, pos, -1);

      // write blocks
      for (var i = 0; i < _exitStretch * 2 + 1; i++)
      {
        if (invRange != 0 && i == _exitStretch)
        {
          room.Proc.Blocks[pos.x, pos.z] |= Block.Key | Block.Exit;
        }
        else
        {
          room.Proc.Blocks[pos.x, pos.z] = Block.Key;
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
    public List<PremadeRoom> PremadeRooms = new List<PremadeRoom>();
    public void Start()
    {
      var config = new Config();
      config.Seed = new System.Random().Next();
      config.Instantiate = Instantiate;
      config.GO = gameObject;
      config.PremadeRooms = new List<PremadeRoom>();
      // var go = Instantiate(
      new Generator().Generate(config);
      // , Vector3.zero, Quaternion.identity);
    }
  }
}