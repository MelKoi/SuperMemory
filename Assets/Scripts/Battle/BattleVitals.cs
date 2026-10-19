using UnityEngine;

public sealed class BattleVitals
{
    public const int MaxHealth = 20;
    public const int MaxToughness = 20;
    public const int OpeningHealth = 20;
    public const int OpeningToughness = 20;
    public const int OpeningEnergy = 0;
    public const int EnergyCapacity = 10;

    public int Health { get; private set; } = OpeningHealth;
    public int Toughness { get; private set; } = OpeningToughness;
    public int Energy { get; private set; } = OpeningEnergy;

    public void ResetToOpening()
    {
        Health = OpeningHealth;
        Toughness = OpeningToughness;
        Energy = OpeningEnergy;
    }

    public void SetHealth(int value)
    {
        Health = Mathf.Clamp(value, 0, MaxHealth);
    }

    public void SetToughness(int value)
    {
        Toughness = Mathf.Clamp(value, 0, MaxToughness);
    }

    public void SetEnergy(int value)
    {
        Energy = Mathf.Clamp(value, 0, EnergyCapacity);
    }
}
