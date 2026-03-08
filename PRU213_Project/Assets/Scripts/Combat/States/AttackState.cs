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

        if (frameCounter == data.startupFrames)
        {
            if (data.type == AttackType.Projectile)
            {
                Debug.Log("Spawning projectile at frame: " + frameCounter);
                SpawnProjectile(); // Bắn phi tiêu khi hết startup
            }
            else
            {
                player.Hitbox.ResetHitbox(); // Reset cận chiến
            }
        }

        if (data.type == AttackType.Melee)
        {
            if (frameCounter >= data.startupFrames &&
                frameCounter < data.startupFrames + data.activeFrames)
            {
                player.Hitbox.CheckHit(data); 
            }
        }

       
        if (frameCounter >= data.startupFrames + data.activeFrames + data.recoveryFrames)
        {
            CompleteAttack();
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