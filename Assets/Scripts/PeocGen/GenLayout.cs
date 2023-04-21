using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GenLayout : MonoBehaviour
{
    private enum Block
    {
        None = 0,
        Ground = 1,
        Key = 2,
        Right = 4,
        Left = 8,
        Front = 16,
        Back = 32,
        Inner = 64,
        Outer = 128,
        Pit = 256,
    }

    public int RoomAmt = 3;
    public int RoomSizeRange = 16;
    public int RoomSizeMin = 16;
    public float FloorProb = 0.1f;
    public float WallProb = 0.1f;
    public float InteriorProb = 0.3f;
    public float NoiseFac = 0.5f;
    public int HalfInnerMin = 1;
    public int HalfInnerRange = 2;
    public int InnerSizeMin = 2;
    public int InnerSizeRange = 1;
    public int Seed = 0;
    public GameObject Player;

    const int MAP_SIZE = 128;
    Block[,] _layout = new Block[MAP_SIZE, MAP_SIZE];
    int _totalDir;
    int[] _dir_cans;
    int _currentDir;
    int _cursorX;
    int _cursorZ;
    List<GameObject> _floorPrefabs = new List<GameObject>();
    List<GameObject> _wallPrefabs = new List<GameObject>();
    List<GameObject> _topsPrefabs = new List<GameObject>();
    GameObject _exitPrefab;
    GameObject _columnPrefab;
    System.Random _random;
    Vector3[] _bounds;


    void Awake()
    {
        if (Seed == 0)
        {
            _random = new System.Random();
            Seed = _random.Next();
        }
        _random = new System.Random(Seed);
        _totalDir = _currentDir = (int)(_random.NextDouble() * 4);

        _cursorX = MAP_SIZE / 2;
        _cursorZ = MAP_SIZE / 2;
        Player.transform.position = new Vector3(_cursorX * 4 + 8, 5, _cursorZ * 4 + 8);

        _dir_cans = Enumerable.Range(0, 4).Where(n => n != ((_totalDir + 2) & 3)).ToArray();
        WriteRoom(5, 5);
        AdvanceDir();
        MoveRoom(5, 5);
        WriteHall((int)(_random.NextDouble() * 5) + 3);
        for (int i = 0; i < RoomAmt; ++i)
        {
            var roomWidth = (int)(_random.NextDouble() * RoomSizeRange) + RoomSizeMin;
            var roomdepth = (int)(_random.NextDouble() * RoomSizeRange) + RoomSizeMin;
            MoveRoom(roomWidth, roomdepth, true);
            WriteRoom(roomWidth, roomdepth);
            WriteInterior(roomWidth, roomdepth);
            AdvanceDir();
            MoveRoom(roomWidth, roomdepth);
            WriteHall((int)(_random.NextDouble() * 2) + 1);
        }
        MoveRoom(5, 5, true);
        WriteRoom(5, 5);


        _floorPrefabs = LoadPrefabs("Floors/FloorTile_Empty", "Assets/Resources/Floors");
        _wallPrefabs = LoadPrefabs("Walls/Wall_Empty", "Assets/Resources/Walls");
        _topsPrefabs = System.IO.Directory.GetFiles("Assets/Resources/Tops")
            .Where(path => !path.EndsWith(".meta"))
            .Select(path => (GameObject)Resources.Load(path.Substring(17, path.IndexOf('.') - 17)))
            .ToList();
        _exitPrefab = (GameObject)Resources.Load("Exit");
        _columnPrefab = (GameObject)Resources.Load("Column_1");

        for (int i = 0; i < MAP_SIZE; ++i)
        {
            for (int j = 0; j < MAP_SIZE; ++j)
            {
                var inner = (Block.Inner & _layout[i, j]) != 0 ;
                if (!inner && (Block.Ground & _layout[i, j]) != 0) {
                    GenerateGround(i, j);
                }
                if (!inner && (Block.Front & _layout[i, j]) != 0) {
                    var wall0 = _wallPrefabs[_random.NextDouble() < WallProb ? (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0];
                    GenerateFrontWall(wall0, i, j);
                }
                if(!inner && (Block.Back & _layout[i, j]) != 0 ) {
                    var wall0 = _wallPrefabs[_random.NextDouble() < WallProb ? (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0];
                    GenerateBackWall(wall0, i, j);
                }
                if(!inner &&(Block.Right  & _layout[i, j]) != 0 ) {
                    var wall0 = _wallPrefabs[_random.NextDouble() < WallProb ? (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0];
                    GenerateRightWall(wall0, i, j);
                }
                if(!inner && (Block.Left  & _layout[i, j]) != 0 ) {
                    var wall0 = _wallPrefabs[_random.NextDouble() < WallProb ? (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0];
                    GenerateLeftWall(wall0, i, j);
                }
                if(inner || (Block.Outer  & _layout[i, j]) != 0  ) {
                    var walls = Block.Right | Block.Left | Block.Back | Block.Front;
                    switch ((_layout[i, j] & walls) )//^ (inner ? walls: Block.None))
                    {
                        case  Block.Front | Block.Right:
                            GenerateFrontRightColumn(i, j);
                            break;
                        case Block.Front | Block.Left:
                            GenerateFrontLeftColumn(i, j);
                            break;
                        case Block.Back | Block.Right:
                            GenerateBackRightColumn(i, j);
                            break;
                        case Block.Back | Block.Left:
                            GenerateBackLeftColumn(i, j);
                            break;

                    };
                }
            }
        }
    }

    void AddInteriorWall(int x, int z)
    {
        if (_layout[x, z] == Block.None)
        {
            return;
        }

        if (Mathf.PerlinNoise(((float)x) * NoiseFac, ((float)z) * NoiseFac) > InteriorProb)
        {
            return;
        }
        _layout[x, z] = Block.None;
        if (_layout[x + 1, z] != Block.None)
        {
            _layout[x + 1, z] = Block.Right;
        }
        if (_layout[x - 1, z] != Block.None)
        {
            _layout[x - 1, z] = Block.Left;

        }
        if (_layout[x, z + 1] != Block.None)
        {
            _layout[x, z + 1] = Block.Back;
        }
        if (_layout[x, z - 1] != Block.None)
        {
            _layout[x, z - 1] = Block.Front;
        }
    }

    List<GameObject> LoadPrefabs(string empty, string dir)
    {
        var prefabs = new List<GameObject>();
        prefabs.Add((GameObject)Resources.Load(empty));
        prefabs.AddRange(
        System.IO.Directory.GetFiles(dir)
            .Where(path => !path.EndsWith(".meta") && !path.EndsWith("Empty.prefab"))
            .Select(path => (GameObject)Resources.Load(path.Substring(17, path.IndexOf('.') - 17))));
        return prefabs;
    }

    int GenerateFrontRightColumn(int x, int z)
    {
        GeneratePrefab(_columnPrefab, new Vector3(x * 4 - 1, 0, z * 4 + 3), 0);
        GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), -90);
        return 1;
    }

    int GenerateFrontLeftColumn(int x, int z)
    {
        GeneratePrefab(_columnPrefab, new Vector3(x * 4 + 3, 0, z * 4 + 3), 0);
        GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), 0);
        return 1;
    }


    int GenerateBackRightColumn(int x, int z)
    {
        GeneratePrefab(_columnPrefab, new Vector3(x * 4 - 1, 0, z * 4 - 1), 0);
        GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), 180);
        return 1;
    }
    int GenerateBackLeftColumn(int x, int z)
    {
        GeneratePrefab(_columnPrefab, new Vector3(x * 4 + 3, 0, z * 4 - 1), 0);
        GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), 90);
        return 1;
    }

    int GenerateFrontWall(GameObject prefab, int x, int z)
    {
        GeneratePrefab(prefab, new Vector3(x * 4 + 1, 0, z * 4 + 3), 0);
        GeneratePrefab(_topsPrefabs[1], new Vector3(x * 4 + 1, 4, z * 4 + 2), 0);
        return 1;

    }
    int GenerateBackWall(GameObject prefab, int x, int z)
    {
        var wall = GeneratePrefab(prefab, new Vector3(x * 4 + 1, 0, z * 4 - 1), 180);
        var collider = wall.GetComponent<BoxCollider>();
        var size = collider.size;
        size.y *= 2;
        collider.size = size;
        GeneratePrefab(_topsPrefabs[1], new Vector3(x * 4 + 1, 4, z * 4), 180);

        return 1;
    }
    int GenerateRightWall(GameObject prefab, int x, int z)
    {
        GeneratePrefab(prefab, new Vector3(x * 4 - 1, 0, z * 4 + 1), -90);
        GeneratePrefab(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4 + 1), -90);
        return 1;

    }
    int GenerateLeftWall(GameObject prefab, int x, int z)
    {
        GeneratePrefab(prefab, new Vector3(x * 4 + 3, 0, z * 4 + 1), 90);
        GeneratePrefab(_topsPrefabs[1], new Vector3(x * 4 + 2, 4, z * 4 + 1), 90);
        // var top1 = Instantiate(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4 + 2), Quaternion.identity);
        // top1.transform.parent = gameObject.transform;
        // top1.transform.Translate(new Vector3(2, 0, 0));
        // top1.transform.Rotate(new Vector3(-90, 90, 0));
        return 1;
    }

    GameObject GeneratePrefab(GameObject prefab, Vector3 pos, float yRot)
    {
        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.parent = gameObject.transform;
        go.transform.Rotate(new Vector3(-90, yRot, 0));
        return go;
    }

    int GenerateGround(int x, int z)
    {

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                var ground = Instantiate(
                    _floorPrefabs[_random.NextDouble() < FloorProb ?
                    (int)(_random.NextDouble() * (_floorPrefabs.Count - 1)) + 1 : 0
                ], new Vector3(x * 4 + i * 2, 0, z * 4 + j * 2), Quaternion.AngleAxis(-90, Vector3.right));
                ground.transform.parent = gameObject.transform;
            }
        }
        return 1;
    }




    void WriteWallAdd(int x, int z, Block val)
    {
        if ((_layout[x, z] & Block.Key) != 0){
            return;
        }
        if ((((_layout[x, z] == Block.Right) || (_layout[x, z] == Block.Left)) && ((val == Block.Front) || (val == Block.Back)))
         ||(((_layout[x, z] == Block.Front) || (_layout[x, z] == Block.Back)) && ((val == Block.Right) || (val == Block.Left)))) {
            _layout[x, z] |= val | Block.Outer;
            return;
        }
        if ((_layout[x, z] & (Block.Right | Block.Left | Block.Front | Block.Back)) != 0)
        {
            _layout[x, z] &= val;
            return;
        }
        _layout[x, z] = val;

    
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

        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < depth; ++j) {
                if ((_layout[x + i , z + j] & Block.Key) != 0) {
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

    void WriteRoom(int width, int depth)
    {
        WriteWallAdd(_cursorX, _cursorZ, Block.Inner | Block.Front | Block.Left);
        for (int i = 1; i < width - 1; ++i)
        {
            WriteWallAdd(_cursorX + i, _cursorZ, Block.Front);
        }
        WriteWallAdd(_cursorX + width - 1, _cursorZ, Block.Inner | Block.Front | Block.Right);

        for (int i = 1; i < depth - 1; ++i)
        {
            WriteWallAdd(_cursorX, _cursorZ + i, Block.Left);
            for (int j = 1; j < width - 1; ++j)
            {
                WriteWeak(_cursorX + j, _cursorZ + i, Block.Ground);
            }
            WriteWallAdd(_cursorX + width - 1, _cursorZ + i, Block.Right);
        }

        WriteWallAdd(_cursorX, _cursorZ + depth - 1, Block.Inner | Block.Back | Block.Left);
        for (int i = 1; i < width - 1; ++i)
        {
            WriteWallAdd(_cursorX + i, _cursorZ + depth - 1, Block.Back);
        }
        WriteWallAdd(_cursorX + width - 1, _cursorZ + depth - 1, Block.Inner | Block.Back | Block.Right);
    }

    void WriteInterior(int width, int depth)
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
            var interiorAmt = (int)(_random.NextDouble() * HalfInnerRange ) + HalfInnerMin ;
            var spaceX = width - 3;
            var spaceZ = depth - 3;
            while ( interiorAmt > 0 && spaceX > 0 && spaceZ > 0)
            {
                var innerW = (int)(_random.NextDouble() * System.Math.Min(InnerSizeRange, spaceX) + InnerSizeMin);
                var innerD = (int)(_random.NextDouble() * System.Math.Min(InnerSizeRange, spaceZ) + InnerSizeMin);
                spaceX -= innerW;
                spaceZ -= innerD;
                var innerX = (int)(_random.NextDouble() * (width - innerW) + _cursorX);
                var innerZ = (int)(_random.NextDouble() * (depth - innerD) + _cursorZ);
                Debug.Log((innerX - _cursorX) + " " + (innerZ - _cursorZ) + " " + innerW + " " + innerD);
                if (!CheckRoom(innerX, innerZ, innerW, innerD))
                {
                    continue;
                }
                WriteWallAdd( innerX, innerZ, Block.Outer | Block.Back | Block.Right);
                for (int i = 1; i < innerW - 1; ++i) {
                    WriteWallAdd( innerX + i, innerZ, Block.Back);
                }
                WriteWallAdd( innerX + innerW - 1, innerZ, Block.Outer | Block.Back | Block.Left);

                for (int i = 1; i < innerD - 1; ++i) {
                    WriteWallAdd( innerX, innerZ + i, Block.Right);
                    for (int j = 1; j < innerW - 1; ++j) {
                        _layout[innerX + j, innerZ + i] = Block.None;
                    }
                    WriteWallAdd( innerX + innerW - 1, innerZ + i, Block.Left);
                }

                WriteWallAdd( innerX, innerZ + innerD - 1, Block.Outer | Block.Front | Block.Right);
                for (int i = 1; i < innerW - 1; ++i) {
                    WriteWallAdd( innerX + i, innerZ + innerD - 1, Block.Front);
                }
                WriteWallAdd( innerX + innerW - 1, innerZ + innerD - 1, Block.Outer | Block.Front | Block.Left);
            }
        }
    }


    void MoveRoom(int width, int depth, bool inv = false)
    {
        var xOff = 0;
        var zOff = 0;

        if ((_currentDir & 1) == 1)
        {
            if ((_totalDir & 1) == 0)
            {
                zOff = (int)(_random.NextDouble() * (depth - 2) / 2 +
                    ((_totalDir == 0) ^ inv ? 1 : depth / 2));
            }
            else
            {
                zOff = (int)(_random.NextDouble() * (depth - 2)) + 1;
            }
            if ((_currentDir == 3 && !inv) || (_currentDir == 1 && inv)) {
                xOff = width - 1;
            }
        }
        else
        {
            if ((_totalDir & 1) == 1)
            {
                xOff = (int)(_random.NextDouble() * (width - 2) / 2 +
                    ((_totalDir == 1) ^ inv ? 1 : width / 2));
            }
            else
            {
                xOff = (int)(_random.NextDouble() * (width - 2)) + 1;
            }
            if ((_currentDir == 2 && !inv) || (_currentDir == 0 && inv))
            {
                zOff = depth - 1;
            }
        }

        if (inv)
        {
            _cursorX -= xOff;
            _cursorZ -= zOff;
        }
        else
        {
            _cursorX += xOff;
            _cursorZ += zOff;

        }
    }

    void MoveDir(int dir)
    {
        switch (dir)
        {
            case 0:
                _cursorZ -= 1;
                break;
            case 1:
                _cursorX -= 1;
                break;
            case 2:
                _cursorZ += 1;
                break;
            case 3:
                _cursorX += 1;
                break;
        }
    }

    void WriteHall(int len)
    {
        bool isHor = (_currentDir & 1) == 1;
        int xOff = isHor ? 0 : 1;
        int zOff = isHor ? 1 : 0;
        Block b0 = isHor ? Block.Back : Block.Right;
        Block b1 = isHor ? Block.Front : Block.Left;
        MoveDir((_currentDir + 2) & 3);
        _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        WriteWallAdd(_cursorX + xOff, _cursorZ + zOff, b0);
        WriteWallAdd(_cursorX - xOff, _cursorZ - zOff, b1);
        for (int i = 0; i < len; ++i)
        {
            MoveDir(_currentDir);
            _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;//isHor ? 'h' : 'v';
            _layout[_cursorX + xOff, _cursorZ + zOff] = b0;
            _layout[_cursorX - xOff, _cursorZ - zOff] = b1;
        }
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground; //isHor ? 'h' : 'v';
        _layout[_cursorX + xOff, _cursorZ + zOff] = b0;
        _layout[_cursorX - xOff, _cursorZ - zOff] = b1;
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        _layout[_cursorX + xOff, _cursorZ + zOff] = b0;
        _layout[_cursorX - xOff, _cursorZ - zOff] = b1;
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorZ] = Block.Key | Block.Ground;
        MoveDir((_currentDir + 2) & 3);
    }

    void AdvanceDir()
    {
        if (_currentDir == _totalDir)
        {
            _currentDir = _dir_cans[(int)(_random.NextDouble() * 3)];
        }
        else
        {
            var dir_cans = _dir_cans.Where(n => n != ((_currentDir + 2) & 3)).ToArray();
            _currentDir = dir_cans[(int)(_random.NextDouble() * 2)];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
