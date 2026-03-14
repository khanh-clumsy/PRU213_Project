using UnityEngine;

public class AttackState : PlayerState
{
    protected AttackData data;
    private int frameCounter;
    private float frameTimer;
    private const float FRAME_TIME = 1f / 60f;
    private int comboIndex;
    private CombatController combat;
    private bool hasSpawnedProjectile = false;
    private bool hasTeleported = false;
    private bool attackCompleted = false;

    public AttackState(Player player, AttackData data, int index = 0) : base(player)
    {
        this.data = data;
        this.comboIndex = index;
        combat = player.GetComponent<CombatController>();
    }

    public override void Enter()
    {
        frameCounter = 0;
        frameTimer = 0f;
        hasSpawnedProjectile = false;
        hasTeleported = false;
        attackCompleted = false;

        player.Hitbox.ResetHitbox(); // ✅ clear hitTargets để đòn mới có thể hit lại

        player.Animator.speed = data.animationSpeed;
        player.Animator.Play(data.attackName);
        player.Movement.Stop();
    }

    public override void Update()
    {
        if (attackCompleted) return;

        // Frame counter chạy theo animationSpeed để khớp với animation
        frameTimer += Time.deltaTime * data.animationSpeed;
        while (frameTimer >= FRAME_TIME)
        {
            frameTimer -= FRAME_TIME;
            ProcessFrame();
        }

        // Chờ animation clip kết thúc thật sự thay vì đếm frame recovery
        CheckAnimationEnd();
    }

    private void ProcessFrame()
    {
        frameCounter++;

        HandleTeleport();
        HandleActiveFrames();
    }

    // Teleport đúng lúc hitbox bắt đầu bật (startupFrames), không phải frame 1
    private void HandleTeleport()
    {
        if (!data.isTeleport || hasTeleported) return;
        if (frameCounter != data.startupFrames) return;

        hasTeleported = true;
        TeleportToOpponent();
    }

    private void HandleActiveFrames()
    {
        bool inActiveWindow = frameCounter >= data.startupFrames
                           && frameCounter < data.startupFrames + data.activeFrames;

        if (!inActiveWindow) return;

        switch (data.type)
        {
            case AttackType.Melee:
                player.Hitbox.CheckHit(data);
                break;

            case AttackType.Projectile:
                if (!hasSpawnedProjectile)
                    SpawnProjectile();
                break;
        }
    }

    // Chờ animation clip chạy hết rồi mới CompleteAttack
    // Đảm bảo Loop Time = false trên clip trong Animator Controller
    private void CheckAnimationEnd()
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);

        bool isCurrentClip = stateInfo.IsName(data.attackName);
        bool isFinished = stateInfo.normalizedTime >= 1f && !player.Animator.IsInTransition(0);

        if (isCurrentClip && isFinished)
            CompleteAttack();
    }

    private void CompleteAttack()
    {
        if (attackCompleted) return;
        attackCompleted = true;

        AttackData nextAttack = combat.GetBufferedAttack();

        if (nextAttack != null && data.type == AttackType.Melee && comboIndex < combat.lightCombo.Count - 1)
        {
            int nextIndex = comboIndex + 1;
            player.StateMachine.ChangeState(new AttackState(player, combat.lightCombo[nextIndex], nextIndex));
        }
        else
        {
            combat.ClearBuffer();
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    private void TeleportToOpponent()
    {
        Player[] allPlayers = Object.FindObjectsOfType<Player>();
        Player target = null;

        foreach (var p in allPlayers)
        {
            if (p.playerID != player.playerID)
            {
                target = p;
                break;
            }
        }

        if (target != null)
        {
            float offset = target.transform.localScale.x > 0 ? -0.2f : 0.2f;
            Vector3 newPosition = target.transform.position + new Vector3(offset, 0, 0);
            player.transform.position = newPosition;

            float directionToTarget = target.transform.position.x > player.transform.position.x ? 1f : -1f;
            Vector3 localScale = player.transform.localScale;
            player.transform.localScale = new Vector3(Mathf.Abs(localScale.x) * directionToTarget, localScale.y, localScale.z);

            Physics2D.SyncTransforms();
        }
    }

    private void SpawnProjectile()
    {
        if (data.projectilePrefab == null) return;

        hasSpawnedProjectile = true;
        float facingDir = player.transform.localScale.x > 0 ? 1f : -1f;
        Vector3 spawnPos = player.transform.position + new Vector3(0, 0.7f, 0);

        GameObject obj = Object.Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
        obj.GetComponent<Projectile>()?.Setup(player, data, new Vector2(facingDir, 0), data.projectileSpeed);
    }

    public override void Exit()
    {
        player.Animator.speed = 1f;
    }
}