using System;
using System.Collections.Generic;
using System.Globalization;
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
    private MatchRules match;
    private BattleEndButton endButton;
    private BattleActionMenu actionMenu;
    private GameObject shufflePrompt;
    private Text shufflePromptText;
    private readonly System.Random random = new System.Random();
    private Button playerActionButton;
    private Text playResult;
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
        CreatePlayDrop(canvasRect);
        playerArea = CreateArea(canvasRect, BattleSide.Player);
        enemyArea = CreateArea(canvasRect, BattleSide.Enemy);
        Apply();
        actionMenu = BattleActionMenu.Create(canvasRect, playerArea, enemyArea, GetComponent<BattleActionSettings>());
        if (playerActionButton != null)
        {
            playerActionButton.onClick.AddListener(actionMenu.ShowActions);
        }

        endButton = BattleEndButton.Create(canvasRect);
        CreateShufflePrompt(canvasRect);
        if (Application.isPlaying)
        {
            CreatePlayButton(canvasRect);
            BattleDeckSelect.Create(canvasRect, this).Open();
        }
    }

    public void ShowPlayerHand(CardCatalog catalog, IReadOnlyList<CardInstance> hand, int deckCount)
    {
        if (playerArea == null)
        {
            return;
        }

        playerArea.SetDeckCount(deckCount);
        if (playerArea.Hand != null)
        {
            playerArea.Hand.Present(catalog, hand, OnCardDrop);
        }
    }

    public void ShowEnemyHand(int count)
    {
        if (enemyArea != null && enemyArea.Hand != null)
        {
            enemyArea.Hand.PresentBacks(count);
        }
    }

    public void BindMatch(MatchRules rules)
    {
        match = rules;
        if (endButton != null)
        {
            endButton.Bind(rules, OnDeclaredEnd);
        }

        if (actionMenu != null)
        {
            actionMenu.UseMatch(AttackWithRules, MoveWithRules);
        }

        RefreshMatchView();
        if (WaitingForShuffleChoice())
        {
            ShowShufflePrompt();
        }
    }

    public void AnnouncePlayerMain()
    {
        if (match == null)
        {
            return;
        }

        var turn = (match.Flow.TurnIndex + 1) / 2;
        SetPlayResult("你的第 " + turn.ToString(CultureInfo.InvariantCulture) + " 回合，主要阶段。能量 " + match.Player.Energy.ToString(CultureInfo.InvariantCulture) + "。");
    }

    void OnDeclaredEnd()
    {
        ResumeUntilPlayerMain();
    }

    void ResumeUntilPlayerMain()
    {
        var stop = MatchCycle.ContinueUntil(match, BattleSide.Player, random);
        if (stop.WaitingForShuffle)
        {
            ShowShufflePrompt();
            return;
        }

        RefreshMatchView();
        AnnouncePlayerMain();
    }

    bool WaitingForShuffleChoice()
    {
        return match != null
            && match.Flow.IsWaiting
            && match.Flow.Timing == MatchTiming.Shuffle
            && match.Flow.ActiveSide == BattleSide.Player
            && match.Player.Deck.Count > 0;
    }

    void ShowShufflePrompt()
    {
        if (shufflePrompt == null || match == null)
        {
            return;
        }

        var cost = MatchTimingPolicy.ShuffleLifeCost;
        shufflePromptText.text = "牌库还有 "
            + match.Player.Deck.Count.ToString(CultureInfo.InvariantCulture)
            + " 张。主动洗牌支付 "
            + cost.ToString(CultureInfo.InvariantCulture)
            + " 点生命，只洗当前牌库。";
        shufflePrompt.SetActive(true);
        shufflePrompt.transform.SetAsLastSibling();
        SetPlayResult("要不要主动洗牌？");
    }

    void AnswerShuffle(bool shuffle)
    {
        if (shufflePrompt != null)
        {
            shufflePrompt.SetActive(false);
        }

        if (match == null)
        {
            return;
        }

        MatchCycle.AnswerShuffle(match, BattleSide.Player, shuffle, random);
        RefreshMatchView();
        if (match.Flow.IsFinished)
        {
            SetPlayResult("支付了 1 点生命，这局结束。");
            return;
        }

        var done = shuffle ? "已主动洗牌。" : "没有洗牌。";
        AnnouncePlayerMain();
        SetPlayResult(done + playResult.text);
    }

    void AttackWithRules(int weaponNumber)
    {
        if (match == null)
        {
            return;
        }

        var board = FindFirstObjectByType<BattleBoardPerspective>();
        if (board == null)
        {
            SetPlayResult("没有场地。");
            return;
        }

        var distance = BoardGeometry.Distance(
            board.PlayerColumn,
            board.PlayerRow,
            board.EnemyColumn,
            board.EnemyRow);
        var openedWindow = false;
        if (!match.TryAttack(BattleSide.Player, weaponNumber - 1, distance, out var error))
        {
            SetPlayResult(error);
            return;
        }

        var attack = match.Flow.CurrentAttack;
        if (attack != null && attack.WaitingForResponses)
        {
            openedWindow = true;
            match.CloseResponseWindow();
        }

        RefreshMatchView();
        if (match.Flow.IsFinished)
        {
            SetPlayResult("生命归零，这局结束。");
            return;
        }

        var last = match.Flow.LastAttack;
        if (last != null && last.Negated)
        {
            SetPlayResult("攻击被打消。");
            return;
        }

        SetPlayResult(openedWindow ? "攻击已结算。对方没有打出对应。" : "攻击已结算。");
    }

    void MoveWithRules(int rowDelta)
    {
        if (match == null)
        {
            return;
        }

        var board = FindFirstObjectByType<BattleBoardPerspective>();
        if (board == null || !board.CanMovePlayer(rowDelta))
        {
            SetPlayResult("无法移动。");
            return;
        }

        if (!match.TryMove(BattleSide.Player, out var error))
        {
            SetPlayResult(error);
            return;
        }

        board.TryMovePlayer(rowDelta);
        SetPlayResult("已移动。");
    }

    void CreateShufflePrompt(RectTransform parent)
    {
        var root = BattleUiFactory.Create("ShufflePrompt", parent);
        BattleUiFactory.Stretch(root, Vector2.zero, Vector2.one);
        var dim = root.gameObject.AddComponent<Image>();
        dim.sprite = BattleUiSprites.Square;
        dim.color = new Color(0.08f, 0.09f, 0.1f, 0.45f);
        dim.raycastTarget = true;

        var panel = BattleUiFactory.Create("Panel", root);
        BattleUiFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 220f));
        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.sprite = BattleUiSprites.Square;
        panelImage.color = new Color(0.94f, 0.95f, 0.96f, 1f);

        shufflePromptText = BattleUiFactory.CreateText("Text", panel, string.Empty, 24, TextAnchor.MiddleCenter);
        shufflePromptText.color = new Color(0.16f, 0.16f, 0.18f, 1f);
        shufflePromptText.horizontalOverflow = HorizontalWrapMode.Wrap;
        BattleUiFactory.Place(shufflePromptText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(460f, 96f));

        CreatePromptButton(panel, "洗牌", new Vector2(-110f, 28f), () => AnswerShuffle(true));
        CreatePromptButton(panel, "不洗", new Vector2(110f, 28f), () => AnswerShuffle(false));
        shufflePrompt = root.gameObject;
        shufflePrompt.SetActive(false);
    }

    static void CreatePromptButton(RectTransform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        var rect = BattleUiFactory.Create(label, parent);
        BattleUiFactory.Place(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), position, new Vector2(160f, 48f));
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = new Color(0.22f, 0.26f, 0.32f, 1f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        var text = BattleUiFactory.CreateText("Text", rect, label, 22, TextAnchor.MiddleCenter);
        text.color = Color.white;
        BattleUiFactory.Stretch(text.rectTransform, Vector2.zero, Vector2.one);
    }

    public void RefreshMatchView()
    {
        if (match == null || playerArea == null)
        {
            return;
        }

        ShowPlayerHand(match.Cards, match.Player.Hand, match.Player.Deck.Count);
        ShowEnemyHand(match.Enemy.Hand.Count);
        playerArea.SetHealth(match.Player.Health);
        playerArea.SetToughness(match.Player.Toughness);
        if (enemyArea != null)
        {
            enemyArea.SetHealth(match.Enemy.Health);
            enemyArea.SetToughness(match.Enemy.Toughness);
        }

        ApplyOpeningEnergy(match.Player.Energy, match.Enemy.Energy);
        if (match.Player.Weapons.Count > 0)
        {
            playerArea.SetWeaponCharge(1, match.Player.Weapons[0].Charge);
        }

        if (match.Player.Weapons.Count > 1)
        {
            playerArea.SetWeaponCharge(2, match.Player.Weapons[1].Charge);
        }
    }

    bool OnCardDrop(CardInstance card, CardDropTarget target)
    {
        if (match == null)
        {
            SetPlayResult("还没到主要阶段。");
            return false;
        }

        var hand = match.Player.Hand;
        var index = -1;
        for (var i = 0; i < hand.Count; i++)
        {
            if (ReferenceEquals(hand[i], card))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return false;
        }

        var charging = target != null && target.Charge;
        string error;
        var ok = charging
            ? match.TryCharge(BattleSide.Player, index, target.WeaponSlot, out error)
            : match.TryPlay(BattleSide.Player, index, false, out error);
        if (!ok)
        {
            SetPlayResult(error);
            return false;
        }

        var flow = match.Flow;
        if (flow.Timing != MatchTiming.Main || flow.ActiveSide != BattleSide.Player || !flow.IsWaiting)
        {
            ResumeUntilPlayerMain();
            return true;
        }

        RefreshMatchView();
        SetPlayResult(charging ? "已蓄能。" : "已打出。");
        return true;
    }

    public void ApplyTurnState(int playerEnergy, int enemyEnergy, int playerDeck, int enemyDeck)
    {
        ApplyOpeningEnergy(playerEnergy, enemyEnergy);
        if (playerArea != null)
        {
            playerArea.SetDeckCount(playerDeck);
        }

        if (enemyArea != null)
        {
            enemyArea.SetDeckCount(enemyDeck);
        }
    }

    public void ApplyOpeningEnergy(int playerEnergy, int enemyEnergy)
    {
        if (playerArea != null)
        {
            playerArea.SetEnergy(playerEnergy);
        }

        if (enemyArea != null)
        {
            enemyArea.SetEnergy(enemyEnergy);
        }

        GetComponent<BattleActionSettings>()?.AdoptEnergy(playerEnergy);
        var menu = GetComponentInChildren<BattleActionMenu>(true);
        if (menu != null)
        {
            menu.Refresh();
        }
    }

    public void ClearOpeningHand()
    {
        if (playerArea != null && playerArea.Hand != null)
        {
            playerArea.Hand.Clear();
        }
    }

    public void ApplyPlayerDeck(DeckBuildResult result)
    {
        if (playerArea == null || result == null)
        {
            return;
        }

        playerArea.LoadOpeningState(
            result.CharacterId,
            result.WeaponId1,
            result.WeaponId2,
            FindPortrait(result.CharacterId),
            FindWeaponIcon(result.WeaponId1),
            FindWeaponIcon(result.WeaponId2));
        playerArea.SetDeckCount(result.Deck.Count);
        if (enemyArea != null)
        {
            enemyArea.SetDeckCount(0);
        }
    }

    public void SetPlayResult(string message)
    {
        if (playResult != null)
        {
            playResult.text = message ?? string.Empty;
        }
    }

    private void CreatePlayButton(RectTransform canvasRect)
    {
        var rect = BattleUiFactory.Create("PlayButton", canvasRect);
        BattleUiFactory.Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(140f, 48f));
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = new Color(0.93f, 0.93f, 0.94f, 1f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.highlightedColor = new Color(0.86f, 0.90f, 0.96f, 1f);
        colors.pressedColor = new Color(0.72f, 0.75f, 0.80f, 1f);
        colors.fadeDuration = 0.06f;
        button.colors = colors;
        var label = BattleUiFactory.CreateText("Text", rect, "Play", 26, TextAnchor.MiddleCenter);
        label.color = new Color(0.16f, 0.16f, 0.18f, 1f);
        BattleUiFactory.Stretch(label.rectTransform, Vector2.zero, Vector2.one);
        playResult = BattleUiFactory.CreateText("PlayResult", canvasRect, string.Empty, 22, TextAnchor.MiddleCenter);
        playResult.color = new Color(0.96f, 0.97f, 0.98f, 1f);
        BattleUiFactory.Place(playResult.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(720f, 32f));
        button.onClick.AddListener(() =>
        {
            var select = GetComponentInChildren<BattleDeckSelect>(true);
            if (select != null)
            {
                select.Open();
            }
        });
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
        actionMenu = null;
        shufflePrompt = null;
        shufflePromptText = null;
        playResult = null;
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
        var energyRect = BattleUiFactory.Create("EnergyBar", areaRect);
        BattleUiFactory.Place(healthRect, corner, pivot, new Vector2(barX, 156f * direction), new Vector2(BarWidth, 28f));
        BattleUiFactory.Place(toughnessRect, corner, pivot, new Vector2(barX, 120f * direction), new Vector2(BarWidth, 22f));
        BattleUiFactory.Place(energyRect, corner, pivot, new Vector2(barX, 84f * direction), new Vector2(BarWidth, 22f));

        var health = healthRect.gameObject.AddComponent<BattleVitalBar>();
        health.Initialize(new Color(0.80f, 0.32f, 0.30f, 1f), !player);
        var toughness = toughnessRect.gameObject.AddComponent<BattleVitalBar>();
        toughness.Initialize(new Color(0.93f, 0.62f, 0.46f, 1f), !player);
        var energy = energyRect.gameObject.AddComponent<BattleVitalBar>();
        energy.Initialize(new Color(0.93f, 0.76f, 0.28f, 1f), !player);

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
        if (player)
        {
            firstWeapon.AllowChargeDrop(0);
            secondWeapon.AllowChargeDrop(1);
        }
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
            BattleUiFactory.Place(handRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1280f, 380f));
        }
        else
        {
            BattleUiFactory.Place(handRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(760f, 160f));
        }

        var hand = handRect.gameObject.AddComponent<HandZone>();
        area.Initialize(side, portrait, health, toughness, energy, firstWeapon, secondWeapon, hand, deckLabel);
        return area;
    }

    private static void CreatePlayDrop(RectTransform canvasRect)
    {
        var play = BattleUiFactory.Create("PlayDrop", canvasRect);
        BattleUiFactory.Stretch(play, Vector2.zero, Vector2.one);
        var image = play.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;
        play.gameObject.AddComponent<CardDropTarget>();
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
