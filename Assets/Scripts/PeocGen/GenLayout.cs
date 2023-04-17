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

    const int MAP_SIZE = 512;
    char[,] _layout = new char[MAP_SIZE, MAP_SIZE];
    int _totalDir;
    int[] _dirs;
    int _currentDir;
    int _cursorX;
    int _cursorY;
    List<GameObject> _floorPrefabs = new List<GameObject>();
    List<GameObject> _wallPrefabs = new List<GameObject>();
    void Awake()
    {
        Debug.Log("here");
        _totalDir = _currentDir = Random.Range(0, 3);
        switch (_totalDir)
        {
            case 0:
                _cursorX = MAP_SIZE / 2;
                _cursorY = MAP_SIZE * 3 / 4;
                break;
            case 1:
                _cursorX = MAP_SIZE * 3 / 4;
                _cursorY = MAP_SIZE / 2;
                break;
            case 2:
                _cursorX = MAP_SIZE / 2;
                _cursorY = MAP_SIZE / 4;
                break;
            case 3:
                _cursorX = MAP_SIZE / 4;
                _cursorY = MAP_SIZE / 2;
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
            WriteHall(Random.Range(3, 5));
        }
        MoveRoom(5, 5);
        WriteRoom(5, 5);
        _floorPrefabs = LoadPrefabs("Floors/FloorTile_Empty", "Assets/Resources/Floors");
        _wallPrefabs = LoadPrefabs("Walls/Wall_Empty", "Assets/Resources/Walls");

        for (int i = 0; i < MAP_SIZE; ++i)
        {
            for (int j = 0; j < MAP_SIZE; ++j)
            {
                switch (_layout[i, j])
                {
                    case 'g':
                        GenerateGround(i, j);
                        break;
                    case 'b':
                        {
                            var wall = Instantiate(_wallPrefabs[0], new Vector3(i * 4, 0, j * 4), Quaternion.identity);
                            wall.transform.Translate(new Vector3(1, 0, 3));
                            wall.transform.Rotate(new Vector3(-90, 0, 0));
                            break;
                        }
                    case 'f':
                        {
                            // var wall = Instantiate(_wallPrefabs[0], new Vector3(i * 4, 0, j * 4), Quaternion.identity);
                            // wall.transform.Rotate(new Vector3(-90, 90, 0));
                            break;
                        }
                    case 'r':
                        {
                            var wall = Instantiate(_wallPrefabs[0], new Vector3(i * 4, 0, j * 4), Quaternion.identity);
                            wall.transform.Translate(new Vector3(-1, 0, 1));
                            wall.transform.Rotate(new Vector3(-90, -90, 0));
                            break;
                        }
                    case 'l':
                        {
                            var wall = Instantiate(_wallPrefabs[0], new Vector3(i * 4, 0, j * 4), Quaternion.identity);
                            wall.transform.Translate(new Vector3(3, 0, 1));
                            wall.transform.Rotate(new Vector3(-90, 90, 0));
                            break;
                        }

                }
            }
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

    void GenerateGround(int x, int y)
    {

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                Instantiate(_floorPrefabs[

                 // 0
                 Random.Range(0.0f, 1.0f) < FloorProb ? Random.Range(1, _floorPrefabs.Count) : 0
                ], new Vector3(x * 4 + i * 2, 0, y * 4 + j * 2), Quaternion.AngleAxis(-90, Vector3.right));
            }
        }
    }


    void MoveRoom(int width, int height)
    {

        if ((_currentDir & 1) == 1)
        {
            _cursorY -= Random.Range(1, height - 1);
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
                _cursorY -= height - 1;
            }

        }
    }

    void WriteRoom(int width, int height)
    {
        Debug.Log(_currentDir + " " + _cursorX + " " + _cursorY);
        _layout[_cursorX, _cursorY] = '|';
        for (int i = 1; i < width - 1; ++i)
        {
            _layout[_cursorX + i, _cursorY] = 'b';
        }
        _layout[_cursorX + width - 1, _cursorY] = '|';

        for (int i = 1; i < height - 1; ++i)
        {
            _layout[_cursorX, _cursorY + i] = 'l';
            for (int j = 1; j < width - 1; ++j)
            {
                _layout[_cursorX + j, _cursorY + i] = 'g';
            }
            _layout[_cursorX + width - 1, _cursorY + i] = 'r';
        }

        _layout[_cursorX, _cursorY + height - 1] = '|';
        for (int i = 1; i < width - 1; ++i)
        {
            _layout[_cursorX + i, _cursorY + height - 1] = 'f';
        }
        _layout[_cursorX + width - 1, _cursorY + height - 1] = '|';
    }

    void MoveRoomWall(int width, int height)
    {
        if ((_currentDir & 1) == 1)
        {
            _cursorY += Random.Range(1, height - 1);
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
                _cursorY += height - 1;
            }
        }

    }

    void MoveDir(int dir)
    {
        switch (dir)
        {
            case 0:
                _cursorY -= 1;
                break;
            case 1:
                _cursorX -= 1;
                break;
            case 2:
                _cursorY += 1;
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
        _layout[_cursorX, _cursorY] = (char)(_currentDir + '0');
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorY] = (char)(((_currentDir + 2) & 3) + '0');
        _layout[_cursorX + xOff, _cursorY + yOff] = '|';
        _layout[_cursorX - xOff, _cursorY - yOff] = '|';
        for (int i = 0; i < len - 3; ++i)
        {
            MoveDir(_currentDir);
            _layout[_cursorX, _cursorY] = 'g';
            _layout[_cursorX + xOff, _cursorY + yOff] = isVert ? 'f' : 'r';
            _layout[_cursorX - xOff, _cursorY - yOff] = isVert ? 'b' : 'l';
        }
        MoveDir(_currentDir);
        _layout[_cursorX, _cursorY] = (char)(_currentDir + '0');
        _layout[_cursorX + xOff, _cursorY + yOff] = '|';
        _layout[_cursorX - xOff, _cursorY - yOff] = '|';
        MoveDir(_currentDir);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
