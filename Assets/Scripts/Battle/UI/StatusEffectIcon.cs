using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;
using System.Collections.Generic;
using GameData;
using TMPro;
using UtilityMethods;

namespace BattleUI {
    class StatusEffectIcon : MonoBehaviour {
        public StatusEffect statusEffect;
        public Image image;

        public void UpdateVisual(){
            image.sprite = statusEffect.GetSprite();
	        
	        var stackText = transform.Find("Stack").GetComponent<TextMeshProUGUI>();
	        if(statusEffect.Stack > 1 && statusEffect.GetStatusEffectType(0) != StatusEffectType.Shield)
		        stackText.text = _String.NumberToSprite(statusEffect.Stack);
		    else if(statusEffect.actuals.Count == 1 && !statusEffect.IsTypeOf(StatusEffectType.Aura) 
		            && !statusEffect.actuals[0].isPercent && statusEffect.GetAmount(0) > 0)
		        stackText.text = _String.NumberToSprite((int)statusEffect.GetAmount(0));
			else
		        stackText.text = String.Empty;
	        
	        transform.Find("Turns").GetComponent<TextMeshProUGUI>().text =
		        statusEffect.GetIsInfinite() ? String.Empty : "<sprite=" + statusEffect.Duration() + ">";
	        if (!(statusEffect is UnitStatusEffect)) return;
	        image.material = new Material(Resources.Load<Material>("Shader/GlowBorder"));
	        image.material.SetColor("_Color", _Color.FromUSE((UnitStatusEffect) statusEffect));
        }

        public void UpdatePosition(Vector3 pivot, int index) {
            float width = GetComponent<RectTransform>().sizeDelta.x;
            transform.localPosition = pivot + new Vector3(index * 1.25f * width, 0, 0);
        }

        public void TriggerPointerEnter() {
            RectPosition rectPosition = new RectPosition(transform, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector3(0, 0, 0), true);
            BattleUIManager.Instance.ActivateStatusEffectDisplayPanelAndSetText(rectPosition, statusEffect);
        }

        public void TriggerPointerExit() {
            BattleUIManager.Instance.DeactivateStatusEffectDisplayPanel();
        }
    }
}
