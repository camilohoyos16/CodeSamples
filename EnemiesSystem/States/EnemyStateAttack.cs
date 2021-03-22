using UnityEngine;

public class EnemyStateAttack : IEnemyState
{
    private Transform attackObjective;
    private bool isAttacking;

    public StateAction CurrentStateAction { get; set; }
    public EnemyBehavior Behaviour { get; set; }

    public void Enter() {
        this.Behaviour.rigidbody.velocity = Vector3.one;
        this.Behaviour.navAgent.velocity = Vector3.one;
        this.Behaviour.navAgent.isStopped = true;
        this.isAttacking = false;
        CurrentStateAction = StateAction.Updating;
        this.Behaviour.MoveAnimator = 0;
        this.Behaviour.animator.SetBool(this.Behaviour.attackHash, true);
    }

    public void Updating() {
        if(CurrentStateAction == StateAction.Updating) {
            if (this.isAttacking)
                return;

            if((this.Behaviour.data.attackDistance <= 0 && !this.Behaviour.isTouchingCar) || 
                (this.Behaviour.data.attackDistance > 0 && this.Behaviour.data.attackDistance > this.Behaviour.DistanceWithPlayer())) {
                this.Exit();
            } else {
                this.Attack();
            }
        }
    }

    private void Attack() {
        this.isAttacking = true;
    }

    public void AnimationEnd() {
        if (this.isAttacking) {
            this.isAttacking = false;
        }
    }

    public void Exit() {
        this.Behaviour.animator.SetBool(this.Behaviour.attackHash, false);
        CurrentStateAction = StateAction.Exit;
        this.Behaviour.navAgent.isStopped = false;
        this.Behaviour.ChangeState(EnemyState.Chase);
    }
}
