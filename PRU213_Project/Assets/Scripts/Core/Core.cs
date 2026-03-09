using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Core : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    public Image cardBackground;
    public Outline cardOutline;

    private CoreData data;
    private CoreUIHandler handler;

    public void Setup(CoreData coreData, CoreUIHandler uiHandler)
    {
        data = coreData;
        handler = uiHandler;

        // 1. Gán tên và ảnh
        titleText.text = data.coreName;
        iconImage.sprite = data.icon;

        // 2. Tự động xây dựng nội dung mô tả kèm màu sắc chỉ số
        // Chúng ta dùng Rich Text của TextMeshPro để đổi màu
        string finalDescription = data.description + "\n";

        foreach (var mod in data.modifiers)
        {
            // Nếu giá trị dương (+) thì màu xanh lá, âm (-) thì màu đỏ rực
            string colorTag = mod.value >= 0 ? "<color=#00FF00>" : "<color=#FF4444>";
            string sign = mod.value >= 0 ? "+" : ""; // Hiện dấu + nếu là số dương

            // Ví dụ: <color=#00FF00>+50 Attack</color>
            finalDescription += $"{colorTag}{sign}{mod.value} {mod.type}</color>\n";
        }

        descText.text = finalDescription;
    }

    public void OnSelect()
    {
        // Vẫn giữ logic cũ để truyền dữ liệu về Manager
        handler.OnCoreSelected(this, data);
    }
}