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
                float oldHeight = box.size.y;
                float oldWidth = box.size.x;
                float oldOffsetY = box.offset.y;

                // TÌM CÔNG THỨC TUYỆT ĐỐI CHO MỌI PREFAB (Bất chấp to, nhỏ, Pivot ở chân hay ngực):
                float oldBottomY = oldOffsetY - (oldHeight / 2f); // Tâm gót chân cũ
                float aspectScale = oldWidth / oldHeight; // Tỷ lệ vàng của nhân vật
                float visualBottom = oldBottomY * aspectScale; // Cú "hack" nội suy tìm ra đúng điểm lưng chạm đất lúc nằm ngang

                float newHeight = 0.05f; // Mỏng dính (Rất rất nhỏ)
                float newWidth = oldHeight * 0.8f; // Bè ngang rộng ra lót qua mép Platform để không bị trượt rớt

                float newOffsetY = visualBottom + (newHeight / 2f);

                box.size = new UnityEngine.Vector2(newWidth, newHeight);
                box.offset = new UnityEngine.Vector2(0f, newOffsetY);
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
