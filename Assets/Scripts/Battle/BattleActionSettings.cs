using UnityEngine;

// 行动面板用的数字。改 Inspector 里的值会立刻作用到当前场面。
public sealed class BattleActionSettings : MonoBehaviour
{
    [Header("移动")]
    [Tooltip("前进一步消耗的能量。")]
    [SerializeField, Min(0)] private int forwardEnergyCost = 2;
    [Tooltip("后退一步消耗的能量。")]
    [SerializeField, Min(0)] private int backwardEnergyCost = 2;
    [Tooltip("前进的格数。")]
    [SerializeField, Min(0)] private int forwardCells = 1;
    [Tooltip("后退的格数。")]
    [SerializeField, Min(0)] private int backwardCells = 1;

    [Header("我方资源")]
    [Tooltip("我方当前能量。行动会改这个数，在这里改也会立刻更新能量环。")]
    [SerializeField, Min(0)] private int playerEnergy = 10;
    [Tooltip("武器 1 的当前蓄能。没达到攻击门槛时不能攻击，攻击后清零。")]
    [SerializeField, Min(0)] private int weaponCharge1;
    [Tooltip("武器 2 的当前蓄能。没达到攻击门槛时不能攻击，攻击后清零。")]
    [SerializeField, Min(0)] private int weaponCharge2;

    public int ForwardEnergyCost => Mathf.Max(0, forwardEnergyCost);
    public int BackwardEnergyCost => Mathf.Max(0, backwardEnergyCost);
    public int ForwardCells => Mathf.Max(0, forwardCells);
    public int BackwardCells => Mathf.Max(0, backwardCells);

    public void ApplyTo(BattleArea area)
    {
        if (area == null)
        {
            return;
        }

        area.SetEnergy(Mathf.Clamp(playerEnergy, 0, BattleVitals.EnergyCapacity));
        if (area.HasWeapon1)
        {
            area.SetWeaponCharge(1, Mathf.Max(0, weaponCharge1));
        }

        if (area.HasWeapon2)
        {
            area.SetWeaponCharge(2, Mathf.Max(0, weaponCharge2));
        }
    }

    public void AdoptEnergy(int value)
    {
        playerEnergy = Mathf.Clamp(value, 0, BattleVitals.EnergyCapacity);
    }

    public void AdoptCharge(int weaponNumber, int value)
    {
        value = Mathf.Max(0, value);
        if (weaponNumber == 1)
        {
            weaponCharge1 = value;
        }
        else if (weaponNumber == 2)
        {
            weaponCharge2 = value;
        }
    }

    private void OnValidate()
    {
        if (forwardEnergyCost < 0)
        {
            forwardEnergyCost = 0;
        }

        if (backwardEnergyCost < 0)
        {
            backwardEnergyCost = 0;
        }

        if (forwardCells < 0)
        {
            forwardCells = 0;
        }

        if (backwardCells < 0)
        {
            backwardCells = 0;
        }

        if (weaponCharge1 < 0)
        {
            weaponCharge1 = 0;
        }

        if (weaponCharge2 < 0)
        {
            weaponCharge2 = 0;
        }

        playerEnergy = Mathf.Clamp(playerEnergy, 0, BattleVitals.EnergyCapacity);

        var hud = GetComponent<BattleHud>();
        if (hud != null)
        {
            ApplyTo(hud.Player);
        }

        var menu = GetComponentInChildren<BattleActionMenu>(true);
        if (menu != null)
        {
            menu.Refresh();
        }
    }
}
