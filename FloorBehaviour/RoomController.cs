using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class RoomController : MonoBehaviour
{
    public GameObject roomEntitiesParent;
    private List<RoomEntity> roomEntities = new List<RoomEntity>();

    public Transform enemiesParent;
    public Transform propsParent;
    public Transform interactivePropsParent;

    [Space(10)]

    public SpriteShapeController floor;
    public DoorRoomController enterDoor;
    public DoorRoomController exitDoor;

    private List<EnemyController> roomEnemies = new List<EnemyController>();
    private List<EnemySpawnerController> roomEnemySpawners = new List<EnemySpawnerController>();
    private List<InteractiveProp> roomInterectiveProps = new List<InteractiveProp>();
    private List<GameObject> roomProps = new List<GameObject>();

    private bool isAlreadyInit;
    public int roomNumber;
    private GameObject playerGO;

    public LevelState currenLevelState;
    private Action<LevelState> changedLevelStateAction;

    // Start is called before the first frame update
    protected void Start()
    {
        this.playerGO = GameManager.Instance.PlayerGO;
    }

    private void GetRoomEntities() {
        RoomEntity[] entities = this.roomEntitiesParent.GetComponentsInChildren<RoomEntity>();
        foreach (RoomEntity entity in entities) {
            if(entity.gameObject.GetHashCode() != this.gameObject.GetHashCode()) {
                this.roomEntities.Add(entity);
            }
        }
    }

    private void InstantiateEntities() {
        for (int i = 0; i < roomEntities.Count; i++) {
            roomEntities[i].InitEntity(enemiesParent, propsParent, interactivePropsParent);
            var enemy = roomEntities[i].currentEntity.GetComponent<EnemyController>();
            if (enemy != null) {
                enemy.roomController = this;
                this.changedLevelStateAction += enemy.OnRoomStateChanged;
                enemy.Spawn();
                roomEnemies.Add(enemy);
            } else {
                var interactiveProp = roomEntities[i].currentEntity.GetComponent<InteractiveProp>();
                if (interactiveProp != null) {
                    var enemySpawner = roomEntities[i].currentEntity.GetComponent<EnemySpawnerController>();
                    if (enemySpawner != null) {
                        enemySpawner.roomController = this;
                        roomEnemySpawners.Add(enemySpawner);
                    } else {
                        interactiveProp.roomController = this;
                        roomInterectiveProps.Add(interactiveProp);
                    }
                } else {
                    roomProps.Add(roomEntities[i].currentEntity);
                }
            }
        }
    }

    private void OnDestroy() {
        if (EventManager.HasInstance()) {
            EventManager.Instance.RemoveListener<OnEnemySpawnedEvent>(this.OnEnemiesSpawnedListener);
            EventManager.Instance.RemoveListener<OnEnemyDeathEvent>(this.OnEnemiesDeathListener);
            EventManager.Instance.RemoveListener<OnEnemySpawnerDestroyedEvent>(this.OnEnemySpawnerDeathListener);
        }
    }

    public void ChangeRoomPosition(Vector3 newPosition) {
        this.transform.position = newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangeLevelState(LevelState newState) {
        switch (newState) {
            case LevelState.Waiting:
                break;
            case LevelState.Starting:
            case LevelState.Ending:
                GameManager.Instance.ChangeCurrentState(GameState.Cutscene);
                break;
            case LevelState.Playing:
                GameManager.Instance.ChangeCurrentState(GameState.Playing);
                break;
            case LevelState.Cleaning:
                break;
            default:
                break;
        }
        this.currenLevelState = newState;
        this.changedLevelStateAction?.Invoke(newState);
    }

    public void PlayerEnterRoom(Action postAction) {
        this.ChangeLevelState(LevelState.Starting);
        this.enterDoor.EnterPlayer(this.playerGO, 1, delegate () {
            this.ActiveRoom();
            postAction?.Invoke();
        });

    }

    public void PlayerExitRoom(Action postAction) {
        this.ChangeLevelState(LevelState.Ending);
        this.exitDoor.ExitPlayer(this.playerGO, 1, delegate () {
            this.ChangeLevelState(LevelState.Cleaning);
            this.CleanRoom();
            postAction?.Invoke();
        });
    }

    protected virtual void ActiveRoom() {
        this.enterDoor.LockDoor();
        this.ChangeLevelState(LevelState.Playing);

        EventManager.Instance.AddListener<OnEnemySpawnedEvent>(this.OnEnemiesSpawnedListener);
        EventManager.Instance.AddListener<OnEnemyDeathEvent>(this.OnEnemiesDeathListener);
        EventManager.Instance.AddListener<OnEnemySpawnerDestroyedEvent>(this.OnEnemySpawnerDeathListener);
    }

    public virtual void DrawRoom() {
        if (!this.isAlreadyInit) {
            this.isAlreadyInit = true;
            this.GetRoomEntities();
        }
        this.InstantiateEntities();
        this.CheckCondition();
        this.ChangeLevelState(LevelState.Waiting);
        this.floor.spriteShape = ResourcesManager.Instance.GetFloor(Env.FLOOR_SPRITE_SHAPES_POOL_PATH.GetRandomEntry());
    }


    protected virtual void CheckCondition() {
        bool isDone = true;
        if(this.roomEnemies.Count > 0) {
            isDone = false;
        }

        if (this.roomEnemySpawners.Count > 0) {
            isDone = false;
        }

        if (isDone) {
            this.RoomIsDone();
        }
    }

    protected virtual void RoomIsDone() {
        //Active Next Room
        this.exitDoor.UnlockDoor();

        EventManager.Instance.RemoveListener<OnEnemySpawnedEvent>(this.OnEnemiesSpawnedListener);
        EventManager.Instance.RemoveListener<OnEnemyDeathEvent>(this.OnEnemiesDeathListener);
        EventManager.Instance.RemoveListener<OnEnemySpawnerDestroyedEvent>(this.OnEnemySpawnerDeathListener);
    }

    public void RecycleRoom() {
        this.ChangeLevelState(LevelState.Waiting);
        PoolManager.Instance.ReleaseObject(this.gameObject);
    }

    public void CleanRoom() {
        this.exitDoor.LockDoor();

        int index = this.roomProps.Count;
        for (int i = 0; i < index; i++) {
            PoolManager.Instance.ReleaseObject(this.roomProps[i]);
        }
        this.roomProps.Clear();

        index = this.roomInterectiveProps.Count;
        for (int i = 0; i < index; i++) {
            this.roomInterectiveProps[i].ResetProp();
            PoolManager.Instance.ReleaseObject(this.roomInterectiveProps[i].gameObject);
        }
        this.roomInterectiveProps.Clear();
        this.changedLevelStateAction = null;
    }
    
    private void OnEnemiesSpawnedListener(OnEnemySpawnedEvent e) {
        e.enemySpawned.roomController = this;
        e.enemySpawned.transform.SetParent(this.enemiesParent);
        this.roomEnemies.Add(e.enemySpawned);
    }

    private void OnEnemiesDeathListener(OnEnemyDeathEvent e) {
        this.roomEnemies.Remove(e.enemyDeath);

        this.CheckCondition();
    }

    private void OnEnemySpawnerDeathListener(OnEnemySpawnerDestroyedEvent e) {
        this.roomEnemySpawners.Remove(e.spawnerDestroyed);

        this.CheckCondition();
    }
}
