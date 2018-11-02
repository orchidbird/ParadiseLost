using Battle.Damage;
using Enums;
using UnityEngine;
namespace Battle.Skills {
		public class AttachStatusEffectToDestroyer : BasePassiveSkillLogic {
		public override void TriggerOnDestroyed (Unit actor, TrigActionType reason, Unit destroyedUnit){
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets (destroyedUnit, passiveSkill, actor);
		}
	}
}
