using UnityEngine;

public class AttackState : PlayerState
{
    private AttackData data;
    private int frameCounter;
    private float frameTimer;
    private const float FRAME_TIME = 1f / 60f;
    private int comboIndex;
    private CombatController combat;
    private bool hasSpawnedProjectile = false; // Để tránh bắn nhiều phi tiêu 1 lúc

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
        player.Animator.speed = data.animationSpeed;
        player.Animator.Play(data.attackName);
        player.Movement.Stop(); 
    }

    public override void Update()
    {
        frameTimer += Time.deltaTime;
        while (frameTimer >= FRAME_TIME)
        {
            frameTimer -= FRAME_TIME;
            ProcessFrame();
        }
    }

    private void ProcessFrame()
    {
        frameCounter++;

        // Xử lý dịch chuyển ở frame đầu tiên nếu là Teleport Attack
        if (frameCounter == 1 && data.isTeleport)
        {
            TeleportToOpponent();
            if (data.type == AttackType.Melee)
            {
                player.Hitbox.ResetHitbox();
                player.Hitbox.CheckHit(data);
            }
        }

        if (frameCounter == data.startupFrames)
        {
            if (data.type == AttackType.Projectile)
                SpawnProjectile();
            else
                player.Hitbox.ResetHitbox();
        }

        if (data.type == AttackType.Melee)
        {
            if (frameCounter >= data.startupFrames && frameCounter < data.startupFrames + data.activeFrames)
            {
                player.Hitbox.CheckHit(data);
            }
        }

        if (frameCounter >= data.startupFrames + data.activeFrames + data.recoveryFrames)
        {
            CompleteAttack();
        }
    }

    private void TeleportToOpponent()
    {
        // Tìm tất cả các Player trong Scene
        Player[] allPlayers = Object.FindObjectsOfType<Player>();
        Player target = null;

        foreach (var p in allPlayers)
        {
            // Tìm đối thủ (người không phải là bản thân dựa trên playerID)
            if (p.playerID != player.playerID)
            {
                target = p;
                break;
            }
        }

        if (target != null)
        {
            // 1. Xác định vị trí xuất hiện: 
            float offset = target.transform.localScale.x > 0 ? -0.7f : 0.7f;
            Vector3 newPosition = target.transform.position + new Vector3(offset, 0, 0);

            player.transform.position = newPosition;

            float directionToTarget = target.transform.position.x > player.transform.position.x ? 1f : -1f;
            Vector3 localScale = player.transform.localScale;
            player.transform.localScale = new Vector3(Mathf.Abs(localScale.x) * directionToTarget, localScale.y, localScale.z);

            Physics2D.SyncTransforms();

            Debug.Log($"Teleported to opponent at {newPosition}. Facing: {directionToTarget}");
        }
    }
    private void SpawnProjectile()
    {
        if (hasSpawnedProjectile || data.projectilePrefab == null) return;

        hasSpawnedProjectile = true;
        float facingDir = player.transform.localScale.x > 0 ? 1f : -1f;
        Vector2 dir = new Vector2(facingDir, 0);

        // Tạo một khoảng offset theo trục Y (ví dụ: cao hơn 0.5 đơn vị)
        float heightOffset = 0.3f;
        Vector3 spawnPosition = player.transform.position + new Vector3(0, heightOffset, 0);

        GameObject obj = Object.Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);

        var proj = obj.GetComponent<Projectile>();
        if (proj != null) proj.Setup(player, data, dir, data.projectileSpeed);
    }

    private void CompleteAttack()
    {
        AttackData nextAttack = combat.GetBufferedAttack(); 

        // Chỉ combo nếu là đòn đánh Melee và còn đòn tiếp theo
        if (nextAttack != null && data.type == AttackType.Melee && comboIndex < combat.lightCombo.Count - 1)
        {
            int nextIndex = comboIndex + 1;
            player.StateMachine.ChangeState(new AttackState(player, combat.lightCombo[nextIndex], nextIndex));
        }
        else
        {
            combat.ClearBuffer();
            player.StateMachine.ChangeState(player.IdleState); // Thoát về Idle
        }
    }

    public override void Exit()
    {
        player.Animator.speed = 1f; 
    }
}