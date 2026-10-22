using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(BattleActionSettings))]
public sealed class BattleHud : MonoBehaviour
{
    private const float PortraitSize = 280f;
    private const float BarWidth = 300f;

    [SerializeField] private int playerCharacterId = -1;
    [SerializeField] private int playerWeaponId1 = -1;
    [SerializeField] private int playerWeaponId2 = -1;
    [SerializeField] private int enemyCharacterId = -1;
    [SerializeField] private int enemyWeaponId1 = -1;
    [SerializeField] private int enemyWeaponId2 = -1;
    [SerializeField] private CharacterArt[] characterPortraits = Array.Empty<CharacterArt>();
    [SerializeField] private WeaponArt[] weaponIcons = Array.Empty<WeaponArt>();

    private BattleArea playerArea;
    private BattleArea enemyArea;
    private Button playerActionButton;
    private bool editorBuildQueued;

    public BattleArea Player => playerArea;
    public BattleArea Enemy => enemyArea;

    private void OnEnable()
    {
        if (GetComponent<BattleActionSettings>() == null)
        {
            gameObject.AddComponent<BattleActionSettings>();
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (editorBuildQueued)
            {
                return;
            }

            editorBuildQueued = true;
            EditorApplication.delayCall += DelayedEditorBuild;
            return;
        }
#endif
        Rebuild();
    }

#if UNITY_EDITOR
    private void DelayedEditorBuild()
    {
        editorBuildQueued = false;
        if (this == null || !isActiveAndEnabled)
        {
            return;
        }

        Rebuild();
    }
#endif

    private void OnValidate()
    {
        if (playerArea != null && enemyArea != null)
        {
            Apply();
        }
    }

    public void Refresh()
    {
        if (playerArea == null || enemyArea == null)
        {
            Rebuild();
            return;
        }

        Apply();
    }

    private void Rebuild()
    {
        ClearGenerated();

        var canvasRect = BattleUiFactory.Create("BattleCanvas", transform);
        BattleUiFactory.Stretch(canvasRect, Vector2.zero, Vector2.one);
        var canvas = canvasRect.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect.gameObject.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();
        playerArea = CreateArea(canvasRect, BattleSide.Player);
        enemyArea = CreateArea(canvasRect, BattleSide.Enemy);
        Apply();
        var menu = BattleActionMenu.Create(canvasRect, playerArea, enemyArea, GetComponent<BattleActionSettings>());
        if (playerActionButton != null)
        {
            playerActionButton.onClick.AddListener(menu.ShowActions);
        }

        BattleEndButton.Create(canvasRect);
    }

    private static void EnsureEventSystem()
    {
        if (!Application.isPlaying || FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventObject = new GameObject("EventSystem");
        eventObject.AddComponent<EventSystem>();
        eventObject.AddComponent<InputSystemUIInputModule>();
    }

    private void ClearGenerated()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        playerArea = null;
        enemyArea = null;
        playerActionButton = null;
    }

    private void Apply()
    {
        playerArea.LoadOpeningState(
            playerCharacterId,
            playerWeaponId1,
            playerWeaponId2,
            FindPortrait(playerCharacterId),
            FindWeaponIcon(playerWeaponId1),
            FindWeaponIcon(playerWeaponId2));
        enemyArea.LoadOpeningState(
            enemyCharacterId,
            enemyWeaponId1,
            enemyWeaponId2,
            FindPortrait(enemyCharacterId),
            FindWeaponIcon(enemyWeaponId1),
            FindWeaponIcon(enemyWeaponId2));
        GetComponent<BattleActionSettings>()?.ApplyTo(playerArea);
        var menu = GetComponentInChildren<BattleActionMenu>(true);
        if (menu != null)
        {
            menu.Refresh();
        }
    }

    private BattleArea CreateArea(RectTransform parent, BattleSide side)
    {
        var areaRect = BattleUiFactory.Create(side == BattleSide.Player ? "PlayerArea" : "EnemyArea", parent);
        BattleUiFactory.Stretch(areaRect, Vector2.zero, Vector2.one);
        var area = areaRect.gameObject.AddComponent<BattleArea>();

        var player = side == BattleSide.Player;
        var corner = player ? Vector2.zero : Vector2.one;
        var pivot = corner;
        var direction = player ? 1f : -1f;

        var portraitRect = BattleUiFactory.Create("CharacterPortrait", areaRect);
        BattleUiFactory.Place(portraitRect, corner, pivot, new Vector2(24f * direction, 20f * direction), new Vector2(PortraitSize, PortraitSize));
        var portrait = portraitRect.gameObject.AddComponent<BattlePortraitView>();
        portrait.Initialize();

        var barX = 322f * direction;
        var healthRect = BattleUiFactory.Create("HealthBar", areaRect);
        var toughnessRect = BattleUiFactory.Create("ToughnessBar", areaRect);
        var energyRect = BattleUiFactory.Create("EnergyRing", areaRect);
        BattleUiFactory.Place(healthRect, corner, pivot, new Vector2(barX, 156f * direction), new Vector2(BarWidth, 28f));
        BattleUiFactory.Place(toughnessRect, corner, pivot, new Vector2(barX, 120f * direction), new Vector2(BarWidth, 22f));
        BattleUiFactory.Place(energyRect, corner, pivot, new Vector2(barX, 24f * direction), new Vector2(190f, 72f));

        var health = healthRect.gameObject.AddComponent<BattleVitalBar>();
        health.Initialize(new Color(0.80f, 0.32f, 0.30f, 1f), !player);
        var toughness = toughnessRect.gameObject.AddComponent<BattleVitalBar>();
        toughness.Initialize(new Color(0.93f, 0.62f, 0.46f, 1f), !player);
        var energy = energyRect.gameObject.AddComponent<EnergyRing>();

        var weaponAnchor = player ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
        var weaponSize = new Vector2(136f, 136f);
        var weapon1Rect = BattleUiFactory.Create("Weapon1", areaRect);
        var weapon2Rect = BattleUiFactory.Create("Weapon2", areaRect);
        BattleUiFactory.Place(weapon1Rect, weaponAnchor, weaponAnchor, new Vector2(-42f * direction, 187f * direction), weaponSize);
        BattleUiFactory.Place(weapon2Rect, weaponAnchor, weaponAnchor, new Vector2(-240f * direction, 63f * direction), weaponSize);

        var firstWeapon = weapon1Rect.gameObject.AddComponent<BattleWeaponView>();
        var secondWeapon = weapon2Rect.gameObject.AddComponent<BattleWeaponView>();
        firstWeapon.Initialize();
        secondWeapon.Initialize();
        var firstReadout = CreateWeaponReadout(weapon1Rect, !player);
        var secondReadout = CreateWeaponReadout(weapon2Rect, !player);
        firstWeapon.SetReadout(firstReadout[0], firstReadout[1]);
        secondWeapon.SetReadout(secondReadout[0], secondReadout[1]);

        var cardAnchor = weaponAnchor;
        var cardPivot = new Vector2(0.5f, 0.5f);
        var cardRect = BattleUiFactory.Create("CardTumb", areaRect);
        var cardImage = cardRect.gameObject.AddComponent<Image>();
        cardImage.sprite = BattleUiSprites.Square;
        cardImage.color = Color.white;
        cardImage.raycastTarget = false;
        BattleUiFactory.Place(cardRect, cardAnchor, cardPivot, new Vector2(-590f * direction, 251f * direction), new Vector2(68f, 96f));
        cardRect.localRotation = Quaternion.Euler(0f, 0f, -18f);

        var textAlignment = player ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        var textPivot = player ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        var discardLabel = BattleUiFactory.CreateText("Discard", areaRect, "弃牌：0", 26, textAlignment);
        var deckLabel = BattleUiFactory.CreateText("Deck", areaRect, "牌库：0", 26, textAlignment);
        BattleUiFactory.Place(deckLabel.rectTransform, cardAnchor, textPivot, new Vector2(-544f * direction, 280f * direction), new Vector2(120f, 34f));
        BattleUiFactory.Place(discardLabel.rectTransform, cardAnchor, textPivot, new Vector2(-544f * direction, 242f * direction), new Vector2(120f, 34f));

        var actionAnchor = player ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 1f);
        var actionRect = BattleUiFactory.Create("ActionButton", areaRect);
        BattleUiFactory.Place(actionRect, actionAnchor, actionAnchor, new Vector2(-396f * direction, 272f * direction), new Vector2(114f, 44f));
        var actionImage = actionRect.gameObject.AddComponent<Image>();
        actionImage.sprite = BattleUiSprites.Square;
        actionImage.color = new Color(0.93f, 0.93f, 0.94f, 1f);
        actionImage.raycastTarget = player;
        if (player)
        {
            playerActionButton = actionRect.gameObject.AddComponent<Button>();
            playerActionButton.targetGraphic = actionImage;
            playerActionButton.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = playerActionButton.colors;
            colors.highlightedColor = new Color(0.86f, 0.9f, 0.96f, 1f);
            colors.pressedColor = new Color(0.72f, 0.75f, 0.8f, 1f);
            colors.fadeDuration = 0.06f;
            playerActionButton.colors = colors;
        }

        var actionLabel = BattleUiFactory.CreateText("Text", actionRect, "行动", 26, TextAnchor.MiddleCenter);
        actionLabel.color = new Color(0.16f, 0.16f, 0.18f, 1f);
        BattleUiFactory.Stretch(actionLabel.rectTransform, Vector2.zero, Vector2.one);

        var handRect = BattleUiFactory.Create("HandZone", areaRect);
        if (player)
        {
            BattleUiFactory.Stretch(handRect, new Vector2(0.58f, 0.015f), new Vector2(0.985f, 0.175f));
        }
        else
        {
            BattleUiFactory.Stretch(handRect, new Vector2(0.015f, 0.825f), new Vector2(0.42f, 0.985f));
        }

        var hand = handRect.gameObject.AddComponent<HandZone>();
        area.Initialize(side, portrait, health, toughness, energy, firstWeapon, secondWeapon, hand, deckLabel);
        return area;
    }

    private static Text[] CreateWeaponReadout(RectTransform weapon, bool above)
    {
        var readout = BattleUiFactory.Create("Readout", weapon);
        var edge = above ? 1f : 0f;
        readout.anchorMin = new Vector2(0f, edge);
        readout.anchorMax = new Vector2(1f, edge);
        readout.pivot = new Vector2(0.5f, above ? 0f : 1f);
        readout.anchoredPosition = new Vector2(0f, above ? 4f : -4f);
        readout.sizeDelta = new Vector2(0f, 56f);
        var charge = BattleUiFactory.CreateText("Charge", readout, "蓄能数 0", 24, TextAnchor.MiddleCenter);
        var damage = BattleUiFactory.CreateText("Damage", readout, "伤害 0/0", 24, TextAnchor.MiddleCenter);
        BattleUiFactory.Stretch(charge.rectTransform, new Vector2(0f, 0.5f), Vector2.one);
        BattleUiFactory.Stretch(damage.rectTransform, Vector2.zero, new Vector2(1f, 0.5f));
        return new[] { charge, damage };
    }

    private Sprite FindPortrait(int characterId)
    {
        return FindArt(characterPortraits, characterId, static entry => entry.characterId, static entry => entry.portrait, "Battle/Portraits/");
    }

    private Sprite FindWeaponIcon(int weaponId)
    {
        return FindArt(weaponIcons, weaponId, static entry => entry.weaponId, static entry => entry.icon, "Battle/Weapons/");
    }

    private static Sprite FindArt<T>(T[] entries, int id, Func<T, int> idOf, Func<T, Sprite> spriteOf, string resourceFolder)
    {
        if (id < 0)
        {
            return null;
        }

        if (entries != null)
        {
            for (var i = 0; i < entries.Length; i++)
            {
                if (idOf(entries[i]) == id && spriteOf(entries[i]) != null)
                {
                    return spriteOf(entries[i]);
                }
            }
        }

        return Resources.Load<Sprite>(resourceFolder + id.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Serializable]
    public struct CharacterArt
    {
        public int characterId;
        public Sprite portrait;
    }

    [Serializable]
    public struct WeaponArt
    {
        public int weaponId;
        public Sprite icon;
    }
}
