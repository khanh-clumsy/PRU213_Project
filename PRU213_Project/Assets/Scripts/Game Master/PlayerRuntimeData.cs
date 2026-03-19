/// <summary>
/// Serializable data class containing player stats that persist across scenes
/// Used to save and restore player state between rounds
/// </summary>
[System.Serializable]
public class PlayerRuntimeData
{
    public int maxHP;
    public int currentHP;
    public int maxMana;
    public int currentMana;
    public int attackDamage;
    public float moveSpeed;

    public PlayerRuntimeData(int maxHP, int currentHP, int maxMana, int currentMana, int attackDamage, float moveSpeed)
    {
        this.maxHP = maxHP;
        this.currentHP = currentHP;
        this.maxMana = maxMana;
        this.currentMana = currentMana;
        this.attackDamage = attackDamage;
        this.moveSpeed = moveSpeed;
    }
}
