using System;

namespace Battle.Skills {
    class Json_3_l_SkillLogic : BasePassiveSkillLogic{
	    public override void TriggerAfterCast(CastLog castLog){
		    var casting = castLog.casting;
			if (casting.Caster.CodeName == "json" && casting.GetTargets ().Exists (target => target.statusEffectList.Exists (SE => SE.DisplayName (true) == "표식" && SE.Stack >= 2)))
				casting.Caster.ChangeAP (4);
	    }
    }
}
