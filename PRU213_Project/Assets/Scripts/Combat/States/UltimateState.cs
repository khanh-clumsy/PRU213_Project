using UnityEngine;
using System.Collections;

public class UltimateState : AttackState
{
    private float ultimateDuration = 3.0f;
    private Player target;
    private bool hasDealtDamage = false;
    private GameObject currentUltimateVfx;

    public UltimateState(Player player, AttackData data) : base(player, data) { }

    public override void Enter()
    {
        if (player.CurrentMana < 100)
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        target = FindTarget();
        if (target == null)
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        player.UseMana(100);
        hasDealtDamage = false;

        TeleportToEnemy();
        FreezeTarget();
        base.Enter();

        Time.timeScale = 0.05f;
        player.StartCoroutine(HandleUltimateSequence());
    }

    private Player FindTarget()
    {
        foreach (var p in Object.FindObjectsOfType<Player>())
            if (p.playerID != player.playerID) return p;
        return null;
    }

    private void TeleportToEnemy()
    {
        float offset = (target.transform.position.x > player.transform.position.x) ? -0.5f : 0.5f;
        Vector3 newPos = new Vector3(
            target.transform.position.x + offset,
            player.transform.position.y,  // Sửa lại thành Y của chính người tung chiêu thay vì Y của đối thủ
            player.transform.position.z
        );
        player.transform.position = newPos;

        float direction = target.transform.position.x - player.transform.position.x;
        Vector3 scale = player.transform.localScale;
        player.transform.localScale = new Vector3(
            Mathf.Abs(scale.x) * (direction >= 0 ? 1f : -1f),
            scale.y,
            scale.z
        );

        Physics2D.SyncTransforms();
    }

    private void FreezeTarget()
    {
        if (target != null)
        {
            target.DisableAllActions();
        }
    }

    private void UnfreezeTarget()
    {
        if (target != null)
        {
            target.EnableAllActions();
        }
    }

    private IEnumerator HandleUltimateSequence()
    {
        // Phase 1: Freeze màn hình
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        // Phase 2: Chờ đến giữa animation rồi gây damage 1 lần
        float halfDuration = ultimateDuration * 0.2f;
        yield return new WaitForSeconds(halfDuration);

        DealDamage();

        // Phase 3: Chờ nửa còn lại rồi kết thúc
        yield return new WaitForSeconds(ultimateDuration - halfDuration);

        player.StateMachine.ChangeState(player.IdleState);
    }

    private void DealDamage()
    {
        if (hasDealtDamage || target == null) return;
        hasDealtDamage = true;

        if (player.ultimateEffectPrefab != null)
        {
            // Spawn hiệu ứng ngay tại vị trí đối thủ, dời trục Z lên đằng trước
            Vector3 spawnPos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z - 1f);
            currentUltimateVfx = Object.Instantiate(player.ultimateEffectPrefab, spawnPos, Quaternion.identity);
        }

        player.Hitbox.ResetHitbox();
        player.Hitbox.CheckHit(data);
    }

    public override void Exit()
    {
        base.Exit();
        Time.timeScale = 1.0f;
        UnfreezeTarget();

        // Huỷ khối hình hiệu ứng ngay lập tức khi thoát chiêu (nếu vẫn còn)
        if (currentUltimateVfx != null)
        {
            Object.Destroy(currentUltimateVfx);
        }
    }
}