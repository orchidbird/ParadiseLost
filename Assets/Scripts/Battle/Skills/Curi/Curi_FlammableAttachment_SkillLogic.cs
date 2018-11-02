using System.Collections;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
    public class Curi_FlammableAttachment_SkillLogic : BaseActiveSkillLogic {
        public override void TriggerStatusEffectAtActionEnd(Unit target, UnitStatusEffect statusEffect) { //'가연성 부착물' 스킬로직
	        if (statusEffect.DisplayName(true) != "가연성 부착물") return;
	        
	        float damage = statusEffect.GetAmountOfType(StatusEffectType.Etc);

	        List<Unit> damagedUnitList = Utility.UnitsInRange(Utility.TilesInDiamondRange(target.Pos, 0, 1, 0));
	        foreach (var secondaryTarget in damagedUnitList)
		        secondaryTarget.ApplyDamageByNonCasting(damage, statusEffect.GetCaster(), true);
	        target.RemoveStatusEffect(statusEffect);
        }
    }
}
