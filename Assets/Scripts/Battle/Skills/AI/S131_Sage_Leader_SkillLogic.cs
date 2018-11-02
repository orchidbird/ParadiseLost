using System;
using Battle.Damage;
using System.Linq;

namespace Battle.Skills {
    class S131_Sage_Leader_SkillLogic : AttachAuraOnStart{
	    public override bool IsAuraTarget(Unit unit){return unit.CodeName.Contains("Apprentice");}
    }
}
