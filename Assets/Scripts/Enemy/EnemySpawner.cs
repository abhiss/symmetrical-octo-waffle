using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Shared;
using UnityEngine.Assertions;

class EnemySpawner : NetworkBehaviour
{
    [System.Serializable] 
    public class SpawnData
    {
        public SpawnData(GameObject enemy, float spawnChance)
        {
            this.Enemy = enemy;
            this.SpawnChance = spawnChance;
        }
        public GameObject Enemy;
        [Range(0, 1)]
        public float SpawnChance;
    }

    [SerializeField] private List<SpawnData> _enemySpawns;
    [SerializeField] [Range(1, 100)] private float _timeBetweenSpawns;
    private float _timeTowardsNextSpawn; 
    private float _sumOfSpawnChances;
    private AudioSource _audio;
    private HealthSystem _healthSystem;
    [SerializeField] private GameObject _deathExplosion;

    /* 
    Spawn rate is the reciprocal of timeBetweenSpawns for naming clarity.
    For example, a spawn rate of 2 means enemies will spawn every 1/2 seconds. 
    */
    public float SpawnRate { get =>  1 / _timeBetweenSpawns; set => _timeBetweenSpawns = 1 / value; }

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _healthSystem = GetComponent<HealthSystem>();
        InitializeOnDamageEvent();
        foreach(SpawnData spawn in _enemySpawns)
        {
            _sumOfSpawnChances += spawn.SpawnChance;
        }
        NormalizeSpawnRates();
        return;
    }

    private void Update()
    {
        if (!IsServer) {
            Debug.Log($"isserver={IsServer} isclient={IsClient} ishost={IsHost} isspawned={IsSpawned}");
            return;
        };
        if (_timeTowardsNextSpawn >= _timeBetweenSpawns)
        {
            RandomSpawnEnemy();
        }
        _timeTowardsNextSpawn += Time.deltaTime;
    }

    private void InitializeOnDamageEvent()
    {
        _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => 
        {
            // If we are dead, spawn an explosion and destroy ourselves.
            if (args.newHealth <= 0)
            {
                Instantiate(_deathExplosion, transform.position, Quaternion.identity);
                Destroy(gameObject);
                return;
            }
        });
    }

    // Randomly spawns an enemy based on spawn chance.
    public void RandomSpawnEnemy()
    {
        _timeTowardsNextSpawn = 0;
        float randomNumber = UnityEngine.Random.Range(0.0f, 1.0f);
        float lowerRange = 0;
        float upperRange = 0;
        foreach (SpawnData spawn in _enemySpawns)
        {
            upperRange += spawn.SpawnChance;
            if (randomNumber >= lowerRange && randomNumber <= upperRange)
            {
                SpawnEnemy(spawn.Enemy);
                return;
            }
            lowerRange = upperRange;
        }
    }

    public void SpawnEnemy(GameObject enemy)
    {
        Assert.IsTrue(IsServer);
        _audio.Play();
        GameObject go = Instantiate(enemy, Vector3.zero, Quaternion.identity);
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 20.0f, UnityEngine.AI.NavMesh.AllAreas);
        var nav = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        nav.Warp(hit.position+ Vector3.up);
        nav.enabled = true;
        nav.isStopped = false;
        go.GetComponent<NetworkObject>().Spawn(); //spawns across network (all clients);
    }

    // Adds an snemy to the spawn list with a spawn chance, or updates it if one with that enemy's name already exists.
    public void AddEnemySpawn(GameObject enemy, float spawnChance)
    {
        float clampedSpawnChance = Mathf.Clamp(spawnChance, 0f, 1f);
        SpawnData enemySpawn = FindEnemySpawn(enemy);
        if (enemySpawn is null)
        {
            _enemySpawns.Add(new SpawnData(enemy, clampedSpawnChance));
            _sumOfSpawnChances += clampedSpawnChance;
        }
        else
        {
            _sumOfSpawnChances -= enemySpawn.SpawnChance;
            _sumOfSpawnChances += clampedSpawnChance;
            enemySpawn.SpawnChance = clampedSpawnChance;
        }
        NormalizeSpawnRates();
    }

   // Removes an enemy from the spawn list based on its name.
    public void RemoveEnemySpawn(GameObject enemy)
    {
        int i = 0;
        foreach (SpawnData spawn in _enemySpawns)
        {
            if (spawn.Enemy.name == enemy.name)
            {
                _sumOfSpawnChances -= spawn.SpawnChance;
                _enemySpawns.RemoveAt(i);
                NormalizeSpawnRates();
                return;
            }
            i++;
        }
    }

    public void ClearEnemySpawns()
    {
        _enemySpawns.Clear();
        _sumOfSpawnChances = 0;
    }

    // Find if an enemy exists in spawn list based on name, and return it if it does. Otherwise, return null.
    private SpawnData FindEnemySpawn(GameObject enemy)
    {
        foreach (SpawnData spawn in _enemySpawns)
        {
            if (spawn.Enemy.name == enemy.name)
            {
                return spawn;
            }
        }
        return null;
    }

    // Set all spawn rates to sum up to 1.
    private void NormalizeSpawnRates()
    {
        if (_sumOfSpawnChances == 0)
        {
            return;
        }
        foreach (SpawnData spawn in _enemySpawns)
        {
            spawn.SpawnChance /= _sumOfSpawnChances;
        }
    }
}