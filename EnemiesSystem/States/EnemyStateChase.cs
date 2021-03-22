
using System.Runtime.Serialization.Formatters;

public class EnemyStateChase : IEnemyState
{
    public StateAction CurrentStateAction { get; set; }
    public EnemyBehavior Behaviour { get; set; }

    private bool willAttack;

    public void Enter() {
        this.Behaviour.navAgent.speed = this.Behaviour.data.runSpeed;
        this.willAttack = false;
        CurrentStateAction = StateAction.Updating;
        this.Behaviour.navAgent.SetDestination(GameManager.Instance.player.transform.position);
        this.Behaviour.MoveAnimator = 2;
    }

    public void Updating() {
        if (CurrentStateAction == StateAction.Updating) {
            if (this.Behaviour.IsSeeingPlayer()) {
                if (this.Behaviour.navAgent.hasPath) {
                    this.Behaviour.navAgent.SetDestination(GameManager.Instance.player.transform.position);
                    if(this.Behaviour.data.attackDistance <= 0) {
                        if (this.Behaviour.isTouchingCar) {
                           this.willAttack = true;
                        }
                    }else if (this.Behaviour.data.attackDistance < this.Behaviour.navAgent.stoppingDistance) {
                        this.willAttack = true;
                    }

                    if (this.willAttack) {
                        this.Exit();
                    }
                } else {
                    this.Exit();
                }
            } else{
                this.Exit();
            }
        }
    }

    public void AnimationEnd() {
    }

    public void Exit() {
        CurrentStateAction = StateAction.Exit;
        if(this.willAttack) {
            this.Behaviour.ChangeState(EnemyState.Attack);
        } else {
            this.Behaviour.ChangeState(EnemyState.Run);
        }
    }

}