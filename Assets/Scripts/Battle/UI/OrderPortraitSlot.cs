using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderPortraitSlot : BattleUI.DefaultPosition, IPointerEnterHandler, IPointerExitHandler{
	Unit unit;
	bool isHighlighting;
	public Image RestrictionIcon;
		
	public void SetUnit(Unit newUnit){
		if (unit != newUnit) {
			if (unit != null) {
				if (unit.GetOrderPortraitSlot () == this)
					unit.SetOrderPortraitSlot (null);
			}
			if (isHighlighting) {
				if (unit != null && unit != BattleData.turnUnit)
					unit.SetMouseOverHighlightBorder (false);
				HighlightPortrait (false);
			}
		}
		unit = newUnit;
		if (newUnit != null)
			newUnit.SetOrderPortraitSlot (this);
	}

	public void HighlightPortrait(bool onoff){
		if (onoff) {
			isHighlighting = true;
			Instantiate (Resources.Load<GameObject> ("VisualEffect/Prefab/HighlightPortraitFrame"), transform);
		} else {
			isHighlighting = false;
			ParticleSystem[] particles = transform.GetComponentsInChildren<ParticleSystem> ();
			foreach(var particle in particles) Destroy(particle.gameObject);
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerData){
		if(unit == null || unit == BattleData.turnUnit || unit.IsHiddenUnderFogOfWar()) return;
		
		unit.SetMouseOverHighlightBorder (true);
		HighlightPortrait (true);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData pointerData){
		if(unit == null || unit == BattleData.turnUnit) return;

		unit.SetMouseOverHighlightBorder (false);
		HighlightPortrait (false);
	}
}
