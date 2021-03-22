
public class BossController : MonoBehaviour, IDamagable
{
    private EnemyStates currentState;

    //ATTACK
    private List<BossAttack> availableAttacks = new List<BossAttack>();
    private List<BossAttack> poolAttacks = new List<BossAttack>();
    private BossAttack currentAttack;
    private bool canAttack;
    private bool isInTheMiddleOfAttack;
    private float attackCooldownTimer;
    private Transform player;
    private bool needToTakeAttack;

    private void Init() {
        this.isAlreadyInit = true;
        this.canAttack = true;

        this.availableAttacks.Add(BossAttacksDataBase.GetBossAttackByType(BossAttackType.Pecueca));
        this.availableAttacks.Add(BossAttacksDataBase.GetBossAttackByType(BossAttackType.Tongue));

        for (int i = 0; i < this.availableAttacks.Count; i++) {
            this.availableAttacks[i].activeAttackAction += this.RecoverAttack;
            this.availableAttacks[i].disposeAttackAction += this.DisposeAttack;
            this.availableAttacks[i].Initialize(new BossAttackEnv {
                boss = this,
                time = this.time
            });
        }

        this.TakeAttack();
    }

    private void UpdateAI() {

        if (this.currentState == EnemyStates.Dead)
            return;

        if(this.currentAttack != null) {
            this.currentAttack.Update();
        }
        this.UpdateRotation();
        this.PoolAttacksUpdate();
        if (this.currentState == EnemyStates.Attacking) {
            if (!this.PlayerIsInAngle()) {
                this.StartMove();
                return;
            }

            if (this.canAttack) {
                if (this.attackCooldownTimer < this.time.time) {
                    this.Attack();
                    return;
                }
            }
            this.StartMove();
        }else if...

    }

    private void StartAttack() {
        this.currentState = EnemyStates.Attacking;
        this.canAttack = true;
    }


    private void PoolAttacksUpdate() {
        for (int i = 0; i < this.poolAttacks.Count; i++) {
            this.poolAttacks[i].Update();
        }
    }

    private bool IsAnyAttackAvailable() {
        return this.availableAttacks.Count > 0;
    }

    private void Attack() {
        if (this.currentAttack == null)
            return;

        this.canAttack = false;
        this.isInTheMiddleOfAttack = true;

        this.currentAttack.attackPosition = GameManager.Instance.PlayerGO.transform.position;
        this.currentAttack.Attack(() => {
            this.isInTheMiddleOfAttack = false;
            this.attackCooldownTimer = this.attackCooldown + this.time.time;
        });
    }

    private void DisposeAttack() {
        this.canAttack = true;
        this.poolAttacks.Add(this.currentAttack);
        this.currentAttack = null;
        this.TakeAttack(); 
    }

    private void TakeAttack() {
        if(this.IsAnyAttackAvailable()) {
            int randomIndex = this.availableAttacks.GetRandomEntryIndex();
            this.currentAttack = this.availableAttacks[randomIndex];
            this.availableAttacks.RemoveAt(randomIndex);
        } else {
            this.needToTakeAttack = true;
        }
    }

    private void RecoverAttack(BossAttack attack) {
        Debug.LogError("Attack came back");
        int attackIndex = this.poolAttacks.FindIndex(x => x == attack);
        this.availableAttacks.Add(this.poolAttacks[attackIndex]);
        this.poolAttacks.RemoveAt(attackIndex);

        if (this.needToTakeAttack) {
            this.needToTakeAttack = false;
            this.TakeAttack();
        }
    }
}
