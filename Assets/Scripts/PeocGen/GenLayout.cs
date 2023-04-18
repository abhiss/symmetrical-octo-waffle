using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenLayout : MonoBehaviour
{
    public int RoomAmt = 3;
    public int RoomSizeRange = 16;
    public int RoomSizeMin = 16;
    public float FloorProb = 0.1f;
    public float WallProb = 0.1f;
    public float InteriorProb = 0.3f;
    public float NoiseFac = 0.5f;
    public int Seed = 0;

    const int MAP_SIZE = 256;
    char[,] _layout = new char[MAP_SIZE, MAP_SIZE];
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

    void Awake()
    {
        if (Seed == 0) {
            _random = new System.Random();
            Seed = _random.Next();
        }
        _random = new System.Random(Seed);
        _totalDir = _currentDir = (int)(_random.NextDouble() * 4);
        switch (_totalDir)
        {
            case 0:
                _cursorX = MAP_SIZE / 2;
                _cursorZ = MAP_SIZE * 3 / 4;
                break;
            case 1:
                _cursorX = MAP_SIZE * 3 / 4;
                _cursorZ = MAP_SIZE / 2;
                break;
            case 2:
                _cursorX = MAP_SIZE / 2;
                _cursorZ = MAP_SIZE / 4;
                break;
            case 3:
                _cursorX = MAP_SIZE / 4;
                _cursorZ = MAP_SIZE / 2;
                break;
        };
        _dir_cans = Enumerable.Range(0, 4).Where(n => n != ((_totalDir + 2) & 3)).ToArray();
        MoveRoom(5, 5);
        WriteRoom(5, 5);
        AdvanceDir();
        MoveRoomWall(5, 5);
        WriteHall((int)(_random.NextDouble() * 5) + 3);
        for (int i = 0; i < RoomAmt; ++i)
        {
            var roomWidth = (int)(_random.NextDouble() * RoomSizeRange )+ RoomSizeMin;
            var roomHeight = (int)(_random.NextDouble() * RoomSizeRange )+ RoomSizeMin;
            MoveRoom(roomWidth, roomHeight);
            WriteRoom(roomWidth, roomHeight);
            AdvanceDir();
            MoveRoomWall(roomWidth, roomHeight);
            WriteHall((int)(_random.NextDouble() * 2 )+ 1);
        }
        MoveRoom(5, 5);
        WriteRoom(5, 5);

        for (int i = 0; i < MAP_SIZE; ++i)
        {
            for (int j = 0; j < MAP_SIZE; ++j)
            {
                // AddInteriorWall(i, j);
            }
        }

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
                var wallPrefab = _wallPrefabs[ _random.NextDouble()< WallProb ?  (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0 ];
                var status = _layout[i, j] switch
                {
                    'g' => GenerateGround(i, j),
                    'b' => GenerateBackWall(
                        wallPrefab,
                        i, j),
                    'f' => GenerateFrontWall(
                        wallPrefab,
                        i, j),
                    'l' => GenerateLeftWall(
                        wallPrefab,
                        i, j),
                    'r' => GenerateRightWall(
                        wallPrefab,
                        i, j),
                    'd' => GenerateVertExit(i, j),
                    'e' => GenerateHorExit(i, j),
                    '{' => GenerateBackLeft(i, j),
                    '}' => GenerateBackRight(i, j),
                    '[' => GenerateFrontLeft(i, j),
                    ']' => GenerateFrontRight(i, j),
                    _ => 0,
                };
            }
        }
    }

    void AddInteriorWall(int x, int z)
    {
        if (_layout[x, z] == '\0')
        {
            return;
        }

        if (Mathf.PerlinNoise(((float)x) * NoiseFac, ((float)z) * NoiseFac) > InteriorProb)
        {
            return;
        }
        _layout[x, z] = '\0';
        if (_layout[x + 1, z] != '\0')
        {
            _layout[x + 1, z] = 'r';
        }
        if (_layout[x - 1, z] != '\0')
        {
            _layout[x - 1, z] = 'l';

        }
        if (_layout[x, z + 1] != '\0')
        {
            _layout[x, z + 1] = 'b';
        }
        if (_layout[x, z - 1] != '\0')
        {
            _layout[x, z - 1] = 'f';
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

    int GenerateBackRight(int x, int z) {
        var column = Instantiate(_columnPrefab, new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        column.transform.Translate(new Vector3(-1, -0.04f, 3));
        column.transform.Rotate(new Vector3(-90, 0, 0));
        var top0 = Instantiate(_topsPrefabs[0], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        top0.transform.Translate(new Vector3(0, 0, 2));
        top0.transform.Rotate(new Vector3(-90, 0, 0));
        return 1;
    }
    int GenerateBackLeft(int x, int z) {
        var column = Instantiate(_columnPrefab, new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        column.transform.Translate(new Vector3(3,-0.04f, 3));
        column.transform.Rotate(new Vector3(-90, 0, 0));
        var top0 = Instantiate(_topsPrefabs[0], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        top0.transform.Translate(new Vector3(2, 0, 2));
        top0.transform.Rotate(new Vector3(-90, 0, 0));
        return 1;
    }
    int GenerateFrontRight(int x, int z) {
        var column = Instantiate(_columnPrefab, new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        column.transform.Translate(new Vector3(-1,-0.04f, -1));
        column.transform.Rotate(new Vector3(-90, 0, 0));
        var top0 = Instantiate(_topsPrefabs[0], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        top0.transform.Translate(new Vector3(0, 0, 0));
        top0.transform.Rotate(new Vector3(-90, 0, 0));
        return 1;
    }
    int GenerateFrontLeft(int x, int z) {
        var column = Instantiate(_columnPrefab, new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        column.transform.Translate(new Vector3(3,-0.04f, -1));
        column.transform.Rotate(new Vector3(-90, 0, 0));
        var cieling0 = Instantiate(_topsPrefabs[0], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        cieling0.transform.Translate(new Vector3(2, 0, 0));
        cieling0.transform.Rotate(new Vector3(-90, 0, 0));
        return 1;
    }

    int GenerateBackWall( GameObject prefab, int x, int z)
    {
        var wall = Instantiate(prefab, new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        wall.transform.Translate(new Vector3(1, 0, 3));
        wall.transform.Rotate(new Vector3(-90, 0, 0));
        var top0 = Instantiate(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        top0.transform.Translate(new Vector3(0, 0, 2));
        top0.transform.Rotate(new Vector3(-90, 0, 0));
        var top1 = Instantiate(_topsPrefabs[1], new Vector3(x * 4 + 2, 4, z * 4), Quaternion.identity);
        top1.transform.Translate(new Vector3(0, 0, 2));
        top1.transform.Rotate(new Vector3(-90, 0, 0));
        return 1;

    }
    int GenerateFrontWall( GameObject prefab, int x, int z)
    {

        // var wall = Instantiate(_wallPrefabs[0], new Vector3(i * 4, 0, j * 4), Quaternion.identity);
        // wall.transform.Rotate(new Vector3(-90, 90, 0));
        // break;
        return 1;
    }
    int GenerateLeftWall( GameObject prefab, int x, int z)
    {
        var wall = Instantiate( prefab , new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        wall.transform.Translate(new Vector3(-1, 0, 1));
        wall.transform.Rotate(new Vector3(-90, -90, 0));
        var top0 = Instantiate(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        top0.transform.Rotate(new Vector3(-90, 90, 0));
        var top1 = Instantiate(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4 + 2), Quaternion.identity);
        top1.transform.Rotate(new Vector3(-90, 90, 0));
        return 1;

    }
    int GenerateRightWall( GameObject prefab, int x, int z)
    {
        var wall = Instantiate(prefab, new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        wall.transform.Translate(new Vector3(3, 0, 1));
        wall.transform.Rotate(new Vector3(-90, 90, 0));
        var top0 = Instantiate(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        top0.transform.Translate(new Vector3(2, 0, 0));
        top0.transform.Rotate(new Vector3(-90, 90, 0));
        var top1 = Instantiate(_topsPrefabs[1], new Vector3(x * 4, 4, z * 4 + 2), Quaternion.identity);
        top1.transform.Translate(new Vector3(2, 0, 0));
        top1.transform.Rotate(new Vector3(-90, 90, 0));
        return 1;

    }


    int GenerateHorExit(int x, int z)
    {
        GenerateRightWall( _exitPrefab, x, z);
        GenerateLeftWall( _exitPrefab, x, z);
        return 1;
    }

    int GenerateVertExit(int x, int z)
    {
        GenerateBackWall( _exitPrefab, x, z);
        GenerateFrontWall( _exitPrefab, x, z);
        return 1;

    }
    int GenerateGround(int x, int z)
    {

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                Instantiate(_floorPrefabs[
                 _random.NextDouble()< FloorProb ? 
(int)(_random.NextDouble() * (_floorPrefabs.Count - 1)) + 1 : 0
                ], new Vector3(x * 4 + i * 2, 0, z * 4 + j * 2), Quaternion.AngleAxis(-90, Vector3.right));
            }
        }
        return 1;
    }


    void MoveRoom(int width, int height)
    {

        if ((_currentDir & 1) == 1)
        {
            _cursorZ -= (int)(_random.NextDouble() * (height - 2)) + 1;
            if (_currentDir == 1)
            {
                _cursorX -= width - 1;
            }
        }
        else
        {
            _cursorX -= (int)(_random.NextDouble() * (width - 2)) + 1;
            if (_currentDir == 0)
            {
                _cursorZ -= height - 1;
            }

        }
    }

    void WriteWeak(int x, int z, char val) {
            if (_layout[x, z] == '\0')
            {
                _layout[x, z] = val;
            }
    }

    void WriteRoom(int width, int height)
    {
        WriteWeak(_cursorX, _cursorZ , '{');
        for (int i = 1; i < width - 1; ++i)
        {
            WriteWeak(_cursorX + i, _cursorZ, 'b');
        }
        WriteWeak(_cursorX + width - 1, _cursorZ , '}');

        for (int i = 1; i < height - 1; ++i)
        {
            WriteWeak(_cursorX, _cursorZ + i, 'r');
            for (int j = 1; j < width - 1; ++j)
            {
                WriteWeak(_cursorX + j, _cursorZ + i, 'g');
            }
            WriteWeak(_cursorX + width - 1, _cursorZ + i, 'l');
        }

        WriteWeak(_cursorX, _cursorZ + height - 1, '[');
        for (int i = 1; i < width - 1; ++i)
        {
            WriteWeak(_cursorX + i, _cursorZ + height - 1, 'f');
        }
        WriteWeak(_cursorX + width - 1, _cursorZ + height - 1, ']');
    }

    void MoveRoomWall(int width, int height)
    {
        if ((_currentDir & 1) == 1)
        {
            _cursorZ += (int)(_random.NextDouble() * (height - 2)) + 1;
            if (_currentDir == 3)
            {
                _cursorX += width - 1;
            }
        }
        else
        {
            _cursorX += (int)(_random.NextDouble() * (width - 2)) + 1;
            if (_currentDir == 2)
            {
                _cursorZ += height - 1;
            }
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
        int yOff = isHor ? 1 : 0;
        _layout[_cursorX, _cursorZ] = isHor ? 'e' : 'd';
        for (int i = 0; i < len; ++i)
        {
            MoveDir(_currentDir);
            _layout[_cursorX, _cursorZ] = isHor ? 'h' : 'v';
            _layout[_cursorX + xOff, _cursorZ + yOff] = isHor ? 'f' : 'l';
            _layout[_cursorX - xOff, _cursorZ - yOff] = isHor ? 'b' : 'r';
        }
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorZ] = isHor ? 'h' : 'v';
        _layout[_cursorX + xOff, _cursorZ + yOff] = isHor ? 'f' : 'l';
        _layout[_cursorX - xOff, _cursorZ - yOff] = isHor ? 'b' : 'r';
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorZ] = isHor ? 'e' : 'd';
    }

    void AdvanceDir() {
        if(_currentDir == _totalDir) {
            _currentDir = _dir_cans[(int)(_random.NextDouble() * 3)];
        } else {
            var dir_cans = _dir_cans.Where(n => n != ((_currentDir + 2) & 3)).ToArray();
            _currentDir = dir_cans[(int)(_random.NextDouble() * 2)];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
