using System.Linq;
using System.Text.RegularExpressions;
using Enums;
using UnityEngine;

namespace Battle.Skills{
    class Kashasty_Unique_SkillLogic : BasePassiveSkillLogic{
        public override void TriggerAfterCast(CastLog castLog) {
            Casting casting = castLog.casting;
            APChangeLog apChangeLog = (APChangeLog)castLog.GetEffectLogList().Find(log => log is APChangeLog && ((APChangeLog)log).unit == casting.Caster);
            float usedAP = 0;
            if(apChangeLog != null)
                usedAP = -apChangeLog.amount;
	        int count = casting.applies.Max(apply => apply.GetDamage().CountTacticalBonus);
            casting.Caster.ChangeAP((int)(usedAP * 0.05f * count));
        }
	    
	    public override void TriggerOnStageStart(Unit caster){
		    ActiveSkill skill = caster.GetActiveSkillList().Find(sk => sk.GetName() == "더블 샷");
		    if (skill == null) return;

		    skill.powerFactor = 0.8f;
		    skill.secondRange = new TileRange(RangeForm.One, 0, 0, 0);

		    if (caster.GetPassiveSkillList().Any(passive => passive.korName == "산탄 장전")){
			    skill.powerFactor *= 0.8f;
			    skill.secondRange = new TileRange(RangeForm.Square, 0, 1, 1);
		    }
		    if (caster.GetPassiveSkillList().Any(passive => passive.korName == "그물탄"))
			    skill.powerFactor *= 0.5f;
		    
		    skill.explainKor = skill.explainKor.Replace("0.8", skill.powerFactor.ToString());
		    skill.explainEng = skill.explainEng.Replace("0.8", skill.powerFactor.ToString());
	    }
    }
}
