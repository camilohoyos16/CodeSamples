using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PecuecaBossAttack : BossAttack
{
    PecuecaBullet currentBullet;

    protected override void ChargeAttack() {
        base.ChargeAttack();

        this.currentBullet = ResourcesManager.Instance.GetPecuecaBullet();
        PoolManager.Instance.recoverAllObjectsCallback += this.currentBullet.DestroyObject;
        this.currentBullet.transform.position = this.env.boss.transform.position;
        this.currentBullet.transform.DOScale(1, this.chargeTime).From(0);
    }

    protected override void MakeAttack() {
        base.MakeAttack();
        this.currentBullet.Shoot(this.attackPosition);
        this.stateAction?.Invoke();
        this.FinishAttack();
    }
}
