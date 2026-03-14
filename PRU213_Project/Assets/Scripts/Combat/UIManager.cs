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
        if (p1HealthFill != null) p1HealthFill.fillAmount = 1f;
        if (p2HealthFill != null) p2HealthFill.fillAmount = 1f;

        if (p1EnergyFill != null) p1EnergyFill.fillAmount = 0f;
        if (p2EnergyFill != null) p2EnergyFill.fillAmount = 0f;

        if (timerText != null)
            timerText.text = "02:00";
    }

    private void UpdateHealthUI(int playerID, int currentHP)
    {
        float percent = Mathf.Clamp01(currentHP / maxHP);

        if (playerID == 1)
        {
            if (p1HealthFill != null)
                p1HealthFill.fillAmount = percent;
        }
        else if (playerID == 2)
        {
            if (p2HealthFill != null)
                p2HealthFill.fillAmount = percent;
        }
    }

    private void UpdateEnergyUI(int playerID, int currentEnergy)
    {
        float percent = Mathf.Clamp01(currentEnergy / maxEnergy);

        if (playerID == 1)
        {
            if (p1EnergyFill != null)
                p1EnergyFill.fillAmount = percent;
        }
        else if (playerID == 2)
        {
            if (p2EnergyFill != null)
                p2EnergyFill.fillAmount = percent;
        }
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