using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UtilityMethods;

namespace BattleUI {
    public class StatusEffectDisplayPanel : MonoBehaviour {
        public Text displayName;
        public Text CasterName;
		public TextMeshProUGUI explanation;
        
        public void SetText(StatusEffect statusEffect){
	        displayName.text = statusEffect.DisplayName(false);
	        if (statusEffect is UnitStatusEffect)
		        displayName.color = _Color.FromUSE((UnitStatusEffect) statusEffect);
	        
	        CasterName.text = "(" + UnitInfo.ConvertName(statusEffect.GetCaster().CodeName) + ")";
            
            explanation.text = statusEffect.GetExplanation();
			if (statusEffect is UnitStatusEffect && statusEffect.IsTypeOf(Enums.StatusEffectType.EvasionChange))
				explanation.text += "(<color=orange>총 " + (int)(((UnitStatusEffect)statusEffect).GetOwner().GetEvasionChance() * 100) + "%</color>)";
        }
    }
}
