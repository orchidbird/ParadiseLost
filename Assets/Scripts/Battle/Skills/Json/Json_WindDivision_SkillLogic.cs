using Battle.Damage;
using System.Collections.Generic;
using UtilityMethods;

namespace Battle.Skills {
    class Json_WindDivision_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerAfterMove(Unit caster, Tile beforeTile, Tile afterTile) {
            int dist = Calculate.Distance(beforeTile.location, afterTile.location);
	        StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets(caster, caster, passiveSkill, dist, true);
        }
    }
}
