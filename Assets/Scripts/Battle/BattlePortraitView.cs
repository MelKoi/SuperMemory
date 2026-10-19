using UnityEngine;
using UnityEngine.UI;

public sealed class BattlePortraitView : MonoBehaviour
{
    private Image face;

    public void Initialize()
    {
        var rim = BattleUiFactory.Create("Rim", transform);
        BattleUiFactory.Stretch(rim, Vector2.zero, Vector2.one);
        var rimImage = rim.gameObject.AddComponent<Image>();
        rimImage.sprite = BattleUiSprites.Circle;
        rimImage.color = new Color(0.18f, 0.36f, 0.72f, 1f);
        rimImage.raycastTarget = false;

        var maskRect = BattleUiFactory.Create("FaceMask", transform);
        BattleUiFactory.Stretch(maskRect, Vector2.zero, Vector2.one);
        maskRect.offsetMin = new Vector2(12f, 12f);
        maskRect.offsetMax = new Vector2(-12f, -12f);
        var maskImage = maskRect.gameObject.AddComponent<Image>();
        maskImage.sprite = BattleUiSprites.Circle;
        maskImage.color = Color.white;
        maskImage.raycastTarget = false;
        var mask = maskRect.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        var faceRect = BattleUiFactory.Create("Face", maskRect);
        BattleUiFactory.Stretch(faceRect, Vector2.zero, Vector2.one);
        face = faceRect.gameObject.AddComponent<Image>();
        face.raycastTarget = false;
        face.preserveAspect = true;
        face.enabled = false;
    }

    public void SetPortrait(Sprite sprite)
    {
        if (face == null)
        {
            return;
        }

        if (sprite == null)
        {
            face.enabled = false;
            face.sprite = null;
            return;
        }

        face.enabled = true;
        face.sprite = sprite;
        face.color = Color.white;
    }
}
