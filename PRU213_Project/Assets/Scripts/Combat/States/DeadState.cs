using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Combat;

namespace Assets.Scripts.Combat.States
{
    public class DeadState : PlayerState
    {
        public DeadState(Player player) : base(player)
        {
        }

        public override void Enter()
        {
            base.Enter();
            player.Animator.Play("Dead");
            player.DisableAllActions();

            // --- TỐI ƯU CODE FIX LỖI "LƠ LỬNG" ---

            // 1. Tắt ngay lập tức Hitbox và Hurtbox (con)
            UnityEngine.Transform hitbox = player.transform.Find("Hitbox");
            if (hitbox != null) hitbox.gameObject.SetActive(false);

            UnityEngine.Transform hurtbox = player.transform.Find("Hurtbox");
            if (hurtbox != null) hurtbox.gameObject.SetActive(false);

            // 2. Bóp lùn TẤT CẢ các BoxCollider2D trên root
            UnityEngine.BoxCollider2D[] rootBoxes = player.GetComponents<UnityEngine.BoxCollider2D>();
            foreach (var box in rootBoxes)
            {
                // SOLUTION TỐI THƯỢNG: 
                // Biến cái Collider thành một "tấm ván" siêu dẹt và bè ngang!
                // Rộng 1.5f: Đảm bảo mắc cạn vắt ngang qua mỏm Platform, tuyệt đối không bị trượt rớt mép!
                // Cao 0.05f: Cực kỳ cực kỳ nhỏ (mỏng dính), sát rạt mặt đất!
                box.size = new UnityEngine.Vector2(1.5f, 0.05f);

                // Hạ offset Y xuống -0.25f (hoặc -0.2f) để thân hình rớt chạm đúng tấm mỏng đó
                box.offset = new UnityEngine.Vector2(0f, -0.25f);
            }

            // Gói bảo hiểm chống lọt sàn: 
            // Nếu sàn Ground quá mỏng, vận tốc rơi có thể làm xuyên thấu. Bật Continuous sẽ chặn đứng lỗi này!
            if (player.Rigidbody != null)
            {
                player.Rigidbody.collisionDetectionMode = UnityEngine.CollisionDetectionMode2D.Continuous;
            }
        }

        public override void Update()
        {
            base.Update();
            // Cưỡng chế tắt Hurtbox và Hitbox mỗi frame, đề phòng Animator của animation "Dead" vô tình bật lại!
            UnityEngine.Transform hitbox = player.transform.Find("Hitbox");
            if (hitbox != null) hitbox.gameObject.SetActive(false);

            UnityEngine.Transform hurtbox = player.transform.Find("Hurtbox");
            if (hurtbox != null) hurtbox.gameObject.SetActive(false);
        }
    }
}
