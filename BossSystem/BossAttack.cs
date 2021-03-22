public enum AttackState
{
    Ready,
    Doing,
    Charging,
    CoolDown,
    Blocked
}

public class BossAttackEnv {
    public BossController boss;
}

public class BossAttack : IEquatable<BossAttack>
{
    public BossAttackType attackType;
    public float damage;
    public float coolDownTime;
    public float chargeTime;
    public Vector3 attackPosition;

    protected BossAttackEnv env;
    protected AttackState attackState;

    private float coolDownTimer;
    private float chargeTimer;

    protected Action stateAction;
    public Action<BossAttack> activeAttackAction;
    public Action disposeAttackAction;

    public virtual void Initialize(BossAttackEnv env) {
        this.env = env;
        this.attackState = AttackState.Ready;
        this.coolDownTimer = 0;
        this.chargeTimer = 0;
    }

    public virtual void Update() {
        switch (this.attackState) {
            case AttackState.Ready:
                break;
            case AttackState.Doing:
                break;
            case AttackState.Charging:
                if (this.chargeTimer < this.env.time.time) {
                    this.MakeAttack();
                }
                break;
            case AttackState.CoolDown:
                if(this.coolDownTimer < this.env.time.time) {
                    this.PutAttackReady();
                }
                break;
            case AttackState.Blocked:
                break;
            default:
                break;
        }
    }

    private void PutAttackReady() {
        this.attackState = AttackState.Ready;
        this.activeAttackAction?.Invoke(this);
    }

    public void Attack(Action stateAction) {
        this.stateAction = stateAction;
        this.ChargeAttack();
    }

    protected virtual void ChargeAttack() {
        this.attackState = AttackState.Charging;
        this.chargeTimer = this.chargeTime + this.env.time.time;
    }

    protected virtual void MakeAttack() {

    }

    //This.could be call from boss controller with animation/attack event
    public void FinishAttack() {
        this.disposeAttackAction?.Invoke();
        this.CoolDownAttack();
    }

    public virtual void CoolDownAttack() {
        this.attackState = AttackState.CoolDown;
        this.coolDownTimer = this.coolDownTime + this.env.time.time;
    }

    public bool Equals(BossAttack other) {
        if (other == null) {
            if (this == null) {
                return true;
            } else {
                return false;
            }
        }
        if (this.attackType == other.attackType) {
            return true;
        }

        return false;
    }
}
