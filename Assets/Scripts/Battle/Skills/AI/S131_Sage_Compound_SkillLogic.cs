using Battle.Damage;

namespace Battle.Skills {
    class S131_Sage_Compound_SkillLogic : BasePassiveSkillLogic {
	    public override void TriggerOnMyTurnStart(Unit caster){
		    StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
	    }
        /*public override void TriggerOnPhaseStart(Unit caster, int phase){
	        if (!UnitManager.GetAllUnits().Any(unit => unit.CodeName.Contains("door0"))) return;
	        
	        if ((phase - 1) % 3 == 0 && phase != 1) {
		        StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
	        }
        }*/
    }
}
