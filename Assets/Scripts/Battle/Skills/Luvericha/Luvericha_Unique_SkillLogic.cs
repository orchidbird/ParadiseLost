using System.Collections.Generic;
using Enums;
using Battle.Damage;
using UtilityMethods;

namespace Battle.Skills {
    class Luvericha_Unique_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerUsingSkill(Casting casting, List<Unit> targets) {
            Unit caster = casting.Caster;

            string displayName;
            string displayNameToRemove;
            List<UnitStatusEffect> statusEffectList = new List<UnitStatusEffect>();

            if(casting.Skill.IsOffensive()){
                displayName = Language.Select("포르테", "Forte");
                displayNameToRemove = "피아노";
            }else{
                displayName = Language.Select("피아노", "Piano");
                displayNameToRemove = "포르테";
            }
            UnitStatusEffect statusEffectToRemove = caster.statusEffectList.Find(se => se.DisplayName(true) == displayNameToRemove);
            if(statusEffectToRemove != null)
                caster.RemoveStatusEffect(statusEffectToRemove);

            UnitStatusEffectInfo useInfo = passiveSkill.unitStatusEffectList.Find(se => se.DisplayName == displayName);
            UnitStatusEffect statusEffect = new UnitStatusEffect(useInfo, caster, caster, passiveSkill);
            statusEffectList.Add(statusEffect);
            StatusEffector.AttachAndReturnUSE(caster, statusEffectList, caster, false);
        }

        public override void ApplyAdditionalDamageFromCasterStatusEffect(CastingApply castingApply, StatusEffect statusEffect) {
            SkillApplyType skillApplyType = castingApply.GetSkill().GetSkillApplyType();
	        bool isStatusEffectForte = statusEffect.DisplayName(true) == "포르테";
            bool isSkillApplyTypeAttack = (skillApplyType == SkillApplyType.DamageAP || skillApplyType == SkillApplyType.DamageHealth 
                                            || skillApplyType == SkillApplyType.Debuff);
            
	        castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, isStatusEffectForte == isSkillApplyTypeAttack? 1.2f : 0.9f);
        }
    }
}
