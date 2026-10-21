using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class BattleActionMenu : MonoBehaviour
{
    private static readonly Color TitleColor = new Color(0.16f, 0.16f, 0.18f, 1f);
    private static readonly Color EnabledText = Color.white;
    private static readonly Color DisabledText = new Color(0.42f, 0.44f, 0.48f, 1f);

    private BattleArea player;
    private BattleArea enemy;
    private BattleActionSettings settings;
    private BattleBoardPerspective board;
    private RectTransform actionPanel;
    private RectTransform weaponPanel;
    private Button forwardButton;
    private Button backwardButton;
    private Button firstWeaponButton;
    private Button secondWeaponButton;
    private Text forwardLabel;
    private Text backwardLabel;
    private Text firstWeaponLabel;
    private Text secondWeaponLabel;

    public static BattleActionMenu Create(RectTransform parent, BattleArea playerArea, BattleArea enemyArea, BattleActionSettings actionSettings)
    {
        var root = BattleUiFactory.Create("ActionMenu", parent);
        BattleUiFactory.Stretch(root, Vector2.zero, Vector2.one);
        var menu = root.gameObject.AddComponent<BattleActionMenu>();
        menu.player = playerArea;
        menu.enemy = enemyArea;
        menu.settings = actionSettings;
        menu.Build();
        menu.Close();
        return menu;
    }

    public void ShowActions()
    {
        SetOpen(actionPanel, weaponPanel);
    }

    public void Refresh()
    {
        if (settings == null || player == null)
        {
            return;
        }

        var energy = player.Vitals != null ? player.Vitals.Energy : 0;
        var arena = Board();
        var canForward = arena != null && arena.CanMovePlayer(settings.ForwardCells);
        var canBackward = arena != null && arena.CanMovePlayer(-settings.BackwardCells);
        SetChoice(
            forwardButton,
            forwardLabel,
            MoveCaption("前进", settings.ForwardEnergyCost, settings.ForwardCells, energy >= settings.ForwardEnergyCost, canForward),
            canForward && energy >= settings.ForwardEnergyCost);
        SetChoice(
            backwardButton,
            backwardLabel,
            MoveCaption("后退", settings.BackwardEnergyCost, settings.BackwardCells, energy >= settings.BackwardEnergyCost, canBackward),
            canBackward && energy >= settings.BackwardEnergyCost);
        SetChoice(firstWeaponButton, firstWeaponLabel, WeaponCaption(1), WeaponReady(1));
        SetChoice(secondWeaponButton, secondWeaponLabel, WeaponCaption(2), WeaponReady(2));
    }

    private void Build()
    {
        actionPanel = CreatePanel("ActionPanel", "行动", Close);
        forwardButton = CreateChoice(actionPanel, "Forward", new Vector2(0f, -72f), new Vector2(340f, 76f), MoveForward, out forwardLabel);
        backwardButton = CreateChoice(actionPanel, "Backward", new Vector2(0f, -160f), new Vector2(340f, 76f), MoveBackward, out backwardLabel);
        CreateChoice(actionPanel, "WeaponAttack", new Vector2(0f, -248f), new Vector2(340f, 76f), ShowWeapons, out var attackLabel);
        attackLabel.text = "武器攻击";

        weaponPanel = CreatePanel("WeaponPanel", "选择武器", ShowActions);
        firstWeaponButton = CreateChoice(weaponPanel, "Weapon1", new Vector2(0f, -72f), new Vector2(340f, 108f), () => Attack(1), out firstWeaponLabel);
        secondWeaponButton = CreateChoice(weaponPanel, "Weapon2", new Vector2(0f, -192f), new Vector2(340f, 108f), () => Attack(2), out secondWeaponLabel);
        Refresh();
    }

    private void ShowWeapons()
    {
        SetOpen(weaponPanel, actionPanel);
    }

    private void Close()
    {
        if (actionPanel != null)
        {
            actionPanel.gameObject.SetActive(false);
        }

        if (weaponPanel != null)
        {
            weaponPanel.gameObject.SetActive(false);
        }
    }

    private void SetOpen(RectTransform shown, RectTransform hidden)
    {
        if (hidden != null)
        {
            hidden.gameObject.SetActive(false);
        }

        if (shown != null)
        {
            shown.gameObject.SetActive(true);
        }

        transform.SetAsLastSibling();
        Refresh();
    }

    private void MoveForward()
    {
        if (settings == null)
        {
            return;
        }

        Move(settings.ForwardCells, settings.ForwardEnergyCost);
    }

    private void MoveBackward()
    {
        if (settings == null)
        {
            return;
        }

        Move(-settings.BackwardCells, settings.BackwardEnergyCost);
    }

    private void Move(int rowDelta, int cost)
    {
        var arena = Board();
        if (player == null || settings == null || arena == null)
        {
            return;
        }

        if (!arena.CanMovePlayer(rowDelta) || !player.TrySpendEnergy(cost))
        {
            Refresh();
            return;
        }

        if (!arena.TryMovePlayer(rowDelta))
        {
            player.SetEnergy(player.Vitals.Energy + cost);
        }

        settings.AdoptEnergy(player.Vitals.Energy);
        Refresh();
    }

    private void Attack(int weaponNumber)
    {
        if (player == null || enemy == null || settings == null || !WeaponReady(weaponNumber))
        {
            Refresh();
            return;
        }

        var view = player.GetWeapon(weaponNumber);
        view.GetDamage(out var lifeDamage, out var toughnessDamage);
        enemy.ApplyDamage(lifeDamage, toughnessDamage);
        view.SpendAllCharge();
        settings.AdoptCharge(weaponNumber, 0);
        Refresh();
    }

    private bool WeaponReady(int weaponNumber)
    {
        if (player == null)
        {
            return false;
        }

        var equipped = weaponNumber == 1 ? player.HasWeapon1 : player.HasWeapon2;
        var view = player.GetWeapon(weaponNumber);
        return equipped && view != null && view.CanAttack;
    }

    private string WeaponCaption(int weaponNumber)
    {
        var fallback = "武器" + weaponNumber.ToString(CultureInfo.InvariantCulture);
        if (player == null)
        {
            return fallback;
        }

        var equipped = weaponNumber == 1 ? player.HasWeapon1 : player.HasWeapon2;
        var weaponName = weaponNumber == 1 ? player.WeaponName1 : player.WeaponName2;
        if (!equipped)
        {
            return fallback + "\n未装备";
        }

        if (string.IsNullOrEmpty(weaponName))
        {
            weaponName = fallback;
        }

        var view = player.GetWeapon(weaponNumber);
        var charge = view != null ? view.Charge : 0;
        var chargeText = charge.ToString(CultureInfo.InvariantCulture);
        if (view == null || !view.CanAttack)
        {
            return weaponName + "\n蓄能 " + chargeText + " · 无法攻击";
        }

        view.GetDamage(out var lifeDamage, out var toughnessDamage);
        return weaponName
            + "\n蓄能 "
            + chargeText
            + " · 伤害 "
            + lifeDamage.ToString(CultureInfo.InvariantCulture)
            + "/"
            + toughnessDamage.ToString(CultureInfo.InvariantCulture);
    }

    private BattleBoardPerspective Board()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<BattleBoardPerspective>();
        }

        return board;
    }

    private static string MoveCaption(string title, int cost, int cells, bool affordable, bool reachable)
    {
        var detail = cost.ToString(CultureInfo.InvariantCulture)
            + " 能量 · "
            + cells.ToString(CultureInfo.InvariantCulture)
            + " 格";
        if (!affordable)
        {
            detail = "能量不足";
        }
        else if (!reachable)
        {
            detail = "无法移动";
        }

        return title + "\n" + detail;
    }

    private static void SetChoice(Button button, Text label, string content, bool enabled)
    {
        if (button == null || label == null)
        {
            return;
        }

        label.text = content;
        button.interactable = enabled;
        label.color = enabled ? EnabledText : DisabledText;
    }

    private RectTransform CreatePanel(string name, string title, UnityAction onBack)
    {
        var panel = BattleUiFactory.Create(name, transform);
        BattleUiFactory.Place(panel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-28f, 0f), new Vector2(400f, 400f));
        var image = panel.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = new Color(0.96f, 0.97f, 0.98f, 0.98f);

        CreateChoice(panel, "Back", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -12f), new Vector2(92f, 36f), onBack, out var backLabel);
        backLabel.fontSize = 20;
        backLabel.text = "回退";

        var titleLabel = BattleUiFactory.CreateText("Title", panel, title, 26, TextAnchor.MiddleCenter);
        titleLabel.color = TitleColor;
        BattleUiFactory.Place(titleLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(28f, -14f), new Vector2(180f, 34f));
        return panel;
    }

    private Button CreateChoice(RectTransform parent, string name, Vector2 position, Vector2 size, UnityAction onClick, out Text label)
    {
        return CreateChoice(parent, name, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, size, onClick, out label);
    }

    private Button CreateChoice(
        RectTransform parent,
        string name,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 position,
        Vector2 size,
        UnityAction onClick,
        out Text label)
    {
        var rect = BattleUiFactory.Create(name, parent);
        BattleUiFactory.Place(rect, anchor, pivot, position, size);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = Color.white;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.normalColor = new Color(0.22f, 0.26f, 0.32f, 1f);
        colors.highlightedColor = new Color(0.32f, 0.38f, 0.46f, 1f);
        colors.pressedColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        colors.selectedColor = new Color(0.32f, 0.38f, 0.46f, 1f);
        colors.disabledColor = new Color(0.78f, 0.79f, 0.82f, 1f);
        colors.fadeDuration = 0.06f;
        button.colors = colors;
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        label = BattleUiFactory.CreateText("Text", rect, string.Empty, 22, TextAnchor.MiddleCenter);
        label.color = EnabledText;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        BattleUiFactory.Stretch(label.rectTransform, Vector2.zero, Vector2.one);
        return button;
    }
}
