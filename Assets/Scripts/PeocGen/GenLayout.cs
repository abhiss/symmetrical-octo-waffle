using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenLayout : MonoBehaviour
{
    public int RoomAmt = 3;
    public int RoomSizeMax = 16;
    public int RoomSizeMin = 32;
    public float FloorProb = 0.1f;
    public float InteriorProb = 0.3f;
    public float NoiseFac = 32.0f;

    const int MAP_SIZE = 256;
    char[,] _layout = new char[MAP_SIZE, MAP_SIZE];
    int _totalDir;
    int[] _dirs;
    int _currentDir;
    int _cursorX;
    int _cursorZ;
    List<GameObject> _floorPrefabs = new List<GameObject>();
    List<GameObject> _wallPrefabs = new List<GameObject>();
    List<GameObject> _cielingPrefabs = new List<GameObject>();
    void Awake()
    {
        Debug.Log("here");
        _totalDir = _currentDir = Random.Range(0, 3);
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
        _dirs = Enumerable.Range(0, 4).Where(n => n != ((_totalDir + 2) & 3)).ToArray();
        MoveRoom(5, 5);
        WriteRoom(5, 5);
        _currentDir = _dirs[Random.Range(0, 2)];
        MoveRoomWall(5, 5);
        WriteHall(Random.Range(3, 5));
        for (int i = 0; i < RoomAmt; ++i)
        {
            var roomWidth = Random.Range(RoomSizeMin, RoomSizeMax);
            var roomHeight = Random.Range(RoomSizeMin, RoomSizeMax);
            MoveRoom(roomWidth, roomHeight);
            WriteRoom(roomWidth, roomHeight);
            _currentDir = _dirs[Random.Range(0, 2)];
            MoveRoomWall(roomWidth, roomHeight);
            WriteHall(Random.Range(1, 3));
        }
        MoveRoom(5, 5);
        WriteRoom(5, 5);
        _floorPrefabs = LoadPrefabs("Floors/FloorTile_Empty", "Assets/Resources/Floors");
        _wallPrefabs = LoadPrefabs("Walls/Wall_Empty", "Assets/Resources/Walls");
        _cielingPrefabs = System.IO.Directory.GetFiles("Assets/Resources/Cielings")
            .Where(path => !path.EndsWith(".meta"))
            .Select(path => (GameObject)Resources.Load(path.Substring(17, path.IndexOf('.') - 17)))
            .ToList();
        Resources.Load("Exit");
        for (int i = 0; i < MAP_SIZE; ++i)
        {
            for (int j = 0; j < MAP_SIZE; ++j)
            {
                AddInteriorWall(i, j);
            }
        }

        for (int i = 0; i < MAP_SIZE; ++i)
        {
            for (int j = 0; j < MAP_SIZE; ++j)
            {
                var status = _layout[i, j] switch
                {
                    'g' => GenerateGround(i, j),
                    'b' => GenerateBackWall(i, j),
                    'f' => GenerateFrontWall(i, j),
                    'l' => GenerateLeftWall(i, j),
                    'r' => GenerateRightWall(i, j),
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

        Debug.Log(Mathf.PerlinNoise(((float) x) / ((float)NoiseFac), ((float) z) / ((float)NoiseFac)));
        if (Mathf.PerlinNoise(((float) x) / ((float)NoiseFac), ((float) z) / ((float)NoiseFac)) > InteriorProb)
        {
            return;
        }
        Debug.Log("removed");
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

    int GenerateBackWall(int x, int z)
    {
        var wall = Instantiate(_wallPrefabs[0], new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        wall.transform.Translate(new Vector3(1, 0, 3));
        wall.transform.Rotate(new Vector3(-90, 0, 0));
        var cieling0 = Instantiate(_cielingPrefabs[2], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        cieling0.transform.Translate(new Vector3(0, 0, 2));
        cieling0.transform.Rotate(new Vector3(-90, 90, 0));
        var cieling1 = Instantiate(_cielingPrefabs[2], new Vector3(x * 4 + 2, 4, z * 4), Quaternion.identity);
        cieling1.transform.Translate(new Vector3(0, 0, 2));
        cieling1.transform.Rotate(new Vector3(-90, 90, 0));
        return 1;

    }
    int GenerateFrontWall(int x, int z)
    {

        // var wall = Instantiate(_wallPrefabs[0], new Vector3(i * 4, 0, j * 4), Quaternion.identity);
        // wall.transform.Rotate(new Vector3(-90, 90, 0));
        // break;
        return 1;
    }
    int GenerateLeftWall(int x, int z)
    {
        var wall = Instantiate(_wallPrefabs[0], new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        wall.transform.Translate(new Vector3(-1, 0, 1));
        wall.transform.Rotate(new Vector3(-90, -90, 0));
        var cieling0 = Instantiate(_cielingPrefabs[2], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        cieling0.transform.Rotate(new Vector3(-90, 0, 0));
        var cieling1 = Instantiate(_cielingPrefabs[2], new Vector3(x * 4, 4, z * 4 + 2), Quaternion.identity);
        cieling1.transform.Rotate(new Vector3(-90, 0, 0));
        return 1;

    }
    int GenerateRightWall(int x, int z)
    {
        var wall = Instantiate(_wallPrefabs[0], new Vector3(x * 4, 0, z * 4), Quaternion.identity);
        wall.transform.Translate(new Vector3(3, 0, 1));
        wall.transform.Rotate(new Vector3(-90, 90, 0));
        var cieling0 = Instantiate(_cielingPrefabs[2], new Vector3(x * 4, 4, z * 4), Quaternion.identity);
        cieling0.transform.Translate(new Vector3(2, 0, 0));
        cieling0.transform.Rotate(new Vector3(-90, 180, 0));
        var cieling1 = Instantiate(_cielingPrefabs[2], new Vector3(x * 4, 4, z * 4 + 2), Quaternion.identity);
        cieling1.transform.Translate(new Vector3(2, 0, 0));
        cieling1.transform.Rotate(new Vector3(-90, 180, 0));
        return 1;

    }


    int GenerateGround(int x, int z)
    {

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                Instantiate(_floorPrefabs[

                 // 0
                 Random.Range(0.0f, 1.0f) < FloorProb ? Random.Range(1, _floorPrefabs.Count) : 0
                ], new Vector3(x * 4 + i * 2, 0, z * 4 + j * 2), Quaternion.AngleAxis(-90, Vector3.right));
            }
        }
        return 1;
    }


    void MoveRoom(int width, int height)
    {

        if ((_currentDir & 1) == 1)
        {
            _cursorZ -= Random.Range(1, height - 1);
            if (_currentDir == 1)
            {
                _cursorX -= width - 1;
            }
        }
        else
        {
            _cursorX -= Random.Range(1, width - 1);
            if (_currentDir == 0)
            {
                _cursorZ -= height - 1;
            }

        }
    }

    void WriteRoom(int width, int height)
    {
        Debug.Log(_currentDir + " " + _cursorX + " " + _cursorZ);
        _layout[_cursorX, _cursorZ] = '{';
        for (int i = 1; i < width - 1; ++i)
        {
            _layout[_cursorX + i, _cursorZ] = 'b';
        }
        _layout[_cursorX + width - 1, _cursorZ] = '}';

        for (int i = 1; i < height - 1; ++i)
        {
            _layout[_cursorX, _cursorZ + i] = 'r';
            for (int j = 1; j < width - 1; ++j)
            {
                _layout[_cursorX + j, _cursorZ + i] = 'g';
            }
            _layout[_cursorX + width - 1, _cursorZ + i] = 'l';
        }

        _layout[_cursorX, _cursorZ + height - 1] = '[';
        for (int i = 1; i < width - 1; ++i)
        {
            _layout[_cursorX + i, _cursorZ + height - 1] = 'f';
        }
        _layout[_cursorX + width - 1, _cursorZ + height - 1] = ']';
    }

    void MoveRoomWall(int width, int height)
    {
        if ((_currentDir & 1) == 1)
        {
            _cursorZ += Random.Range(1, height - 1);
            if (_currentDir == 3)
            {
                _cursorX += width - 1;
            }
        }
        else
        {
            _cursorX += Random.Range(1, width - 1);
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
        bool isVert = (_currentDir & 1) == 1;
        int xOff = isVert ? 0 : 1;
        int yOff = isVert ? 1 : 0;
        _layout[_cursorX, _cursorZ] = (char)(_currentDir + '0');
        for (int i = 0; i < len; ++i)
        {
            MoveDir(_currentDir);
            _layout[_cursorX, _cursorZ] = 'g';
            _layout[_cursorX + xOff, _cursorZ + yOff] = isVert ? 'f' : 'l';
            _layout[_cursorX - xOff, _cursorZ - yOff] = isVert ? 'b' : 'r';
        }
        MoveDir(_currentDir);
        MoveDir(_currentDir);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
