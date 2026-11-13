using System.Collections.Generic;
using System.Globalization;
using CardTables;
using UnityEngine;
using UnityEngine.UI;

// 点击 Play 后组卡、站位、起手。先手的第一个主要阶段直接结束，然后停在你的主要阶段。之后每点一次结束，就走完对方的回合，再回到你的主要阶段。
public sealed class BattleDeckSelect : MonoBehaviour
{
    private static readonly Color Panel = new Color(0.94f, 0.95f, 0.96f, 1f);
    private static readonly Color Ink = new Color(0.16f, 0.16f, 0.18f, 1f);
    private static readonly Color Idle = new Color(0.86f, 0.87f, 0.89f, 1f);
    private static readonly Color Picked = new Color(0.55f, 0.72f, 0.86f, 1f);

    private BattleHud hud;
    private GameObject root;
    private RectTransform characterColumn;
    private Text weaponNote;
    private Text title;
    private Text status;
    private Text resultText;
    private Text confirmLabel;
    private Button confirm;
    private bool showingResult;
    private bool showingSeating;
    private bool weaponsReady;
    private CardCatalog cards;
    private readonly List<Choice> characters = new List<Choice>();
    private int pickedCharacter = -1;
    private int pickedHandLimit = 4;
    private string glovesName = "拳套";
    private string swordName = "大剑";

    public DeckBuildResult Result { get; private set; }
    public FighterState Player { get; private set; }
    public SeatingResult Seating { get; private set; }
    public FighterState Enemy { get; private set; }
    public BattleSide ActiveSide { get; private set; }

    public static BattleDeckSelect Create(RectTransform parent, BattleHud owner)
    {
        var rect = BattleUiFactory.Create("DeckSelect", parent);
        BattleUiFactory.Stretch(rect, Vector2.zero, Vector2.one);
        var select = rect.gameObject.AddComponent<BattleDeckSelect>();
        select.hud = owner;
        select.root = rect.gameObject;
        select.Build(rect);
        select.root.SetActive(false);
        return select;
    }

    public void Open()
    {
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        showingResult = false;
        showingSeating = false;
        title.text = "组卡";
        hud.ClearOpeningHand();
        characterColumn.gameObject.SetActive(true);
        weaponNote.gameObject.SetActive(true);
        resultText.gameObject.SetActive(false);
        confirmLabel.text = "确定";
        LoadRoster();
        Refresh();
    }

    private void Build(RectTransform parent)
    {
        var shade = parent.gameObject.AddComponent<Image>();
        shade.sprite = BattleUiSprites.Square;
        shade.color = new Color(0.08f, 0.09f, 0.1f, 0.72f);
        shade.raycastTarget = true;

        var panel = BattleUiFactory.Create("Panel", parent);
        BattleUiFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 680f));
        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.sprite = BattleUiSprites.Square;
        panelImage.color = Panel;
        panelImage.raycastTarget = true;

        title = BattleUiFactory.CreateText("Title", panel, "组卡", 34, TextAnchor.MiddleCenter);
        title.color = Ink;
        BattleUiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(400f, 48f));

        weaponNote = BattleUiFactory.CreateText("WeaponNote", panel, "武器固定为拳套、大剑", 24, TextAnchor.MiddleLeft);
        weaponNote.color = Ink;
        BattleUiFactory.Place(weaponNote.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -88f), new Vector2(520f, 36f));

        characterColumn = BattleUiFactory.Create("Characters", panel);
        BattleUiFactory.Place(characterColumn, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -140f), new Vector2(560f, 420f));
        resultText = BattleUiFactory.CreateText("DeckList", panel, string.Empty, 22, TextAnchor.UpperLeft);
        resultText.color = Ink;
        resultText.horizontalOverflow = HorizontalWrapMode.Wrap;
        resultText.verticalOverflow = VerticalWrapMode.Overflow;
        BattleUiFactory.Place(resultText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -96f), new Vector2(540f, 500f));
        resultText.gameObject.SetActive(false);

        status = BattleUiFactory.CreateText("Status", panel, string.Empty, 22, TextAnchor.MiddleLeft);
        status.color = Ink;
        BattleUiFactory.Place(status.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(36f, 28f), new Vector2(280f, 64f));

        confirm = CreateButton(panel, "确定", new Vector2(1f, 0f), new Vector2(-168f, 36f), Confirm);
        confirmLabel = confirm.GetComponentInChildren<Text>();
        var close = CreateButton(panel, "关闭", new Vector2(1f, 0f), new Vector2(-36f, 36f), Close);
        close.GetComponent<Image>().color = Idle;
    }

    private void LoadRoster()
    {
        ClearColumn(characterColumn);
        characters.Clear();
        cards = null;
        weaponsReady = false;
        if (!TableDatabase.TryGet("Charactor", out var characterTable)
            || !TableDatabase.TryGet("Weapon", out var weaponTable)
            || !TableDatabase.TryGet("AttackCard", out var attacks)
            || !TableDatabase.TryGet("ActionCard", out var actions)
            || !TableDatabase.TryGet("BuffCard", out var buffs))
        {
            status.text = "角色、武器或卡牌表没有读到。";
            return;
        }

        if (!weaponTable.Contains(DeckBuilder.GlovesId) || !weaponTable.Contains(DeckBuilder.GreatswordId))
        {
            status.text = "武器表里没有默认的拳套或大剑。";
            return;
        }

        glovesName = weaponTable.GetString(DeckBuilder.GlovesId, "name");
        swordName = weaponTable.GetString(DeckBuilder.GreatswordId, "name");
        weaponNote.text = "武器固定为" + glovesName + "、" + swordName;

        try
        {
            cards = CardCatalog.Load(attacks, actions, buffs);
        }
        catch (System.Exception exception)
        {
            status.text = "卡牌表读失败：" + exception.Message;
            return;
        }

        weaponsReady = true;
        var weaponCards = DeckBuilder.Collect(cards, DeckBuilder.WeaponOwner(DeckBuilder.GlovesId)).Count
            + DeckBuilder.Collect(cards, DeckBuilder.WeaponOwner(DeckBuilder.GreatswordId)).Count;
        var characterY = 0f;
        for (var row = 0; row < characterTable.RowCount; row++)
        {
            var name = characterTable.GetStringAt(row, "name");
            var handLimit = characterTable.GetIntAt(row, "cardnum");
            if (handLimit <= 0 || name.StartsWith("标准角色", System.StringComparison.Ordinal))
            {
                continue;
            }

            var id = characterTable.GetIntAt(row, "id");
            var count = DeckBuilder.Collect(cards, DeckBuilder.CharacterOwner(id)).Count + weaponCards;
            var choice = AddChoice(characterColumn, name + "  " + count.ToString(CultureInfo.InvariantCulture) + "张", characterY, () => PickCharacter(id, handLimit));
            choice.Id = id;
            characters.Add(choice);
            characterY -= 64f;
        }
    }

    private void PickCharacter(int id, int handLimit)
    {
        pickedCharacter = id;
        pickedHandLimit = handLimit;
        Refresh();
    }

    private void Refresh()
    {
        for (var i = 0; i < characters.Count; i++)
        {
            Paint(characters[i], characters[i].Id == pickedCharacter);
        }

        var ready = !showingResult && !showingSeating && weaponsReady && cards != null && pickedCharacter >= 0;
        if (confirm != null)
        {
            confirm.interactable = showingResult || showingSeating || ready;
        }

        if (cards == null || !weaponsReady)
        {
            return;
        }

        status.text = ready
            ? "确定后用" + glovesName + "和" + swordName + "组牌库，不洗牌。"
            : "选择 1 名角色。武器是" + glovesName + "和" + swordName + "。";
    }

    private void Confirm()
    {
        if (showingSeating)
        {
            ShowOpeningHand();
            return;
        }

        if (showingResult)
        {
            ShowSeating();
            return;
        }

        if (cards == null || !weaponsReady || pickedCharacter < 0)
        {
            return;
        }

        var result = DeckBuilder.Build(cards, pickedCharacter);
        var fighter = DeckBuilder.CreatePlayer(result, pickedHandLimit, ReadWeapon(result.WeaponId1), ReadWeapon(result.WeaponId2));
        Result = result;
        Player = fighter;
        hud.ApplyPlayerDeck(result);
        hud.SetPlayResult(glovesName + "、" + swordName + "。牌库 " + result.Deck.Count.ToString(CultureInfo.InvariantCulture) + " 张，尚未洗牌。");
        ShowResult(result);
    }

    private void ShowResult(DeckBuildResult result)
    {
        showingResult = true;
        characterColumn.gameObject.SetActive(false);
        weaponNote.gameObject.SetActive(false);
        resultText.gameObject.SetActive(true);
        confirm.interactable = true;
        confirmLabel.text = "下一步";
        var lines = DeckBuilder.Summarize(cards, result.Deck);
        resultText.text = "武器 " + WeaponName(result.WeaponId1) + "、" + WeaponName(result.WeaponId2)
            + "\n牌库 " + result.Deck.Count.ToString(CultureInfo.InvariantCulture) + " 张，手牌 0，未洗牌\n\n"
            + string.Join("\n", lines);
        status.text = "括号备注的牌没有进入牌库。下一步安排先后手和站位。";
    }

    private void ShowSeating()
    {
        var firstPlayer = Random.value < 0.5f ? BattleSide.Player : BattleSide.Enemy;
        var seating = OpeningPlacement.Create(firstPlayer);
        Seating = seating;
        var board = FindFirstObjectByType<BattleBoardPerspective>();
        if (board != null)
        {
            board.PlaceFighters(seating.PlayerColumn, seating.PlayerRow, seating.EnemyColumn, seating.EnemyRow);
        }

        showingSeating = true;
        title.text = "先后手与站位";
        confirm.interactable = true;
        confirmLabel.text = "下一步";
        var you = seating.FirstPlayer == BattleSide.Player ? "你是先手" : "你是后手";
        var them = seating.FirstPlayer == BattleSide.Player ? "对方是后手" : "对方是先手";
        resultText.text = you + "\n" + them
            + "\n我方在近端中间路，对方在远端同一路"
            + "\n距离 " + seating.Distance.ToString(CultureInfo.InvariantCulture);
        status.text = "下一步进入你的回合。牌库还有牌时，会先问要不要主动洗牌。";
        hud.SetPlayResult(you + "。双方在场地两端，距离 " + seating.Distance.ToString(CultureInfo.InvariantCulture) + "。");
    }

    private void ShowOpeningHand()
    {
        var random = new System.Random();
        Enemy = new FighterState(BattleSide.Enemy, string.Empty, 0);
        OpeningHand.Shuffle(Player, random);
        OpeningHand.Shuffle(Enemy, random);
        OpeningHand.DrawTo(Player, MatchTimingPolicy.OpeningHandCount);
        OpeningHand.DrawTo(Enemy, MatchTimingPolicy.OpeningHandCount);
        hud.ShowPlayerHand(cards, Player.Hand, Player.Deck.Count);
        hud.ShowEnemyHand(Enemy.Hand.Count);
        ShowOpeningEnergy();
    }

    private void ShowOpeningEnergy()
    {
        OpeningEnergyGrant.Grant(Player, Enemy, Seating.FirstPlayer);
        hud.ApplyOpeningEnergy(Player.Energy, Enemy.Energy);
        ShowTurnStart();
    }

    private void ShowTurnStart()
    {
        var rules = MatchPlaytest.ReachPlayerMain(cards, Player, Enemy, Seating.FirstPlayer, new System.Random());
        ActiveSide = rules.Flow.ActiveSide;
        hud.BindMatch(rules);
        if (rules.Flow.Timing == MatchTiming.Main)
        {
            hud.AnnouncePlayerMain();
        }
        Close();
    }

    private void Close()
    {
        root.SetActive(false);
    }

    private static WeaponLoadout ReadWeapon(int weaponId)
    {
        if (!BattleTableReader.TryReadWeapon(weaponId, out _, out var light, out var heavy))
        {
            light = default;
            heavy = default;
        }

        return new WeaponLoadout(weaponId, DeckBuilder.WeaponOwner(weaponId), light, heavy);
    }

    private static string WeaponName(int weaponId)
    {
        if (TableDatabase.TryGet("Weapon", out var table) && table.Contains(weaponId))
        {
            return table.GetString(weaponId, "name");
        }

        return DeckBuilder.WeaponOwner(weaponId);
    }

    private Choice AddChoice(RectTransform column, string label, float y, UnityEngine.Events.UnityAction onClick)
    {
        var button = CreateButton(column, label, new Vector2(0f, 1f), new Vector2(0f, y), onClick);
        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(540f, 52f);
        var choice = new Choice();
        choice.Image = button.GetComponent<Image>();
        return choice;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchor, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        var rect = BattleUiFactory.Create(label, parent);
        BattleUiFactory.Place(rect, anchor, new Vector2(1f, 0f), position, new Vector2(120f, 44f));
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = Idle;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var text = BattleUiFactory.CreateText("Text", rect, label, 24, TextAnchor.MiddleCenter);
        text.color = Ink;
        BattleUiFactory.Stretch(text.rectTransform, Vector2.zero, Vector2.one);
        button.onClick.AddListener(onClick);
        return button;
    }

    private static void Paint(Choice choice, bool picked)
    {
        if (choice.Image != null)
        {
            choice.Image.color = picked ? Picked : Idle;
        }
    }

    private static void ClearColumn(RectTransform column)
    {
        for (var i = column.childCount - 1; i >= 0; i--)
        {
            Destroy(column.GetChild(i).gameObject);
        }
    }

    private sealed class Choice
    {
        public int Id;
        public Image Image;
    }
}
