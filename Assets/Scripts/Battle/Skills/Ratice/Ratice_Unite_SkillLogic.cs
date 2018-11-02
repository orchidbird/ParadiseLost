using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
	class Ratice_Unite_SkillLogic : BasePassiveSkillLogic{
		public override void TriggerOnActionEnd(Unit caster){
			List<Unit> nearUnits = Utility.UnitsInRange(Utility.TilesInDiamondRange(caster.Pos, 1, 2, 1));
			List<Unit> nearAllies = nearUnits.FindAll (unit => unit.IsAllyTo(caster));
			int count = nearAllies.Count;

			StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets(caster, caster, passiveSkill, count);
		}

		public override bool IsAuraTarget(Unit unit){
			return unit.IsAllyTo(passiveSkill.Owner);
		}
	}
}
