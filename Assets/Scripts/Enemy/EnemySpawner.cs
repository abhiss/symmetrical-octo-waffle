using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies; //Enemy Spawn List
    [SerializeField] private GameObject nextEnemyToSpawn; //Next enemy that will be spawned
    [SerializeField] private Vector3 defaultSpawnPosition;
    private Random rndNumGenerator;

    [SerializeField] private float spawnInterval; //Time to spawn an enemy
    private float timeTowardsSpawn; //Time elapsed towards next spawn, initialized to 0 after meeting or exceeding spawnInterval
    private float elapsedTime; //Total time elapsed since start of game

    void Spawn(GameObject enemy, Vector3 spawnPos)
    {
        Instantiate(enemy, spawnPos, Quaternion.identity);
    }

    void Spawn(Vector3 spawnPos)
    {
        if (enemies.Length <= 0) { return; }
        if (nextEnemyToSpawn == null)
        {
            int numEnemies = enemies.Length;
            int enemyIndex = rndNumGenerator.Next(0, numEnemies);
            nextEnemyToSpawn = enemies[enemyIndex];
        }
        Spawn(nextEnemyToSpawn, spawnPos);
    }

    bool CheckValidSpawnTime()
    {
        elapsedTime += Time.deltaTime;
        timeTowardsSpawn += Time.deltaTime;
        if (timeTowardsSpawn >= spawnInterval)
        {
            timeTowardsSpawn = 0;
            return true;
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        rndNumGenerator = new Random();
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldSpawnEnemy = CheckValidSpawnTime();
        if (shouldSpawnEnemy)
        {
            Spawn(defaultSpawnPosition);
        }
    }
}
