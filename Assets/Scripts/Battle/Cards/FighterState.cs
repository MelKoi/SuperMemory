using System.Collections.Generic;

public sealed class WeaponLoadout
{
    public WeaponLoadout(int weaponId, string ownerKey, WeaponChargeTier light, WeaponChargeTier heavy)
    {
        WeaponId = weaponId;
        OwnerKey = ownerKey ?? string.Empty;
        Light = light;
        Heavy = heavy;
    }

    public int WeaponId { get; }
    public string OwnerKey { get; }
    public WeaponChargeTier Light { get; }
    public WeaponChargeTier Heavy { get; }
    public int Charge { get; set; }
}

// 一方的资源、牌区和滞留。滞留留到下一次自己的武器攻击。
public sealed class FighterState
{
    readonly HashSet<int> statuses = new HashSet<int>();

    public FighterState(BattleSide side, string characterKey, int handLimit)
    {
        Side = side;
        CharacterKey = characterKey ?? string.Empty;
        HandLimit = handLimit;
        Health = MaxHealth;
        Toughness = MaxToughness;
    }

    public const int MaxHealth = 20;
    public const int MaxToughness = 20;

    public BattleSide Side { get; }
    public string CharacterKey { get; }
    public int HandLimit { get; }
    public int Health { get; private set; }
    public int Toughness { get; private set; }
    public int Energy { get; set; }
    public List<CardInstance> Deck { get; } = new List<CardInstance>();
    public List<CardInstance> Hand { get; } = new List<CardInstance>();
    public List<CardInstance> Discard { get; } = new List<CardInstance>();
    public List<CardInstance> Field { get; } = new List<CardInstance>();
    public List<CardInstance> Removed { get; } = new List<CardInstance>();
    public List<WeaponLoadout> Weapons { get; } = new List<WeaponLoadout>();
    public List<int[]> Linger { get; } = new List<int[]>();
    public int NextCostDelta { get; set; }

    public bool HasStatus(int statusId)
    {
        return statuses.Contains(statusId);
    }

    public void AddStatus(int statusId)
    {
        statuses.Add(statusId);
    }

    public bool RemoveStatus(int statusId)
    {
        return statuses.Remove(statusId);
    }

    public void SetHealth(int value)
    {
        Health = Clamp(value, 0, MaxHealth);
    }

    public void SetToughness(int value)
    {
        Toughness = Clamp(value, 0, MaxToughness);
    }

    public bool Owns(string owner)
    {
        if (string.IsNullOrEmpty(owner))
        {
            return true;
        }

        if (owner == CharacterKey)
        {
            return true;
        }

        for (var i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i].OwnerKey == owner)
            {
                return true;
            }
        }

        return false;
    }

    static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}
