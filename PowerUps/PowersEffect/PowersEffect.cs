using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowersEffect : MonoBehaviour
{
    public CharacterComponents attacker;
    public CharacterComponents victim;
    public Collider ownCollider;
    public float aliveTime;
    protected WaitForSeconds destroyDelay;
    protected bool isInit;
    public abstract void Init();
    public abstract void InitEffect(Power data, CharacterComponents attacker);
    public abstract void StartToEffect();
    public abstract void TriggerPowerEffect();
    public abstract void Effect();

    public void GetFromPool() {
        this.ownCollider.enabled = true;
    }

    public void ReturnToPool(string poolName) {
        PoolManager.Instance.Push(this, poolName);
    }
}
