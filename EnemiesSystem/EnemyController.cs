using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    public bool NeedCheckWithPlayer { 
        get { return behavior.needCheckWithPlayer; } 
        set { behavior.needCheckWithPlayer = value; } 
    }

    public EnemyData data;
    public EnemyType type;

    [HideInInspector] public EnemyBehavior behavior;
    Coroutine disactiveRagdollCoroutine;
    WaitForSeconds ragdollDelay;

    private bool alreadyInit;
    private bool isDead;

    private void Start() {
    }

    public void Init() {
        this.isDead = false;
        if (this.alreadyInit)
            return;

        this.alreadyInit = true;
        this.behavior = this.gameObject.AddComponent<EnemyBehavior>();
        this.behavior.data = this.data;
        this.ragdollDelay = new WaitForSeconds(2);
    }

    public void AnimationTrigger() {
        this.behavior.AnimationTrigger();
    }

    public void TouchedCar(bool value) {
        this.behavior.isTouchingCar = value; 
    }

    public void HittedByCar(bool wasKill) {
        this.isDead = wasKill;
        if (!this.behavior.isRagdoll) {
            this.behavior.TurnOnRagdoll();
        } else {
            StopCoroutine(this.disactiveRagdollCoroutine);
        }
        this.disactiveRagdollCoroutine = StartCoroutine(TurnOffRagdoll());
    }

    public IEnumerator TurnOffRagdoll() {
        yield return this.ragdollDelay;

        while(this.behavior.ragdollRigidbodies[0].velocity.magnitude > 0.1f ) {
            yield return this.ragdollDelay;
        }

        this.disactiveRagdollCoroutine = null;
        this.behavior.TurnOffRagdoll();
        if (this.isDead) {
            this.behavior.EnemyDead();
            EventManager.Instance.Trigger(new OnKilledZombieEvent() {
                enemy = this
            });
        }
    }
}

public class EnemyBehavior : MonoBehaviour
{
    private EnemyState currentState = EnemyState.Wander;
    private IEnemyState currentActionState;

    public bool needCheckWithPlayer;

    //Stats
    public EnemyData data;

    public bool isRagdoll;
    public bool isTouchingCar;


    //Cache variables
    private EnemyStateAttack attackState = new EnemyStateAttack();
    private EnemyStateWander wanderState = new EnemyStateWander();
    private EnemyStateRun runState = new EnemyStateRun();
    private EnemyStateChase chaseState = new EnemyStateChase();

    public Vector3 destinationPoint;
    public RaycastHit seeingCheckHit;
    public Vector3 seeingCheckVector;

    //Main Charcater
    [HideInInspector] public Animator animator;
    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public Collider collider;
    [HideInInspector] public Rigidbody rigidbody;

    //Ragdoll
    public GameObject ragdollStomach;
    private GameObject ragdollGO;
    public SkinnedMeshRenderer mesh;
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;

    //Animator
    private int moveAnimator;
    public int MoveAnimator {
        get { return this.moveAnimator; }
        set { this.moveAnimator = value;
            this.SetMoveAnimationState(value);
        }
    }
    public int attackHash;
    public int moveHash;
    public int walkMultHash;
    public int runMultHash;

    private void OnBecameVisible() {
        if (!this.isRagdoll) {
            this.ragdollGO.SetActive(true);
            this.collider.enabled = true;
            this.rigidbody.useGravity = true;
            this.SetMoveAnimationState(this.moveAnimator);
            this.animator.SetFloat(this.walkMultHash, this.data.walkAnimationMult);
            this.animator.SetFloat(this.runMultHash, this.data.runAnimationMult);
        } else {
            this.mesh.updateWhenOffscreen = false;
        }
    }

    private void OnBecameInvisible() {
        if (!this.isRagdoll) {
            this.ragdollGO.SetActive(false);
            this.collider.enabled = false;
            this.rigidbody.useGravity = false;
        } else {
            this.mesh.updateWhenOffscreen = true;
        }
    }

    protected void Awake() {
        this.ragdollGO = this.transform.FindRecursive("Ragdoll").gameObject;
        this.mesh = this.transform.FindRecursive("Cube.013").gameObject.GetComponent<SkinnedMeshRenderer>();
        

        this.ragdollRigidbodies = this.ragdollGO.GetComponentsInChildren<Rigidbody>(true);
        this.ragdollColliders = this.ragdollGO.GetComponentsInChildren<Collider>(true);

        this.navAgent = this.GetComponent<NavMeshAgent>();
        this.collider = this.GetComponent<Collider>();
        this.rigidbody = this.GetComponent<Rigidbody>();
        this.animator = this.ragdollGO.GetComponent<Animator>();

        this.ragdollStomach = this.ragdollGO.transform.FindRecursive("Stomach").gameObject;

        this.attackHash = Animator.StringToHash("Attack");
        this.moveHash = Animator.StringToHash("Move");
        this.walkMultHash = Animator.StringToHash("WalkMultiplier");
        this.runMultHash = Animator.StringToHash("RunMultiplier");
    }

    private void Start() {
        this.attackState = new EnemyStateAttack();
        this.attackState.Behaviour = this;
        this.wanderState = new EnemyStateWander();
        this.wanderState.Behaviour = this;
        this.runState = new EnemyStateRun();
        this.runState.Behaviour = this;
        this.chaseState = new EnemyStateChase();
        this.chaseState.Behaviour = this;
        this.TurnOffRagdoll();

        this.animator.SetFloat(this.walkMultHash, this.data.walkAnimationMult);
        this.animator.SetFloat(this.runMultHash, this.data.runAnimationMult);

        this.currentActionState = this.wanderState;
        this.currentActionState.Enter();
    }

    private void Update() {
        if (this.currentActionState != null && !this.isRagdoll)
            this.currentActionState.Updating();
    }

    public void AnimationTrigger() {
        if (this.currentActionState != null && !this.isRagdoll)
            this.currentActionState.AnimationEnd();
    }

    private void SetMoveAnimationState(int state) {
        this.animator.SetInteger(this.moveHash, state);
    }

    public void ChangeState(EnemyState state) {
        switch (state) {
            case EnemyState.Wander:
                this.currentActionState = this.wanderState;
                break;
            case EnemyState.Attack:
                this.currentActionState = this.attackState;
                break;
            case EnemyState.Run:
                this.currentActionState = this.runState;
                break;
            case EnemyState.Chase:
                this.currentActionState = this.chaseState;
                break;
            default:
                break;
        }
        
        this.currentActionState.Enter();
        this.currentState = state;
    }

    public float DistanceWithPlayer() {
        return (this.transform.position - GameManager.Instance.player.transform.position).magnitude;
    }

    public bool IsSeeingPlayer() {
        if (this.DistanceWithPlayer() > this.data.seekDistance) {
            return false;
        }

        this.seeingCheckHit = default;
        this.seeingCheckVector = this.transform.position;
        this.seeingCheckVector.y = 1;
        if (Physics.Raycast(this.seeingCheckVector, this.transform.forward, out this.seeingCheckHit, this.data.seekDistance, LayerMask.GetMask("Player", "Building"))) {
            if(!this.seeingCheckHit.transform || !this.seeingCheckHit.transform.CompareTag("Player"))
                return false;
        }

        Vector3 playerPosition = GameManager.Instance.player.transform.position;

        double angle = Math.Acos(Vector3.Dot(this.transform.forward, playerPosition - this.transform.position) / 
            (this.transform.forward.magnitude * (playerPosition - this.transform.position).magnitude)) * Mathf.Rad2Deg;

        if(angle < this.data.sightAngle) {
            return true;
        } else {
            return false;
        }
    }

    public bool CanReachToPlayer() {
        NavMeshPath path = new NavMeshPath();
        this.navAgent.CalculatePath(GameManager.Instance.player.transform.position, path);
        if (path.status == NavMeshPathStatus.PathPartial) {
            return false;
        }
        return true;
    }

    #region Ragdoll

    public void TurnOnRagdoll() {
        this.isRagdoll = true;
        this.animator.enabled = true;

        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.useGravity = false;

        this.navAgent.enabled = false;
        this.collider.enabled = false;
        this.mesh.updateWhenOffscreen = true;

        for (int i = 0; i < this.ragdollRigidbodies.Length; i++) {
            this.ragdollRigidbodies[i].useGravity = true;
        }

        for (int i = 0; i < this.ragdollColliders.Length; i++) {
            this.ragdollColliders[i].enabled = true;
        }

        this.animator.enabled = false;
    }

    public void EnemyDead() {
        this.currentActionState = null;
    }

    public void TurnOffRagdoll() {
        this.isRagdoll = false;

        this.animator.enabled = true;

        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.angularVelocity = Vector3.zero;
        this.rigidbody.useGravity = true;
        this.navAgent.enabled = true;
        this.collider.enabled = true;
        this.mesh.updateWhenOffscreen = false;

        Vector3 vector3 = this.ragdollStomach.transform.position;
        this.navAgent.Warp(vector3);
        this.navAgent.updateRotation = false;
        StartCoroutine(TurnOffRagdollMesh());
    }

    IEnumerator TurnOffRagdollMesh() {
        yield return null;
        this.navAgent.updateRotation = true;
        for (int i = 0; i < this.ragdollRigidbodies.Length; i++) {
            this.ragdollRigidbodies[i].velocity = Vector3.zero;
            this.ragdollRigidbodies[i].angularVelocity = Vector3.zero;
            this.ragdollRigidbodies[i].useGravity = false;
            this.ragdollRigidbodies[i].ResetInertiaTensor();

        }

        for (int i = 0; i < this.ragdollColliders.Length; i++) {
            this.ragdollColliders[i].enabled = false;
        }
    }

    #endregion
}
