using UnityEngine;
using TMPro;

public class CharacterTagUI : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public int characterID;      // 0..4
        public TMP_Text tagText;     // Button/TagText
    }

    public Slot[] slots;

    public void ClearTag(string tag)
    {
        foreach (var s in slots)
        {
            if (s.tagText != null && s.tagText.text == tag)
                s.tagText.text = "";
        }
    }

    public void SetTag(int characterID, string tag)
    {
        foreach (var s in slots)
        {
            if (s.characterID == characterID && s.tagText != null)
            {
                s.tagText.text = tag;  // "P1" or "P2"
                return;
            }
        }
    }

    public void ClearAll()
    {
        foreach (var s in slots)
        {
            if (s.tagText != null)
                s.tagText.text = "";
        }
    }
}