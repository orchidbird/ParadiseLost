using Enums;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class Sepia_1_m_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerOnActionEnd(Unit caster) {
            List<UnitStatusEffect> statusEffectList = caster.statusEffectList;
            Direction front = caster.GetDir();
            List<Unit> units = Utility.UnitsInRange(Utility.GetStraightRange(caster.Pos, -2, -1, front).ConvertAll(pos => TileManager.Instance.GetTile(pos)));
			if(units.Any(unit => unit.IsAllyTo(caster))){
                StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
                return;
            }

            UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "신뢰의 끈");
            if (statusEffect != null)
                caster.RemoveStatusEffect(statusEffect);
        }
    }
}
