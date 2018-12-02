using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills{
	public class BasePassiveSkillLogic: BaseCommonSkillLogic{
		public PassiveSkill passiveSkill;

		public virtual float GetAdditionalRelativePowerBonus(Unit caster)
		{
			return 1.0f;
		}

		public virtual float GetAdditionalAbsoluteDefenseBonus(Unit caster)
		{
			return 0;
		}

		public virtual float GetAdditionalAbsoluteResistanceBonus(Unit caster)
		{
			return 0;
		}

		public virtual float ApplyIgnoreDefenceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense)
		{
			return defense;
		}

		public virtual float ApplyIgnoreDefenceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense)
		{
			return defense;
		}

		public virtual float ApplyIgnoreResistanceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)
		{
			return resistance;
		}

		public virtual float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)
		{
			return resistance;
		}

		public virtual float ApplyBonusEvasionFromTargetPassive(CastingApply castingApply){
			return 0;
		}

		public virtual void TriggerApplyingHeal(CastingApply castingApply) {} //자신이 치유를 시전할 때
		public virtual void OnCastingAmountCalculation(CastingApply castingApply){} //자신이 시전자일 때 적용
		public virtual void OnReceivingAmountCalculation(CastingApply castingApply){} //자신이 Target일 때 적용

		public virtual void ApplyAdditionalDamageFromCasterStatusEffect(CastingApply castingApply, StatusEffect statusEffect) {
		}

		public virtual float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
			return 0;
		}
		public virtual string GetStatusEffectExplanation(StatusEffect statusEffect) {
			return "";
		}
		public virtual Dictionary<Vector2Int, TileWithPath> GetMovablePath(Unit unit) {
			return null;
		}

		public virtual void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit neutralizedUnit, TrigActionType actionType){}

		public virtual void TriggerOnUnitDestroy(Unit unit, Unit destroyedUnit, TrigActionType actionType) {}
		public virtual void TriggerAfterUnitsDestroyed(Unit unit) {}
		public virtual void TriggerOnEvasionEvent(Unit caster, Unit target)
		{
		}

		public virtual void TriggerActiveSkillAppliedToOwner(CastingApply castingApply)
		{		
		}

		public virtual void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target)
		{
		}

		public virtual bool IgnoreCasting(CastingApply apply, int chainCombo) {return false;}

		public virtual bool TriggerDamagedByNonCasting(Unit caster, float damage, Unit target) {
			return true;
		}
		public virtual void TriggerAfterDamaged(Unit target, int damage, Unit caster) 
		{        
		}
		public virtual void TriggerAfterDamagedByCasting(Unit target, Unit caster) 
		{        
		}
		public virtual void TriggerAfterStatusEffectAttachedByCasting(UnitStatusEffect statusEffect, Unit target, Unit caster) {
		}
		public virtual void TriggerBeforeStartChain(List<Chain> chainList, Casting casting){} //직접 연계를 발동시켜야 함
		public virtual void TriggerOnChain(Chain chain){} //연계 공격에 어떤 식으로든 참여하면 발동
		public virtual void TriggerTargetPassiveBeforeCast(Casting casting, Unit target) {
		}
		public virtual void TriggerExistingUnitPassiveBeforeCast(Casting casting, Unit skillOwner) { // 본인이 맞는 공격이 아니더라도 스테이지에 존재만 하면 영향을 미치는 특성
		}
		public virtual void TriggerExistingUnitPassiveOnDebuffAttach(UnitStatusEffect se, Unit existingUnit) { // 스테이지에 존재만 하면 모든 유닛 상태효과 부착에 영향을 미치는 특성
		}
		public virtual void TriggerAfterCast(CastLog castLog) {

		}
		public virtual float GetRetreatHPRatioOfMyTarget(Unit target){
			return Setting.retreatHPFloat;
		}
		public virtual void TriggerAfterMove(Unit caster, Tile beforeTile, Tile afterTile) {
		}

		public virtual bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target) {
			return true;
		}

		// to ignore it, return false
		public virtual bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target){return true;}

		public virtual void TriggerOnStatusEffectRemoved(UnitStatusEffect statusEffect, Unit unit)
		{
		}
		public virtual void TriggerUsingSkill(Casting casting, List<Unit> targets) {
		}
		public virtual void TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount, bool duringAIDecision) {
		}
		public virtual void TriggerWhenShieldExhaustedByDamage(Unit shieldOwner, Unit shieldCaster) {
		}
		public virtual void TriggerOnMove(Unit caster) {
		}

		public virtual void TriggerOnStageStart(Unit caster) {		
		}

		public virtual void TriggerOnPhaseStart(Unit caster, int phase) {
		}

		public virtual void TriggerOnPhaseEnd(Unit caster) {
		}    

		public virtual void TriggerOnActionEnd(Unit caster) {    
		}
		public virtual void TriggerOnRest(Unit caster) {
		}
		public virtual void TriggerOnMyTurnStart(Unit caster) {
		}
		public virtual void TriggerOnAnyTurnStart(Unit caster, Unit turnStarter) {
		}
		public virtual void TriggerOnTurnEnd(Unit caster, Unit turnEnder){}

		public virtual void TriggerOnDestroyed(Unit actor, TrigActionType destroyType, Unit destroyedUnit){
			//destroyedUnit은 항상 passiveSkill.Owner의 역할을 한다
		}
		public virtual void TriggerStatusEffectsOnRest(Unit target, UnitStatusEffect statusEffect) {
		}
		public virtual void TriggerStatusEffectsOnUsingSkill(Unit target, List<Unit> targetsOfSkill, UnitStatusEffect statusEffect) {
		}
		public virtual void TriggerStatusEffectsOnMove(Unit target, UnitStatusEffect statusEffect) {
		}
		public virtual void TriggerStatusEffectAtActionEnd(Unit target, UnitStatusEffect statusEffect) {
		}
		public virtual bool TriggerOnSteppingTrap(Unit caster, TileStatusEffect trap) {
			return true;
		}
		public virtual void TriggerOnTrapOperated(Unit unit, TileStatusEffect trap) {
		}
		public virtual void OnMyCastingApply(CastingApply castingApply){}
		public virtual void OnAnyCastingDamage(CastingApply castingApply, int chain){}
	}
}
