using UnityEngine;

public class EnemyStateWander : IEnemyState
{
    private GameObject moveObjective;
    private float waitTime = 3;
    private float waitTimer = 0;
    private bool isWaiting;

    public StateAction CurrentStateAction { get; set ; }
    public EnemyBehavior Behaviour { get ; set ; }

    public void Enter() {
        this.Behaviour.navAgent.speed = this.Behaviour.data.walkSpeed;
        this.GetNewDestination();
        CurrentStateAction = StateAction.Updating;
        this.isWaiting = false;
    }

    public void Updating() {
        if (CurrentStateAction == StateAction.Updating) {

            if (this.waitTimer < Time.time) {
                if (this.Behaviour.navAgent.remainingDistance <= this.Behaviour.navAgent.stoppingDistance + 0.2f && this.Behaviour.navAgent.hasPath) {
                    if (!this.isWaiting) {
                        this.ReachedToPoint();
                    } else {
                        this.GetNewDestination();
                    }
                }
            }

            if (!this.Behaviour.needCheckWithPlayer)
                return;

            if (this.Behaviour.IsSeeingPlayer() && this.Behaviour.CanReachToPlayer()) {
                this.Exit();
            } 
        }
    }

    public void ReachedToPoint() {
        this.isWaiting = true;
        this.waitTimer = this.waitTime + Time.time;
        this.Behaviour.MoveAnimator = 0;
        //this.Behaviour.navAgent.isStopped = true;
    }

    public void GetNewDestination() {
        //this.Behaviour.navAgent.isStopped = false;
        this.isWaiting = false;
        this.waitTimer = this.waitTime + Time.time;
        Vector3 newPoint = Random.insideUnitCircle.normalized * this.Behaviour.data.wanderRadius;
        newPoint.z = newPoint.y;
        newPoint += this.Behaviour.gameObject.transform.position;
        newPoint.y = 1;
        this.Behaviour.navAgent.destination= newPoint;
        this.Behaviour.MoveAnimator = 1;
    }

    public void AnimationEnd() {
    }

    public void Exit() {
        CurrentStateAction = StateAction.Exit;
        this.Behaviour.ChangeState(EnemyState.Run);
    }

}
