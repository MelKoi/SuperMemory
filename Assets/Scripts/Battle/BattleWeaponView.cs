using UnityEngine;
using UnityEngine.UI;

public sealed class BattleWeaponView : MonoBehaviour
{
    private Image plate;

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
}
