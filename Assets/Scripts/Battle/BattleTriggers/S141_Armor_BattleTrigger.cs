using System.Collections.Generic;
using System.Linq;
using Enums;
using Battle.Damage;

public class S141_Armor_BattleTrigger : BattleTrigger {
	public S141_Armor_BattleTrigger(TrigResultType resultType, StringParser commaParser) : base(resultType, commaParser) {
		TriggerAction = () => {
			SoundManager.Instance.PlaySE ("ControllerGrawl");

			Unit lever = ((UnitDestroyLog)logs.Last()).target;

			List<Unit> nearUnits = Utility.UnitsInRange(Utility.TilesInDiamondRange(lever.Pos, 1, 2, 0));
			string armorName = "armorSchmidt";
			Unit armor = nearUnits.Find(unit => unit.CodeName == armorName);
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(armor, armor.GetPassiveSkillList()[0], armor);
			string newSpriteName = "armorSchmidtActivated";
			armor.LoadSprites(newSpriteName);
		};
	}
}
