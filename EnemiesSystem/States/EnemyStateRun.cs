using UnityEngine.AI;

public class EnemyStateRun : IEnemyState
{
    public StateAction CurrentStateAction { get ; set ; }
    public EnemyBehavior Behaviour { get; set; }

    private bool sawPlayer;

    public void Enter() {
        this.Behaviour.navAgent.speed = this.Behaviour.data.runSpeed;
        this.sawPlayer = false;
        CurrentStateAction = StateAction.Updating;
        this.Behaviour.navAgent.SetDestination(GameManager.Instance.player.transform.position);
        this.Behaviour.MoveAnimator = 2;
    }

    public void Updating() {
        if (CurrentStateAction == StateAction.Updating) {
            if (this.Behaviour.IsSeeingPlayer() && this.Behaviour.CanReachToPlayer()) {
                this.sawPlayer = true;
                this.Exit();
            } else if (!this.Behaviour.navAgent.hasPath || this.Behaviour.navAgent.velocity.magnitude <= 0) {
                this.Exit();
            }
        }
    }

    public void AnimationEnd() {
    }


    public void Exit() {
        CurrentStateAction = StateAction.Exit;
        if (this.sawPlayer) {
            this.Behaviour.destinationPoint = GameManager.Instance.player.transform.position;
            this.Behaviour.ChangeState(EnemyState.Chase);
        } else {
            this.Behaviour.ChangeState(EnemyState.Wander);
        }
    }

}
