using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Timer")]
    public TMP_Text timerText;

    [Header("Player 1 UI")]
    public Image p1HealthFill;
    public Image p1EnergyFill;

    [Header("Player 2 UI")]
    public Image p2HealthFill;
    public Image p2EnergyFill;

    [Header("Max Values")]
    public float maxHP = 100f;
    public float maxEnergy = 100f;

    private void OnEnable()
    {
        GameEvents.OnHealthChanged += UpdateHealthUI;
        GameEvents.OnManaChanged += UpdateEnergyUI;
        GameEvents.OnTimerTick += UpdateTimerUI;
        GameEvents.OnTimeOut += HandleTimeOut;
    }

    private void OnDisable()
    {
        GameEvents.OnHealthChanged -= UpdateHealthUI;
        GameEvents.OnManaChanged -= UpdateEnergyUI;
        GameEvents.OnTimerTick -= UpdateTimerUI;
        GameEvents.OnTimeOut -= HandleTimeOut;
    }

    private void Start()
    {
        // Xóa dòng gán tĩnh p1HealthFill.fillAmount = 1f; để không ghi đè lên giá trị chính xác lúc load ván
        if (timerText != null)
            timerText.text = "01:00";
    }

    private void UpdateHealthUI(int playerID, int currentHP)
    {
        float pMaxHP = maxHP; // Lấy mặc định nếu ko có GameManager
        if (GameManager.Instance != null && GameManager.Instance.GetPlayer(playerID) != null)
        {
            pMaxHP = GameManager.Instance.GetPlayer(playerID).maxHP; // Dùng maxHP thực tế của nhân vật
        }

        float percent = Mathf.Clamp01(currentHP / pMaxHP);

        if (playerID == 1 && p1HealthFill != null)
            p1HealthFill.fillAmount = percent;
        else if (playerID == 2 && p2HealthFill != null)
            p2HealthFill.fillAmount = percent;
    }

    private void UpdateEnergyUI(int playerID, int currentEnergy)
    {
        float pMaxEnergy = maxEnergy;
        if (GameManager.Instance != null && GameManager.Instance.GetPlayer(playerID) != null)
        {
            pMaxEnergy = GameManager.Instance.GetPlayer(playerID).maxMana; // Dùng maxMana thực tế
        }

        float percent = Mathf.Clamp01(currentEnergy / pMaxEnergy);

        if (playerID == 1 && p1EnergyFill != null)
            p1EnergyFill.fillAmount = percent;
        else if (playerID == 2 && p2EnergyFill != null)
            p2EnergyFill.fillAmount = percent;
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText == null) return;

        if (timeRemaining < 0) timeRemaining = 0;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void HandleTimeOut()
    {
        if (timerText != null)
            timerText.text = "00:00";
    }
}