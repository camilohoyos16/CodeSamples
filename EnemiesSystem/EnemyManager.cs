using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    OnFire,
    Injured,
    Crawler
}

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Spawn")]
    public EnemySpawnController enemySpawn;
    private int enemiesQuantity = 100;

    private List<EnemyController> enemies = new List<EnemyController>();
    private List<EnemyController> enemiesFound = new List<EnemyController>(); //Reusing to return required enemies
    
    //QuadTree
    public GameObject navigationFloor; //This could be an area, but for now it will be the invisible floor below city
    private QuadTree quadTree;
    private Boundary baseBoundaries;
    private float timeToCreateQuad = 1f;
    private float createQuadTimer = 0;

    private void Awake() {
    }

    public int GetEnemiesCount() {
        return this.enemies.Count;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.CreateQuad();

        this.enemySpawn.SpawnEnemiesRandomPosition(this.enemiesQuantity, this.enemies);

        EventManager.Instance.AddListener<OnKilledZombieEvent>(this.OnEnemyKilled);
        EventManager.Instance.AddListener<OnClearLevelEvent>(this.OnClearGame);
    }

    private void OnDestroy() {
        if (EventManager.HasInstance()) {
            EventManager.Instance.RemoveListener<OnKilledZombieEvent>(this.OnEnemyKilled);
            EventManager.Instance.RemoveListener<OnClearLevelEvent>(this.OnClearGame);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.createQuadTimer < Time.time) {
            this.createQuadTimer = this.timeToCreateQuad + Time.time;
            this.UpdateQuad();
        }
    }

    public void SpawnEnemies(Transform position, int quantity = 1) {
        this.SpawnEnemies(position.position, quantity);

    }
    public void SpawnEnemies(Vector3 position, int quantity = 1) {
        this.enemies.AddRange(this.enemySpawn.SpawnEnemies(position, quantity));
    }

    public void GetEnemiesNearPlayer(Boundary range) {
        this.enemiesFound.Clear();
        this.quadTree.Query(range, this.enemiesFound);

        for (int i = 0; i < this.enemiesFound.Count; i++) {
            this.enemiesFound[i].NeedCheckWithPlayer = true;
        }
    }

    public void UpdateQuad() {
        this.quadTree = new QuadTree(baseBoundaries, 4);

        for (int i = 0; i < this.enemies.Count; i++) {
            this.enemies[i].NeedCheckWithPlayer = false;
            this.quadTree.InsertPoint(this.enemies[i]);
        }

        this.GetEnemiesNearPlayer(GameManager.Instance.GetPlayerZone());

        GC.Collect();
    }

    private void CreateQuad() {
        MeshRenderer mesh = this.navigationFloor.GetComponent<MeshRenderer>();
        this.baseBoundaries = new Boundary(
            mesh.bounds.center.x,
            mesh.bounds.center.z,
            mesh.bounds.size.x,
            mesh.bounds.size.z
            );

        this.quadTree = new QuadTree(baseBoundaries, 4);
    }

    #region Events

    private void OnEnemyKilled(OnKilledZombieEvent e) {
        this.enemies.Remove(e.enemy);
        PoolManager.Instance.ReleaseObject(string.Concat(Env.ENEMY_POOL_PATH, e.enemy.type), e.enemy.gameObject);
    }

    private void OnClearGame(OnClearLevelEvent e) {
        for (int i = 0; i < this.enemies.Count; i++) {
            PoolManager.Instance.ReleaseObject(string.Concat(Env.ENEMY_POOL_PATH, this.enemies[i].type), this.enemies[i].gameObject);
        }

        this.enemies.Clear();
    }


    #endregion
}
