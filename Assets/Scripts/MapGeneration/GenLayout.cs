using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using System;

public class GenLayout
{
	private enum Block
	{
		None,
		Ground,
		KeyGround,
		Right,
		Left,
		Front,
		Back,
		InnerFrontRight,
		InnerFrontLeft,
		InnerBackRight,
		InnerBackLeft,
		OuterFrontRight,
		OuterFrontLeft,
		OuterBackRight,
		OuterBackLeft,
		Pit,
	}

	private Func<GameObject, Vector3, Quaternion, GameObject> Instantiate;
	private GameObject parentGameObject; //gameobject that generated geometry will be attached to

	public int RoomAmt = 3;
	public int RoomSizeRange = 16;
	public int RoomSizeMin = 16;
	public float FloorProb = 0.1f;
	public float WallProb = 0.1f;
	public float InteriorProb = 0.3f;
	public float NoiseFac = 0.5f;
	public float HalfInnerMin = 1.0f;
	public float HalfInnerRange = 2.0f;
	public int Seed = 0;

	public Vector3 PlayerSpawnLocation { get; private set; }

	const int MAP_SIZE = 2048;
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


	public GenLayout(Func<GameObject, Vector3, Quaternion, GameObject> instantiate, GameObject owner, int seed)
	{
		Instantiate = instantiate;
		parentGameObject = owner;

		Seed = (int)seed;

		_random = new System.Random(Seed);
		_totalDir = _currentDir = (int)(_random.NextDouble() * 4);

		_cursorX = MAP_SIZE / 2;
		_cursorZ = MAP_SIZE / 2;
		PlayerSpawnLocation = new Vector3(_cursorX * 4 + 8, 5, _cursorZ * 4 + 8);

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
				var wall0 = _wallPrefabs[_random.NextDouble() < WallProb ? (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0];
				var wall1 = _wallPrefabs[_random.NextDouble() < WallProb ? (int)(_random.NextDouble() * (_wallPrefabs.Count - 1)) + 1 : 0];
				var status = _layout[i, j] switch
				{
					Block.KeyGround or Block.Ground => GenerateGround(i, j),
					Block.Front => GenerateFrontWall(wall0, i, j),
					Block.Back => GenerateBackWall(wall0, i, j),
					Block.Right => GenerateRightWall(wall0, i, j),
					Block.Left => GenerateLeftWall(wall0, i, j),
					Block.InnerFrontRight => GenerateFrontRightColumn(i, j),
					Block.InnerFrontLeft => GenerateFrontLeftColumn(i, j),
					Block.InnerBackRight => GenerateBackRightColumn(i, j),
					Block.InnerBackLeft => GenerateBackLeftColumn(i, j),
					Block.OuterFrontRight => GenerateFrontRightOuter(wall0, wall1, i, j),
					Block.OuterFrontLeft => GenerateFrontLeftOuter(wall0, wall1, i, j),
					Block.OuterBackRight => GenerateBackRightOuter(wall0, wall1, i, j),
					Block.OuterBackLeft => GenerateBackLeftOuter(wall0, wall1, i, j),
					_ => 0,
				};
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


	int GenerateFrontLeftOuter(GameObject wall0, GameObject wall1, int x, int z)
	{
		GenerateFrontLeftColumn(x, z);
		GenerateFrontWall(wall0, x, z);
		GenerateLeftWall(wall1, x, z);
		return 1;
	}

	int GenerateFrontRightColumn(int x, int z)
	{
		GeneratePrefab(_columnPrefab, new Vector3(x * 4 - 1, 0, z * 4 + 3), 0);
		GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), -90);
		return 1;
	}

	int GenerateFrontRightOuter(GameObject wall0, GameObject wall1, int x, int z)
	{
		GenerateFrontRightColumn(x, z);
		GenerateFrontWall(wall0, x, z);
		GenerateRightWall(wall1, x, z);
		return 1;
	}

	int GenerateFrontLeftColumn(int x, int z)
	{
		GeneratePrefab(_columnPrefab, new Vector3(x * 4 + 3, 0, z * 4 + 3), 0);
		GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), 0);
		return 1;
	}

	int GenerateBackRightOuter(GameObject wall0, GameObject wall1, int x, int z)
	{
		GenerateBackRightColumn(x, z);
		GenerateBackWall(wall0, x, z);
		GenerateRightWall(wall1, x, z);
		return 1;
	}


	int GenerateBackRightColumn(int x, int z)
	{
		GeneratePrefab(_columnPrefab, new Vector3(x * 4 - 1, 0, z * 4 - 1), 0);
		GeneratePrefab(_topsPrefabs[0], new Vector3(x * 4 + 1, 4, z * 4 + 1), 180);
		return 1;
	}
	int GenerateBackLeftOuter(GameObject wall0, GameObject wall1, int x, int z)
	{
		GenerateBackLeftColumn(x, z);
		GenerateBackWall(wall0, x, z);
		GenerateLeftWall(wall1, x, z);
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
		go.transform.parent = parentGameObject.transform;
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
				ground.transform.parent = parentGameObject.transform;
			}
		}
		return 1;
	}



	private static Block[] s_rightTable = new Block[] {
		Block.None,
		Block.None,
		Block.KeyGround,
		Block.Right,
		Block.None,
		Block.OuterFrontRight,
		Block.OuterBackRight,
		Block.Right,
		Block.None,
		Block.Right,
		Block.None,
	};
	private static Block[] s_leftTable = new Block[] {
		Block.None,
		Block.None,
		Block.KeyGround,
		Block.None,
		Block.Left,
		Block.OuterFrontLeft,
		Block.OuterBackLeft,
		Block.None,
		Block.Left,
		Block.None,
		Block.Left,
	};
	private static Block[] s_frontTable = new Block[] {
		Block.None,
		Block.None,
		Block.KeyGround,
		Block.OuterFrontRight,
		Block.OuterFrontLeft,
		Block.Front,
		Block.None,
		Block.Front,
		Block.Front,
		Block.None,
		Block.None,
	};
	private static Block[] s_backTable = new Block[] {
		Block.None,
		Block.None,
		Block.KeyGround,
		Block.OuterBackRight,
		Block.OuterBackLeft,
		Block.None,
		Block.Back,
		Block.None,
		Block.None,
		Block.Back,
		Block.Back,
	};

	void WriteWallAdd(int x, int z, Block val)
	{
		_layout[x, z] = _layout[x, z] switch
		{
			Block.Right => s_rightTable[(int)val],
			Block.Left => s_leftTable[(int)val],
			Block.Front => s_frontTable[(int)val],
			Block.Back => s_backTable[(int)val],
			Block.KeyGround => Block.KeyGround,
			_ => _layout[x, z] = val,
		};
	}


	void WriteWeak(int x, int z, Block val)
	{
		if (_layout[x, z] == Block.None)
		{
			_layout[x, z] = val;
		}
	}

	Vector3 CheckRoom(int width, int depth)
	{
		return new Vector3();
		// var foundMask = 0;
		// Vector3 minKeyDiff;
		// var recordDist = 0;

		// for (int i = 0; i < width; ++i) {
		//     for (int j = 0; j < depth; ++j) {
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
		WriteWallAdd(_cursorX, _cursorZ, Block.InnerFrontLeft);
		for (int i = 1; i < width - 1; ++i)
		{
			WriteWallAdd(_cursorX + i, _cursorZ, Block.Front);
		}
		WriteWallAdd(_cursorX + width - 1, _cursorZ, Block.InnerFrontRight);

		for (int i = 1; i < depth - 1; ++i)
		{
			WriteWallAdd(_cursorX, _cursorZ + i, Block.Left);
			for (int j = 1; j < width - 1; ++j)
			{
				WriteWeak(_cursorX + j, _cursorZ + i, Block.Ground);
			}
			WriteWallAdd(_cursorX + width - 1, _cursorZ + i, Block.Right);
		}

		WriteWallAdd(_cursorX, _cursorZ + depth - 1, Block.InnerBackLeft);
		for (int i = 1; i < width - 1; ++i)
		{
			WriteWallAdd(_cursorX + i, _cursorZ + depth - 1, Block.Back);
		}
		WriteWallAdd(_cursorX + width - 1, _cursorZ + depth - 1, Block.InnerBackRight);

		var sym = _random.NextDouble();
		if (sym < .4f)
		{
			var interiorAmt = (int)(_random.NextDouble() * HalfInnerRange) + HalfInnerMin;
			for (int i = 0; i < interiorAmt; ++i)
			{

			}
		}
		else if (sym < .8f)
		{
			var interiorAmt = (int)(_random.NextDouble() * HalfInnerRange) + HalfInnerMin;
			for (int i = 0; i < interiorAmt; ++i)
			{

			}
		}
		else
		{

			var interiorAmt = (int)(_random.NextDouble() * HalfInnerRange * 2) + HalfInnerMin * 2;
			for (int i = 0; i < interiorAmt; ++i)
			{

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
			if ((_currentDir == 3 && !inv) || (_currentDir == 1 && inv))
			{
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
		_layout[_cursorX, _cursorZ] = Block.KeyGround;
		MoveDir(_currentDir);
		_layout[_cursorX, _cursorZ] = Block.KeyGround;
		WriteWallAdd(_cursorX + xOff, _cursorZ + zOff, b0);
		WriteWallAdd(_cursorX - xOff, _cursorZ - zOff, b1);
		for (int i = 0; i < len; ++i)
		{
			MoveDir(_currentDir);
			_layout[_cursorX, _cursorZ] = Block.KeyGround;//isHor ? 'h' : 'v';
			_layout[_cursorX + xOff, _cursorZ + zOff] = b0;
			_layout[_cursorX - xOff, _cursorZ - zOff] = b1;
		}
		MoveDir(_currentDir);
		_layout[_cursorX, _cursorZ] = Block.KeyGround; //isHor ? 'h' : 'v';
		_layout[_cursorX + xOff, _cursorZ + zOff] = b0;
		_layout[_cursorX - xOff, _cursorZ - zOff] = b1;
		MoveDir(_currentDir);
		_layout[_cursorX, _cursorZ] = Block.KeyGround;
		_layout[_cursorX + xOff, _cursorZ + zOff] = b0;
		_layout[_cursorX - xOff, _cursorZ - zOff] = b1;
		MoveDir(_currentDir);
		_layout[_cursorX, _cursorZ] = Block.KeyGround;
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
