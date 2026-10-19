using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public sealed class BattleHud : MonoBehaviour
{
    private const float PortraitSize = 280f;
    private const float BarWidth = 300f;
    private const float WeaponSize = 168f;

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
    private bool editorBuildQueued;

    public BattleArea Player => playerArea;
    public BattleArea Enemy => enemyArea;

    private void OnEnable()
    {
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

        playerArea = CreateArea(canvasRect, BattleSide.Player);
        enemyArea = CreateArea(canvasRect, BattleSide.Enemy);
        Apply();
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
        BattleUiFactory.Place(portraitRect, corner, pivot, new Vector2(28f * direction, 12f * direction), new Vector2(PortraitSize, PortraitSize));
        var portrait = portraitRect.gameObject.AddComponent<BattlePortraitView>();
        portrait.Initialize();

        var barX = (28f + PortraitSize + 22f) * direction;
        var healthRect = BattleUiFactory.Create("HealthBar", areaRect);
        var toughnessRect = BattleUiFactory.Create("ToughnessBar", areaRect);
        var energyRect = BattleUiFactory.Create("EnergyRing", areaRect);
        if (player)
        {
            BattleUiFactory.Place(healthRect, corner, pivot, new Vector2(barX, 128f), new Vector2(BarWidth, 30f));
            BattleUiFactory.Place(toughnessRect, corner, pivot, new Vector2(barX, 96f), new Vector2(BarWidth, 22f));
            BattleUiFactory.Place(energyRect, corner, pivot, new Vector2(barX, 8f), new Vector2(210f, 80f));
        }
        else
        {
            BattleUiFactory.Place(healthRect, corner, pivot, new Vector2(barX, -36f), new Vector2(BarWidth, 30f));
            BattleUiFactory.Place(toughnessRect, corner, pivot, new Vector2(barX, -74f), new Vector2(BarWidth, 22f));
            BattleUiFactory.Place(energyRect, corner, pivot, new Vector2(barX, -104f), new Vector2(210f, 80f));
        }

        var health = healthRect.gameObject.AddComponent<BattleVitalBar>();
        health.Initialize(new Color(0.80f, 0.32f, 0.30f, 1f), !player);
        var toughness = toughnessRect.gameObject.AddComponent<BattleVitalBar>();
        toughness.Initialize(new Color(0.93f, 0.62f, 0.46f, 1f), !player);
        var energy = energyRect.gameObject.AddComponent<EnergyRing>();

        var weaponAnchor = player ? new Vector2(1f, 0f) : Vector2.up;
        var weaponSize = new Vector2(WeaponSize, WeaponSize);
        var weapon1Rect = BattleUiFactory.Create("Weapon1", areaRect);
        var weapon2Rect = BattleUiFactory.Create("Weapon2", areaRect);
        if (player)
        {
            BattleUiFactory.Place(weapon1Rect, weaponAnchor, weaponAnchor, new Vector2(-224f, 206f), weaponSize);
            BattleUiFactory.Place(weapon2Rect, weaponAnchor, weaponAnchor, new Vector2(-32f, 206f), weaponSize);
        }
        else
        {
            BattleUiFactory.Place(weapon1Rect, weaponAnchor, weaponAnchor, new Vector2(32f, -206f), weaponSize);
            BattleUiFactory.Place(weapon2Rect, weaponAnchor, weaponAnchor, new Vector2(224f, -206f), weaponSize);
        }

        var firstWeapon = weapon1Rect.gameObject.AddComponent<BattleWeaponView>();
        var secondWeapon = weapon2Rect.gameObject.AddComponent<BattleWeaponView>();
        firstWeapon.Initialize();
        secondWeapon.Initialize();

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
        area.Initialize(side, portrait, health, toughness, energy, firstWeapon, secondWeapon, hand);
        return area;
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
