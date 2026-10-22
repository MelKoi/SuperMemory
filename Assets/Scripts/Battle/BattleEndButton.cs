using UnityEngine;
using UnityEngine.UI;

// 屏幕最右侧的结束按钮。只有我方主要阶段可以点，点下去进入结束阶段。
public sealed class BattleEndButton : MonoBehaviour
{
    private static readonly Color EnabledText = new Color(0.16f, 0.16f, 0.18f, 1f);
    private static readonly Color DisabledText = new Color(0.45f, 0.46f, 0.50f, 1f);

    private Button button;
    private Text label;
    private MatchRules rules;
    private bool sandboxMain = true;

    public static BattleEndButton Create(RectTransform parent)
    {
        var rect = BattleUiFactory.Create("EndButton", parent);
        BattleUiFactory.Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(88f, 56f));
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = BattleUiSprites.Square;
        image.color = Color.white;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.normalColor = new Color(0.93f, 0.93f, 0.94f, 1f);
        colors.highlightedColor = new Color(0.86f, 0.90f, 0.96f, 1f);
        colors.pressedColor = new Color(0.72f, 0.75f, 0.80f, 1f);
        colors.selectedColor = new Color(0.86f, 0.90f, 0.96f, 1f);
        colors.disabledColor = new Color(0.62f, 0.63f, 0.66f, 1f);
        colors.fadeDuration = 0.06f;
        button.colors = colors;

        var label = BattleUiFactory.CreateText("Text", rect, "结束", 26, TextAnchor.MiddleCenter);
        label.color = EnabledText;
        BattleUiFactory.Stretch(label.rectTransform, Vector2.zero, Vector2.one);

        var end = rect.gameObject.AddComponent<BattleEndButton>();
        end.button = button;
        end.label = label;
        end.sandboxMain = Application.isPlaying;
        button.onClick.AddListener(end.EndPhase);
        end.Apply(end.CanEnd());
        return end;
    }

    public void Bind(MatchRules match)
    {
        rules = match;
        Apply(CanEnd());
    }

    private void Update()
    {
        if (button == null)
        {
            return;
        }

        var allowed = CanEnd();
        if (button.interactable != allowed)
        {
            Apply(allowed);
        }
    }

    private void EndPhase()
    {
        if (!CanEnd())
        {
            Apply(false);
            return;
        }

        if (rules != null)
        {
            rules.EndMainPhase();
        }
        else
        {
            sandboxMain = false;
        }

        Apply(CanEnd());
    }

    private bool CanEnd()
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        if (rules == null)
        {
            return sandboxMain;
        }

        var flow = rules.Flow;
        return flow != null
            && !flow.IsFinished
            && flow.IsWaiting
            && flow.Timing == MatchTiming.Main
            && flow.ActiveSide == BattleSide.Player
            && !flow.AttackInProgress;
    }

    private void Apply(bool allowed)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = allowed;
        if (label != null)
        {
            label.color = allowed ? EnabledText : DisabledText;
        }
    }
}
