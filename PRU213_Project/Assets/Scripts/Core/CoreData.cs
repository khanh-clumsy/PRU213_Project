using UnityEngine;
using System.Collections.Generic; // Bắt buộc phải có dòng này để dùng List

// 1. Khai báo Enum các loại chỉ số (Có thể thêm Attack, AttackSpeed...)
public enum CoreType
{
    MaxHP,
    MoveSpeed,
    Attack,
    Mana
}

// 2. Tạo một Struct để gom nhóm Loại chỉ số và Giá trị đi kèm với nhau
[System.Serializable]
public class StatModifier
{
    public CoreType type;  // Loại chỉ số bị tác động
    public float value;    // Giá trị (+ là tăng, - là giảm, hoặc % tùy bạn quy định)
}

[CreateAssetMenu(fileName = "NewCore", menuName = "Combat/Core Data")]
public class CoreData : ScriptableObject
{
    public string coreName;
    [TextArea] public string description;
    public Sprite icon;

    // 3. Thay thế 2 biến cũ bằng một Danh sách (List)
    [Header("Stat Changes")]
    public List<StatModifier> modifiers = new List<StatModifier>();
}