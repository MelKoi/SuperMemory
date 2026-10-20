using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleWeaponView : MonoBehaviour
{
    private Image plate;
    private Text chargeText;
    private Text damageText;
    private WeaponChargeTier light;
    private WeaponChargeTier heavy;
    private int charge;

    public int Charge => charge;

    public void Initialize()
    {
        var frame = gameObject.AddComponent<Image>();
        frame.sprite = BattleUiSprites.Square;
        frame.color = new Color(0.78f, 0.78f, 0.8f, 1f);
        frame.raycastTarget = false;

        var plateRect = BattleUiFactory.Create("Plate", transform);
        BattleUiFactory.Stretch(plateRect, Vector2.zero, Vector2.one);
        plateRect.offsetMin = new Vector2(4f, 4f);
        plateRect.offsetMax = new Vector2(-4f, -4f);
        plate = plateRect.gameObject.AddComponent<Image>();
        plate.raycastTarget = false;
        ShowPlaceholder();
    }

    public void SetReadout(Text chargeLabel, Text damageLabel)
    {
        chargeText = chargeLabel;
        damageText = damageLabel;
        RefreshReadout();
    }

    public void Present(Sprite sprite, WeaponChargeTier lightTier, WeaponChargeTier heavyTier)
    {
        light = lightTier;
        heavy = heavyTier;
        charge = 0;
        SetIcon(sprite);
        RefreshReadout();
    }

    public void ClearWeapon()
    {
        light = default;
        heavy = default;
        charge = 0;
        SetIcon(null);
        RefreshReadout();
    }

    public void SetCharge(int value)
    {
        charge = Mathf.Max(0, value);
        RefreshReadout();
    }

    public void SetIcon(Sprite sprite)
    {
        if (plate == null)
        {
            return;
        }

        if (sprite == null)
        {
            ShowPlaceholder();
            return;
        }

        plate.sprite = sprite;
        plate.color = Color.white;
        plate.preserveAspect = true;
    }

    private void ShowPlaceholder()
    {
        plate.sprite = BattleUiSprites.Square;
        plate.color = Color.white;
        plate.preserveAspect = false;
    }

    private void RefreshReadout()
    {
        if (chargeText != null)
        {
            chargeText.text = "蓄能数 " + charge.ToString(CultureInfo.InvariantCulture);
        }

        if (damageText == null)
        {
            return;
        }

        WeaponChargeTier.Resolve(charge, light, heavy, out var lifeDamage, out var toughnessDamage);
        damageText.text = "伤害 " + lifeDamage.ToString(CultureInfo.InvariantCulture) + "/" + toughnessDamage.ToString(CultureInfo.InvariantCulture);
    }
}
