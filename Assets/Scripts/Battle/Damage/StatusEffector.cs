using System.Collections.Generic;
using System.Linq;
using Enums;
using Battle.Skills;
using GameData;
using UnityEngine;

namespace Battle.Damage{
	public static class StatusEffector{
		static List<UnitStatusEffectInfo> unitStatusEffectInfoList;
		static List<TileStatusEffectInfo> tileStatusEffectInfoList;

		public static List<UnitStatusEffectInfo> USEInfoList{get{
			if(unitStatusEffectInfoList == null)
				unitStatusEffectInfoList = Parser.GetParsedData<UnitStatusEffectInfo>();
			return unitStatusEffectInfoList;
		}}
		public static List<TileStatusEffectInfo> TSEInfoList{get{
			if (tileStatusEffectInfoList == null)
				tileStatusEffectInfoList = Parser.GetParsedData<TileStatusEffectInfo>();
			return tileStatusEffectInfoList;
		}}

		public static UnitStatusEffectInfo FindUSE(string skillName){
			return USEInfoList.Find(se => se.GetOriginSkillName() == skillName);
		}

		public static void AttachGeneralRestriction(StatusEffectType restrictionType, Skill originSkill, Unit target, bool byCasting, int durationPhase = 1){
			var seInfo = USEInfoList.Find(se => se.GetOriginSkillName() == "공용" && se.actuals[0].statusEffectType == restrictionType);
			Debug.Assert(seInfo != null);
			var SE = new UnitStatusEffect(seInfo, originSkill.owner, target, originSkill);
			SE.SetRemainPhase(durationPhase);
			AttachAndReturnUSE(originSkill.owner, new List<UnitStatusEffect> {SE}, target, byCasting);
		}
		
		static bool ignoredByShield(UnitStatusEffect statusEffect, Unit target){
			return statusEffect.IsDebuff && !statusEffect.IsTypeOf(StatusEffectType.Overload) && target.GetRemainShield () > 0;
		}

		static bool ignoredByBeingObject(UnitStatusEffect statusEffect, Unit target){
			return statusEffect.IsRestriction && target.IsObject; 
		}

		public static List<UnitStatusEffect> UnitStatusEffectsOfSkill(Skill skill, Unit actor, Unit target){
			List<UnitStatusEffectInfo> useInfos = skill.unitStatusEffectList;
			return useInfos.ConvertAll(info => new UnitStatusEffect(info, actor, target, skill));
		}

		static List<UnitStatusEffect> UnitStatusEffectsOfSkillOfPrecalculatedAmounts(Skill skill, Unit actor, Unit target, List<List<float>> amounts){
			List<UnitStatusEffectInfo> seInfos = skill.unitStatusEffectList;
			List<UnitStatusEffect> results = new List<UnitStatusEffect> ();
			for (int i = 0; i < seInfos.Count; i++) {
				var seInfo = seInfos [i];
				results.Add (new UnitStatusEffect (seInfo, actor, target, skill, amounts [i]));
			}
			return results;
		}

		static List<TileStatusEffect> TileStatusEffectsOfSkill(Skill skill, Unit actor, Tile target){
			return skill.tileStatusEffectList.Select(tseInfo => new TileStatusEffect(tseInfo, actor, target, skill)).ToList();
		}

		public static List<UnitStatusEffect> FindAndAttachUnitStatusEffectsToCastingTargets(CastingApply castingApply, Unit target, int chain){
			var caster = castingApply.Caster;
			var appliedSkill = castingApply.GetCasting().Skill;
			var newStatusEffects = new List<UnitStatusEffect>();
			foreach(var statusEffect in UnitStatusEffectsOfSkill(appliedSkill, caster, target)){
				if(!ignoredByShield (statusEffect, target) && !ignoredByBeingObject(statusEffect, target)
				&& SkillLogicFactory.Get(appliedSkill).TriggerStatusEffectAppliedByCasting(statusEffect, castingApply, chain)){
					newStatusEffects.Add(statusEffect);
				}
			}
			return AttachAndReturnUSE (caster, newStatusEffects, target, true);
		}

		public static List<UnitStatusEffect> FindAndSetStackUnitStatusEffectsNotToCastingTargets(Unit caster, Unit target, Skill skill, int stack, bool add = false, bool byAura = false, bool byCasting = false){
			var newStatusEffects = new List<UnitStatusEffect>();
			foreach (var statusEffect in UnitStatusEffectsOfSkill(skill, caster, target)){
				if (byAura && statusEffect.IsAura() 
				    || ignoredByShield(statusEffect, target) || ignoredByBeingObject(statusEffect, target)
				    || skill is PassiveSkill && !SkillLogicFactory.Get((PassiveSkill)skill).TriggerStatusEffectApplied(statusEffect, caster, target)) continue;
				
				var alreadyAttachedSE = target.statusEffectList.Find (se => se.IsSameStatusEffect(statusEffect));
				if (alreadyAttachedSE != null) {
					if (add)
						alreadyAttachedSE.AddRemainStack (stack);
					else
						alreadyAttachedSE.SetRemainStack (stack);
					alreadyAttachedSE.RefillDuration();
				}else if (stack != 0){
					statusEffect.SetRemainStack (stack);
					newStatusEffects.Add (statusEffect);	
				}
			}
			return AttachAndReturnUSE(caster, newStatusEffects, target, byCasting);
		}

		public static List<UnitStatusEffect> FindAndAttachUnitStatusEffectsToCastingTargets(Unit caster, PassiveSkill appliedSkill, Unit target){
			var newStatusEffects = new List<UnitStatusEffect>();
			foreach (var statusEffect in UnitStatusEffectsOfSkill(appliedSkill, caster, target)){
				if(!ignoredByShield (statusEffect, target) && !ignoredByBeingObject(statusEffect, target)
				&& SkillLogicFactory.Get(appliedSkill).TriggerStatusEffectApplied(statusEffect, caster, target)){
					newStatusEffects.Add(statusEffect);
				}
			}
			return AttachAndReturnUSE(caster, newStatusEffects, target, false);
		}

		public static List<UnitStatusEffect> FindAndAttachUnitStatusEffectsOfPrecalculatedAmounts(Unit caster, Skill appliedSkill, Unit target, List<List<float>> amounts){
			var newStatusEffects = new List<UnitStatusEffect>();
			var skillStatusEffects = UnitStatusEffectsOfSkillOfPrecalculatedAmounts (appliedSkill, caster, target, amounts);
			for(int i = 0; i<skillStatusEffects.Count;i++){
				var se = skillStatusEffects [i];
				if(!ignoredByShield (se, target) && !ignoredByBeingObject(se, target))
					newStatusEffects.Add (se);
			}
			return AttachAndReturnUSE(caster, newStatusEffects, target, false);
		}

		public static void FindAndAttachTileStatusEffects(Unit caster, ActiveSkill appliedSkill, Tile targetTile) {
			var newStatusEffects = new List<TileStatusEffect>();
			foreach (var statusEffect in TileStatusEffectsOfSkill(appliedSkill, caster, targetTile)){
				if(SkillLogicFactory.Get(appliedSkill).TriggerTileStatusEffectApplied(statusEffect)){
					newStatusEffects.Add(statusEffect);
				}
			}
			AttachStatusEffectOnTile(caster, newStatusEffects, targetTile);
		}
		public static void FindAndAttachTileStatusEffects(Unit caster, PassiveSkill appliedSkill, Tile targetTile) {
			List<TileStatusEffect> newStatusEffects = new List<TileStatusEffect>();
			foreach(var statusEffect in TileStatusEffectsOfSkill(appliedSkill, caster, targetTile))
				newStatusEffects.Add (statusEffect);
			AttachStatusEffectOnTile(caster, newStatusEffects, targetTile);
		}

		public static List<UnitStatusEffect> AttachAndReturnUSE(Unit caster, List<UnitStatusEffect> statusEffects, Unit target, bool byCasting){
			var logManager = LogManager.Instance;
			var validStatusEffects = new List<UnitStatusEffect>();
			var actuallyAppliedStatusEffects = new List<UnitStatusEffect>();
			
			foreach (var statusEffect in statusEffects)
				if (!(statusEffect.IsTypeOf(StatusEffectType.Overload) && caster != target))
					validStatusEffects.Add(statusEffect);

			bool isBuffByCasting = false; // 의지 변동을 위해 확인. 여러 효과 중 하나라도 {강화이며 && 기술(ActiveSkill)로 받은 효과}이면 true

			foreach (var statusEffect in validStatusEffects){
				if (ignoredByShield (statusEffect, target) && !ignoredByBeingObject(statusEffect, target))
					continue;				
				var targetPassiveSkills = target.GetPassiveSkillList();
				if(!SkillLogicFactory.Get(targetPassiveSkills).WillReceiveSE(statusEffect, caster, target))
					continue;

				if (statusEffect.IsDebuff)
					foreach (Unit unit in UnitManager.GetAllUnits())
						unit.GetListPassiveSkillLogic ().TriggerExistingUnitPassiveOnDebuffAttach (statusEffect, unit);
				// 위 TriggerExistingUnitPassiveOnDebuffAttach에서 약화강화로 수치가 달라질 수 있으니 아래 TriggerAfterStatusEffectAttachedByCasting와 순서를 바꾸면 안 됨
				if (byCasting)
					SkillLogicFactory.Get (targetPassiveSkills).TriggerAfterStatusEffectAttachedByCasting (statusEffect, target, caster);
				
				if (statusEffect.IsBuff && byCasting)
					isBuffByCasting = true;

                // 동일한 효과가 이미 있을 때 갱신하는 작업은 이제 로그를 실제로 실행할 때 함.
				var oldSameEffect = target.statusEffectList.Find(se => se.IsSameStatusEffect(statusEffect));
				if (oldSameEffect != null && oldSameEffect.IgnoreNewEffect(statusEffect))
					continue;
				
				actuallyAppliedStatusEffects.Add(statusEffect);
				logManager.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Attach, 0, 0, 0));
			}

			if (isBuffByCasting && caster != target && !target.IsObject){
				caster.ChangeWill (WillChangeType.Saviour, BigSmall.None);
				target.ChangeWill (WillChangeType.Cooperative, BigSmall.None);
			}

			return actuallyAppliedStatusEffects;
		}

		public static void AttachStatusEffectOnTile(Unit caster, List<TileStatusEffect> statusEffects, Tile targetTile) {
            // 동일한 효과가 이미 있을 때 갱신하는 작업은 이제 로그를 실제로 실행할 때 함.
            foreach (var statusEffect in statusEffects)
				LogManager.Instance.Record(new StatusEffectLog(statusEffect, StatusEffectChangeType.Attach, 0, 0, 0));
		}
	}
}
