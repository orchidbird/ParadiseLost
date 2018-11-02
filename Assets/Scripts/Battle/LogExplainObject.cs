using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;

public class LogExplainObject : MonoBehaviour{
}

public class LogExplainText : LogExplainObject {
    public Text text;
}

public class LogExplainTile : LogExplainObject, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
    public Tile tile;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(tile != null)
            tile.sprite.color = Color.blue;
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(tile != null)
            tile.sprite.color = Color.white;
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        if(tile != null)
            StartCoroutine(BattleManager.Instance.cameraMover.Slide(tile.transform.position, 0.5f));
    }
}
public class LogExplainUnit : LogExplainObject, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
    public Unit unit;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if (unit != null)
            unit.SetMouseOverHighlightUnitAndPortrait(true);
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(unit != null)
            unit.SetMouseOverHighlightUnitAndPortrait(false);
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        if(unit != null)
            StartCoroutine(BattleManager.Instance.cameraMover.Slide(unit.transform.position, 0.5f));
    }
}
public class LogExplainStatusEffect : LogExplainObject, IPointerEnterHandler, IPointerExitHandler {
    public StatusEffect statusEffect;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        BattleUIManager UM = BattleUIManager.Instance;

        RectPosition rectPosition = new RectPosition(UM.logDisplayPanel.transform, new Vector2(1, 0.5f), new Vector2(0, 0),
                new Vector3(0, transform.position.y - Utility.GetRectCornerPosition(UM.logDisplayPanel.gameObject, Direction.LeftDown).y, 0), inScreen: true);
        BattleUIManager.Instance.ActivateStatusEffectDisplayPanelAndSetText(rectPosition, statusEffect);
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        BattleUIManager.Instance.DeactivateStatusEffectDisplayPanel();
    }
}
public class LogExplainSkill : LogExplainObject, IPointerEnterHandler, IPointerExitHandler {
    public Skill skill;
    public Unit caster;
    public UnitInfo casterInfo; //caster이 destroy되었을 때 사용
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        BattleUIManager UM = BattleUIManager.Instance;

        RectPosition rectPosition = new RectPosition(UM.logDisplayPanel.transform, new Vector2(1, 0.5f), new Vector2(0, 0), 
                new Vector3(0, transform.position.y - Utility.GetRectCornerPosition(UM.logDisplayPanel.gameObject, Direction.LeftDown).y, 0), inScreen: true);
        if(caster != null)
            UM.ActivateSkillViewer(skill, caster: caster);
        else BattleUIManager.Instance.ActivateSkillViewer(skill, casterInfo: casterInfo);
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        BattleUIManager.Instance.DeactivateSkillUI();
    }
}
public class LogExplainImage : LogExplainObject {
    public Image image;
}

public class LogExplainHealthBar : LogExplainObject, IPointerEnterHandler, IPointerExitHandler {
    public HealthViewer healthBar;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        BattleUIManager.Instance.ActivateChangeDisplayPanel(transform.position, healthBar.GetChangeText());
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        BattleUIManager.Instance.DeactivateChangeDisplayPanel();
    }
}
