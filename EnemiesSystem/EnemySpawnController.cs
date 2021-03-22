using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    public Transform[] spawnPoints;

    private bool isSpawningEnemies;
    private float spawnDelayTime = 0.01f;
    private WaitForSeconds spawnDelay;

    private void Start() {
        this.spawnDelay = new WaitForSeconds(this.spawnDelayTime);
    }

    public void SpawnEnemiesRandomPosition(int quantity, List<EnemyController> enemies) {
        this.isSpawningEnemies = true;
        StartCoroutine(this.SpawnRandomPosition(quantity, enemies));
    }

    private IEnumerator SpawnRandomPosition(int quantity, List<EnemyController> enemies) {
        for (int i = 0; i < quantity; i++) {
            yield return this.spawnDelay;
            Vector3 random = Random.insideUnitCircle * 3;
            random.z = random.y;
            random.y = 0;
            enemies.Add(this.SpawnEnemy(this.spawnPoints.GetRandomEntry().position + random));
            //enemies.Add(this.SpawnEnemy(this.spawnPoints[0].position + random));
        }
        this.isSpawningEnemies = false;
    }

    public EnemyController[] SpawnEnemies(Vector3 position, int quantity) {
        EnemyController[] enemies = new EnemyController[quantity];
        for (int i = 0; i < quantity; i++) {
            enemies[i] = this.SpawnEnemy(position);
        }

        return enemies;
    }

    private EnemyController SpawnEnemy(Vector3 position) {
        EnemyController newEnemy = ResourcesManager.Instance.GetEnemyPrefab((EnemyType)Random.Range(0, 3)).GetComponent<EnemyController>();
        newEnemy.Init();
        newEnemy.behavior.navAgent.Warp(position);
        return newEnemy;
    }
}
