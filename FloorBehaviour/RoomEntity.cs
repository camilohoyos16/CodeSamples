using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEntity : MonoBehaviour
{
    [HideInInspector]
    public GameObject currentEntity;

    [SerializeField] private bool isEnemy;
    [SerializeField] private bool isProp;
    [SerializeField] private bool isInteractiveProp;
    [SerializeField] private bool isEnemySpawner;
    [SerializeField] private GameObject sprite;

    private List<string> entityOptions = new List<string>();

    public void InitEntity(Transform enemiesParent, Transform propsParent, Transform interactivePropsParent)
    {
        entityOptions.Clear();

        if (isEnemy)
        {
            entityOptions.Add(Env.ENEMY_ID);
        }

        if (isProp)
        {
            entityOptions.Add(Env.PROP_ID);
        }

        if (isInteractiveProp)
        {
            entityOptions.Add(Env.INTERACTIVE_PROP_ID);
        }

        if (isEnemySpawner) {
            entityOptions.Add(Env.ENEMY_SPAWNER_ID);
        }

        string entitySelected = entityOptions[Random.Range(0, entityOptions.Count)];

        switch (entitySelected)
        {
            case Env.ENEMY_ID:
                this.currentEntity = ResourcesManager.Instance.GetEnemy(Env.ENEMY_POOL_PATHS.GetRandomEntry()).gameObject;
                this.currentEntity.transform.position = this.transform.position;
                this.currentEntity.transform.SetParent(enemiesParent);
                break;
            case Env.PROP_ID:
                this.currentEntity = ResourcesManager.Instance.GetGameObject(Env.PROPS_PATHS.GetRandomEntry());
                this.currentEntity.transform.position = this.transform.position;
                this.currentEntity.transform.SetParent(propsParent);
                break;
            case Env.INTERACTIVE_PROP_ID:
                this.currentEntity = ResourcesManager.Instance.GetGameObject(Env.INTERACTIVE_PROPS_PATH.GetRandomEntry());
                this.currentEntity.transform.position = this.transform.position;
                this.currentEntity.transform.SetParent(interactivePropsParent);
                break;
            case Env.ENEMY_SPAWNER_ID:
                this.currentEntity = ResourcesManager.Instance.GetEnemySpawner().gameObject;
                this.currentEntity.transform.position = this.transform.position;
                this.currentEntity.transform.SetParent(enemiesParent);
                this.currentEntity.GetComponent<EnemySpawnerController>().SpawnSpawner();
                break;
        }

        sprite.SetActive(false);
    }
}
