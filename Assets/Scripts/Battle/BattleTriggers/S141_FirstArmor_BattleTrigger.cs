using Enums;
using Battle.Damage;
using UnityEngine;

public class S141_FirstArmor_BattleTrigger : BattleTrigger {
	public S141_FirstArmor_BattleTrigger(TrigResultType resultType, StringParser commaParser) : base(resultType, commaParser) {
		TriggerAction = () => {
			var schmidt = UnitManager.Instance.GetAnUnit ("schmidt");
			var nearUnits = Utility.UnitsInRange(Utility.TilesInDiamondRange(schmidt.Pos, 1, 2, 0));
			Unit armor = nearUnits.Find(unit => unit.CodeName == "armorSchmidt");
			Debug.Log("갑옷이 " + armor.GetPassiveSkillList().Count + "개의 특성 보유");
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(armor, armor.GetPassiveSkillList()[0], armor);
			string newSpriteName = "armorSchmidtActivated";
			armor.LoadSprites(newSpriteName);
		};
	}
}
