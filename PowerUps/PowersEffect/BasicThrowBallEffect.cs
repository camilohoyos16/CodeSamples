using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicThrowBallEffect : PowersEffect
{
    public int attackPower;
    public override void Init()
    {
        if (!this.isInit) {
            this.isInit = true;
            this.ownCollider = this.gameObject.GetComponent<Collider>();
        }
    }

    public override void InitEffect(Power data, CharacterComponents attacker)
    {
        this.Init();
        this.attackPower = data.attackPower;
        this.attacker = attacker;
        this.ownCollider.enabled = false;
        this.destroyDelay = new WaitForSeconds(this.aliveTime);
        this.StartToEffect();
    }


    public override void StartToEffect()//Could use a coroutine for delay effect
    {
        this.ownCollider.enabled = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        CharacterComponents components = other.GetComponent<CharacterComponents>();
        if(components != null) {
            if(components != this.attacker) {
                this.victim = components;
                this.TriggerPowerEffect();
            }
        }
    }

    public override void TriggerPowerEffect() 
    {
        this.Effect();
    }

    public override void Effect()
    {
        int damage = this.attackPower + this.attacker.Attack.attackPower;
        int newHealt = Mathf.Clamp(this.victim.Health.currentlHealth - damage, 0, this.victim.Health.maxHealth);
        this.victim.Health.SetHeatlh(newHealt);
        StartCoroutine(this.DestroyEffect());
    }

    private IEnumerator DestroyEffect()
    {
        yield return this.destroyDelay;
        this.Destroy();
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }
}
