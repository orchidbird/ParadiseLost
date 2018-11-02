using Enums;

namespace Battle.Skills{
	public class S141_Armor_SkillLogic : BasePassiveSkillLogic{
		public override bool IsAuraTarget(Unit unit){
			return unit.GetSide() == Side.Enemy;
		}
	}
}
