using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleArea : MonoBehaviour
{
    private BattleSide side;
    private BattlePortraitView portrait;
    private BattleVitalBar healthBar;
    private BattleVitalBar toughnessBar;
    private BattleVitalBar energyBar;
    private BattleWeaponView weapon1;
    private BattleWeaponView weapon2;
    private HandZone hand;
    private Text deckCount;
    private BattleVitals vitals = new BattleVitals();

    public BattleSide Side => side;
    public BattleVitals Vitals => vitals ??= new BattleVitals();
    public HandZone Hand => hand;
    public bool HasCharacter { get; private set; }
    public bool HasWeapon1 { get; private set; }
    public bool HasWeapon2 { get; private set; }
    public string CharacterName { get; private set; }
    public string WeaponName1 { get; private set; }
    public string WeaponName2 { get; private set; }
    public int DeckCount { get; private set; }

    public void Initialize(
        BattleSide owner,
        BattlePortraitView portraitView,
        BattleVitalBar health,
        BattleVitalBar toughness,
        BattleVitalBar energy,
        BattleWeaponView firstWeapon,
        BattleWeaponView secondWeapon,
        HandZone handZone,
        Text deckLabel)
    {
        side = owner;
        portrait = portraitView;
        healthBar = health;
        toughnessBar = toughness;
        energyBar = energy;
        weapon1 = firstWeapon;
        weapon2 = secondWeapon;
        hand = handZone;
        deckCount = deckLabel;
        vitals = new BattleVitals();
        if (hand != null)
        {
            hand.Initialize(owner);
        }

        SetDeckCount(0);
    }

    public void LoadOpeningState(int characterId, int weaponId1, int weaponId2, Sprite portraitSprite, Sprite weaponSprite1, Sprite weaponSprite2)
    {
        Vitals.ResetToOpening();
        if (BattleTableReader.TryReadCharacter(characterId, out var characterName, out var health, out var toughness))
        {
            HasCharacter = true;
            CharacterName = characterName;
            Vitals.SetHealth(health);
            Vitals.SetToughness(toughness);
            if (portrait != null)
            {
                portrait.SetPortrait(portraitSprite);
            }
        }
        else
        {
            HasCharacter = false;
            CharacterName = null;
            if (portrait != null)
            {
                portrait.SetPortrait(null);
            }
        }

        HasWeapon1 = PresentWeapon(weapon1, weaponId1, weaponSprite1, out var firstName);
        HasWeapon2 = PresentWeapon(weapon2, weaponId2, weaponSprite2, out var secondName);
        WeaponName1 = firstName;
        WeaponName2 = secondName;
        SetDeckCount(0);
        RefreshBars();
    }

    public void SetDeckCount(int count)
    {
        DeckCount = Mathf.Max(0, count);
        if (deckCount != null)
        {
            deckCount.text = "牌库：" + DeckCount.ToString(CultureInfo.InvariantCulture);
        }
    }

    public void SetWeaponCharge(int weaponNumber, int charge)
    {
        if (weaponNumber == 1)
        {
            if (weapon1 != null)
            {
                weapon1.SetCharge(charge);
            }
        }
        else if (weaponNumber == 2 && weapon2 != null)
        {
            weapon2.SetCharge(charge);
        }
    }

    public void SetHealth(int value)
    {
        Vitals.SetHealth(value);
        if (healthBar != null)
        {
            healthBar.SetValue(Vitals.Health, BattleVitals.MaxHealth);
        }
    }

    public void SetToughness(int value)
    {
        Vitals.SetToughness(value);
        if (toughnessBar != null)
        {
            toughnessBar.SetValue(Vitals.Toughness, BattleVitals.MaxToughness);
        }
    }

    public void SetEnergy(int value)
    {
        Vitals.SetEnergy(value);
        if (energyBar != null)
        {
            energyBar.SetValue(Vitals.Energy, BattleVitals.EnergyCapacity);
        }
    }

    public BattleWeaponView GetWeapon(int weaponNumber)
    {
        if (weaponNumber == 1)
        {
            return weapon1;
        }

        if (weaponNumber == 2)
        {
            return weapon2;
        }

        return null;
    }

    public bool TrySpendEnergy(int cost)
    {
        if (cost < 0 || Vitals.Energy < cost)
        {
            return false;
        }

        SetEnergy(Vitals.Energy - cost);
        return true;
    }

    public void ApplyDamage(int lifeDamage, int toughnessDamage)
    {
        SetHealth(Vitals.Health - Mathf.Max(0, lifeDamage));
        SetToughness(Vitals.Toughness - Mathf.Max(0, toughnessDamage));
    }

    private void RefreshBars()
    {
        if (healthBar != null)
        {
            healthBar.SetValue(Vitals.Health, BattleVitals.MaxHealth);
        }

        if (toughnessBar != null)
        {
            toughnessBar.SetValue(Vitals.Toughness, BattleVitals.MaxToughness);
        }

        if (energyBar != null)
        {
            energyBar.SetValue(Vitals.Energy, BattleVitals.EnergyCapacity);
        }
    }

    private static bool PresentWeapon(BattleWeaponView view, int weaponId, Sprite sprite, out string weaponName)
    {
        if (view == null)
        {
            weaponName = null;
            return false;
        }

        if (!BattleTableReader.TryReadWeapon(weaponId, out weaponName, out var light, out var heavy))
        {
            view.ClearWeapon();
            return false;
        }

        view.Present(sprite, light, heavy);
        return true;
    }
}
