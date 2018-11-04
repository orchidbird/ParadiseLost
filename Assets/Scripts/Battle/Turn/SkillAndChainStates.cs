using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using GameData;
using UtilityMethods;

namespace Battle.Turn {
	public class SkillAndChainStates {
		public static IEnumerator SelectSkillApplyArea(){
			var UIM = BattleUIManager.Instance;
			UIM.DeactivateNonSkillButtons ();

			var originalDir = BattleData.turnUnit.GetDir();
			BattleData.currentState = CurrentState.SelectSkillApplyArea;
			var TM = TileManager.Instance;
			var LM = LogManager.Instance;
			var BM = BattleManager.Instance;
			var caster = BattleData.turnUnit;
			var triggers = BattleData.triggers;
			var skill = BattleData.selectedSkill;

			TutorialManager.Instance.Activate(skill.GetRangeType() == RangeType.Point ? "RangeType_Point" : "RangeType_Auto");
			TutorialManager.Instance.ReserveTutorial("Preview");
			if (skill.GetRangeType() == RangeType.Route)
				TutorialManager.Instance.Activate("Charge");

			UIM.PreviewAp(new APAction(APAction.Action.Skill, caster.GetActualRequireSkillAP(skill)));

			while(BattleData.currentState == CurrentState.SelectSkillApplyArea){          
				BM.SetProperVisualState();

				//마우스 방향을 돌릴 때마다 그에 맞춰서 빨간 범위 표시도 업데이트하고 유닛 시선방향 돌리고 데미지 프리뷰와 2차범위 표시도 업데이트
				IEnumerator forUpdate = UpdateRangePreview(originalDir);
				BM.StartCoroutine(forUpdate);

				BattleData.isWaitingUserInput = true;
				yield return EventTrigger.WaitOr(
					triggers.rightClicked, triggers.skillSelected,
					triggers.directionSelected, triggers.directionLongSelected,
					triggers.tileSelected, triggers.tileLongSelected
				);
				//Debug.Log("기술 선택: " + triggers.skillSelected.Triggered + " / 발동: " + triggers.tileSelected.Triggered);
				BattleData.isWaitingUserInput = false;
				BM.StopCoroutine(forUpdate);

				if (triggers.rightClicked.Triggered){
					SoundManager.Instance.PlaySE ("Cancel");
					caster.SetDirection(originalDir);
					TM.DepreselectAllTiles();
					UIM.SetDirectionUIActive(false);
					BattleData.currentState = CurrentState.FocusToUnit;
					BattleData.isWaitingUserInput = false;
					continue;
				}

				var targetTile = BattleData.SelectedTile;
				var skillLocation = new SkillLocation (caster.TileUnderUnit, targetTile, caster.GetDir ());    	    
				skillLocation.SetRealTargetTile(skill); //지정형 기술이 아닐 경우 targetTile이 없으므로 재지정.
				var casting = new Casting (caster, BattleData.selectedSkill, skillLocation);

				if (!CheckApplyPossibleToTargetTiles (targetTile, casting)) {
					SoundManager.Instance.PlaySE ("Cannot");
					continue;
				}

				if(!casting.IsValid && !Configuration.ignoreWarning[Warning.WarningType.InvalidCasting] && 
					casting.Skill.GetSkillApplyType() != SkillApplyType.Trap && !triggers.skillSelected.Triggered){
					BattleUIManager.Instance.Warn(3);
					yield return EventTrigger.WaitOr(triggers.castCheckYesClicked, triggers.castCheckNoClicked);
					if(!triggers.castCheckYesClicked.Triggered)
						continue;
				}

				if(triggers.skillSelected.Triggered)
					yield return SelectSkillApplyArea();
				else if (triggers.tileSelected.Triggered || triggers.directionSelected.Triggered){
					BattleData.currentState = CurrentState.AfterAction;
					LM.Record(new CastLog(casting));
					HideAfterImages();
					yield return ApplyCasting (casting);
				}else if(triggers.tileLongSelected.Triggered || triggers.directionLongSelected.Triggered) {
					BattleData.currentState = CurrentState.AfterAction;
					if(CheckWaitChainPossible (casting)){
						LM.Record(new ChainLog(casting));
						WaitChain (casting);
					}else{
						LM.Record(new CastLog(casting));
						yield return ApplyCasting (casting);
					}
				}
			}
			HideCastingPreview();
		}

		public static void HideAfterImages(){
			foreach (KeyValuePair<Unit, PreviewState> kv in unitPreviewDict)
				kv.Key.HideAfterImage();
		}

		static IEnumerator UpdateRangePreview(Direction originalDir){
			var TM = TileManager.Instance;
			var caster = BattleData.turnUnit;
			Tile prevMouseOverTile = null;
			var skill = BattleData.selectedSkill;
			var beforeDirection = originalDir;
			unitPreviewDict = new Dictionary<Unit, PreviewState>();
			triggeredUnits = new List<Unit>();

			for(int i = 0; ; i++){
				yield return null;
				var newOverTile = TM.preSelectedMouseOverTile;
				var newDir = skill.GetRangeType() == RangeType.Point
					? Calculate.GetDirectionTo(caster, newOverTile, originalDir)
					: Calculate.GetMouseDirFromUnit(caster, originalDir);
				
				if (i > 0 && newOverTile == prevMouseOverTile && beforeDirection == newDir) continue;
				
				beforeDirection = newDir;
				prevMouseOverTile = newOverTile;
				caster.SetDirection(newDir);
				HideCastingPreview();

				if (skill.GetRangeType() == RangeType.Point) {
					if (newOverTile == caster.TileUnderUnit)
						caster.SetDirection (originalDir);
					if (newOverTile == null) {
						ClearRangeAndCastingPreview (null, newDir);
					} else {
						RenewHealthPreview (caster, newOverTile, newDir);
					}
				}else
					RenewHealthPreview(caster, caster.TileUnderUnit, newDir);
			}
		}

		public static void RenewHealthPreview(Unit caster, Tile targetTile, Direction dir){
			var TM = TileManager.Instance;
			var skill = BattleData.selectedSkill;
			var location = new SkillLocation(caster.TileUnderUnit, targetTile, dir);
			location.SetRealTargetTile(skill); //경로형일 경우 충돌지점을 Location의 TargetTile로 지정
			var casting = new Casting(caster, skill, location);
			ClearRangeAndCastingPreview (casting, dir);
			if(skill.GetRangeType() != RangeType.Auto) //자동이 아닐 경우 유효범위 따로 표시
				TM.PaintTiles(casting.RealRange, TM.TileColorMaterialForRange2);

			if(CheckApplyPossibleToTargetTiles(targetTile, casting))
				DisplayCastingPreview(casting); //새 시전 미리보기 표시를 출력
		}

		static void ClearRangeAndCastingPreview(Casting casting, Direction dir){
			var TM = TileManager.Instance;
			var skill = BattleData.selectedSkill;
			if (skill == null) return;

			if (skill.GetRangeType() == RangeType.Auto)
				TM.PaintTiles(casting.RealRange, TM.TileColorMaterialForRange2);
			else
				TM.PaintTiles(skill.GetTilesInFirstRange(BattleData.turnUnit.Pos, dir), TM.TileColorMaterialForRange1);
		}

		//기술 시전 미리보기 표시
		static void DisplayCastingPreview(Casting casting) {
			List<Chain> triggeredChains = ChainList.GetAllChainTriggered(casting);
			triggeredUnits = new List<Unit>();
			foreach(Chain chain in triggeredChains){
				var caster = chain.Caster;
				if (caster == casting.Caster) continue; // 현재 턴인 유닛 제외
				triggeredUnits.Add(caster);  // 나중에 연계표시 지우기 위해 저장
				caster.PreviewChainTriggered(true);
			}
			unitPreviewDict = GetCastingResultPreview(triggeredChains, false);
			foreach (KeyValuePair<Unit, PreviewState> kv in unitPreviewDict){
				kv.Key.ShowBonusIcons(kv.Value.AttackDamage);
				kv.Key.healthViewer.gameObject.SetActive(true);
				kv.Key.healthViewer.Preview((int)kv.Value.unitHp, (int)kv.Value.unitShield, casting);
				kv.Key.PreviewCCIcon(kv.Value.ccLifeChangeAmount);
				if (!kv.Key.IsHiddenUnderFogOfWar())
					kv.Key.SetAfterImageAt(kv.Value.position, kv.Value.direction);
				kv.Key.preview = null;
			}
		}

		static void CastAll(List<Chain> allChains, bool duringAIDecision){//미리보기 전용.
			foreach(Chain chain in allChains){
				LogManager.Instance.Record(new CastLog(chain.Casting));                  // EventLog를 남긴다.
				chain.Casting.Cast(allChains.Count, duringAIDecision, isPreview: true);  // ApplyCasting한다. 로그만 남기므로 실제로 전투에 영향을 미치지 않는다.
			}
		}
		//시전의 모든 결과 미리얻기
		public static Dictionary<Unit, PreviewState> GetCastingResultPreview(List<Chain> triggeredChains, bool duringAIDecision){ //duringAIDecision: 반사 피해를 고려하지 않도록 하기 위함(18.08.28)
			var LM = LogManager.Instance;
			LogManager.SetDuringPreview (true);
			CastAll(triggeredChains, duringAIDecision);

			var neutralizedUnits = CullPreviewFromEventLog(LM.GetLastEventLog().GetEffectLogList())
								   .Where(kv => kv.Value.unitHp < kv.Key.RetreatHP).Select(x => x.Key).ToList();
			foreach (var unit in neutralizedUnits)
				LM.Record(new UnitDestroyLog(unit, TrigActionType.Neutralize));
			
			var result = CullPreviewFromEventLog(LM.GetLastEventLog().GetEffectLogList());
			for(int i = 0; i < triggeredChains.Count; i++) //연계로 생성된 로그들 삭제.
				LM.RemoveLastEventLog();
			LogManager.SetDuringPreview (false);
			if(duringAIDecision)
				foreach (var kv in result)
					kv.Key.preview = null;
			
			return result;
		}

		static Dictionary<Unit, PreviewState> CullPreviewFromEventLog(List<EffectLog> logs){
			var result = new Dictionary<Unit, PreviewState>();
			foreach(var effectLog in logs){
				Unit unit = null;
				if(effectLog is HPChangeLog)        unit = ((HPChangeLog)effectLog).target;
				if(effectLog is StatusEffectLog)    unit = ((StatusEffectLog)effectLog).GetOwner();
				if(effectLog is PositionChangeLog)  unit = ((PositionChangeLog)effectLog).target;
				if (unit == null) continue;

				if (!result.ContainsKey(unit)){
					var preview = new PreviewState(unit);
					result.Add(unit, preview);
					unit.preview = preview;
				}
				result[unit].ApplyEffectLog(effectLog);
			}
			
			return result;
		}

		public static bool CheckApplyPossibleToTargetTiles(Tile targetTile, Casting casting) {
			ActiveSkill skill = casting.Skill;
			bool clickedTileOutOfRange = skill.GetRangeType () == RangeType.Point && !casting.FirstRange.Contains (targetTile);
			return !clickedTileOutOfRange && CheckApplyPossibleToTargetTilesForAI (casting);
		}
		public static bool CheckApplyPossibleToTargetTilesForAI(Casting casting) {
			return casting.Skill.SkillLogic.CheckApplyPossibleToTargetTiles (casting);
		}
		public class PreviewState {
			public Unit target;
			public float unitHp;
			public float unitShield;
			public Vector2Int position;
			public Direction direction;
			public DamageCalculator.AttackDamage AttackDamage;
			public Dictionary<StatusEffectType, int> ccLifeChangeAmount = new Dictionary<StatusEffectType, int> ();
			public List<UnitStatusEffect> newStatusEffects = new List<UnitStatusEffect>();
			public PreviewState(Unit target) {
				this.target = target;
				position = target.TileUnderUnit.location; //대형 유닛의 경우 target.Pos와 다름!
				direction = target.GetDir();
				unitHp = target.hp;
				unitShield = target.GetRemainShield();
				foreach (var ccType in EnumUtil.ccTypeList)
					ccLifeChangeAmount[ccType] = 0;
			}
			public void ApplyEffectLog(EffectLog log){
				//Debug.Log(log.GetText() + "를 미리보기에 적용");
				float damageAmount = 0;
				float shieldChange = 0;
				if(log is HPChangeLog){
					damageAmount = -((HPChangeLog)log).amount;
					if(((HPChangeLog)log).caster == BattleData.turnUnit)
						AttackDamage = ((HPChangeLog) log).damage;
				}else if(log is StatusEffectLog){
					StatusEffectLog sLog = (StatusEffectLog)log;
					shieldChange = sLog.GetShieldChangeAmount();
					if(sLog.statusEffect.IsTypeOf(StatusEffectType.RetreatHPChange))
						newStatusEffects.Add((UnitStatusEffect)sLog.statusEffect);
					foreach(StatusEffectType ccType in EnumUtil.ccTypeList)
						ccLifeChangeAmount [ccType] += sLog.GetCCRemainPhaseChangeAmount(ccType);
				}else if(log is PositionChangeLog)
					position = ((PositionChangeLog)log).afterPos;

				unitShield = Math.Max(unitShield + shieldChange, 0);
				unitHp = damageAmount > 0 ? Math.Max(unitHp - damageAmount, 0) : Math.Min (unitHp - damageAmount, target.GetMaxHealth ());
			}
		}

		static Dictionary<Unit, PreviewState> unitPreviewDict = new Dictionary<Unit, PreviewState>();
		static List<Unit> triggeredUnits = new List<Unit>();

		public static void HideCastingPreview(){
			// 기술시전 미리보기 해제.
			TileManager.Instance.ClearAllTileColors();
			foreach(Unit caster in triggeredUnits)
				caster.PreviewChainTriggered(false);
			triggeredUnits.Clear();
			foreach (KeyValuePair<Unit, PreviewState> kv in unitPreviewDict) {
				Unit unit = kv.Key;
				kv.Key.HideBonusIcons();
				if (unit.GetComponentInChildren<HealthViewer>() != null) {
					unit.GetComponentInChildren<HealthViewer>().CancelPreview();
					unit.CancelPreviewCCIcon();
					unit.HideAfterImage();
				}
				unit.CheckAndHideObjectHealth();
			}
		}
		public static void BeforeCastingRoutine(Casting casting){
			Unit caster = casting.Caster;
			ActiveSkill skill = casting.Skill;
			caster.BreakCollecting();
			caster.UseActivityPoint (casting.RequireAP);
			skill.SkillLogic.AttachOverload (caster);
			if (skill.GetCooldown () > 0)
				LogManager.Instance.Record(new CoolDownLog(caster, skill));
		}
		public static IEnumerator ApplyCasting (Casting casting) {
			BeforeCastingRoutine(casting);
			yield return ApplyAllTriggeredChains(casting);
		}
		public static void WaitChain (Casting casting) {
			Unit caster = casting.Caster;
			caster.SetDirection(casting.Location.Dir);
			
			// 체인 목록에 추가.
			ChainList.AddChains(casting);
			BattleData.selectedSkill = null;
		}
		//연계'대기' 가능한 상태인가?
		private static bool CheckWaitChainPossible (Casting casting) {
			if (!casting.Skill.IsOffensive ()) return false;

			var caster = casting.Caster;
			// 타일 조건 - 시전자가 있는 타일에 연계 대기 불가능 효과가 걸려있으면 연계대기 불가
			bool tileStatusConditionPossible = true;
			Tile tileUnderCaster = caster.TileUnderUnit;
			foreach(var tileStatusEffect in tileUnderCaster.GetStatusEffectList()) {
				Skill originSkill = tileStatusEffect.GetOriginSkill();
				if (originSkill is ActiveSkill &&
				 	!((ActiveSkill)originSkill).SkillLogic.TriggerTileStatusEffectWhenUnitTryToChain(tileUnderCaster, tileStatusEffect))
					tileStatusConditionPossible = false;
			}

			return tileStatusConditionPossible;
		}

		public static float longestShowTimeOfSkill;
		public static IEnumerator ApplyAllTriggeredChains (Casting casting) {
			var logManager = LogManager.Instance;
			// 현재 시전으로 발동되는 모든 시전의 리스트(현재 시전 포함)를 받는다.
			// 연계발동 불가능한 스킬일 경우엔 ChainList.GetAllChainTriggered에서 현재 시전만 담은 리스트를 반환하므로 걍 그 스킬만 시전되고 끝난다
			var allTriggeredChains = ChainList.GetAllChainTriggered (casting);
			int chainCombo = allTriggeredChains.Count;

			casting.Caster.GetListPassiveSkillLogic().TriggerBeforeStartChain(allTriggeredChains, casting);
			foreach (var chain in allTriggeredChains)
				chain.Caster.GetListPassiveSkillLogic().TriggerOnChain(chain);

			if (chainCombo > 1 && casting.Caster.IsPC)
				yield return ShowChainCutScene(allTriggeredChains);
			
			// 발동되는 모든 시전을 순서대로 실행
			longestShowTimeOfSkill = 0f;
			foreach (var chain in allTriggeredChains){
				float skillShowTime;
				if (chain.Skill.castVeName == "-")
					skillShowTime = 0f;
				else{
					var VEPrefab = VolatileData.GetVisualEffectPrefab (chain.Skill.castVeName);
					skillShowTime = VEPrefab == null ? 0f : EffectPlayer.GetEffectDuration(casting, true);
				}
				
				if (skillShowTime > longestShowTimeOfSkill)
					longestShowTimeOfSkill = skillShowTime;
				if (chain != allTriggeredChains.First())
					logManager.Record(new CastLog(chain.Casting));
				chain.Cast (chainCombo, false);
				
				if(chainCombo > 1 && chain.Caster.IsPC)
					logManager.Record(new WillChangeLog(chain.Caster, chainCombo, WillChangeType.Chain));
			}
			yield return logManager.ExecuteLastEventLogAndConsequences();
		}

		static IEnumerator ShowChainCutScene(List<Chain> chains){
			var CutSceneHolder = GameObject.Find("CutScenes");
			CutSceneHolder.GetComponent<Image>().enabled = true;

			var timeInterval = 0.05f;
			var waitBetweenAttackers = new WaitForSeconds(timeInterval);
			var unitsToShow = chains.FindAll(chain => chain.Caster.IsPC).ConvertAll(chain => chain.Caster);
			for (int i = 0; i < unitsToShow.Count; i++){
				var CutScene = GameObject.Instantiate(Resources.Load<ChainCutScene>("Battle/CutScene"), CutSceneHolder.GetComponent<RectTransform>());
				CutScene.rectTransform.anchoredPosition += 
					new Vector2(-(CutScene.rectTransform.rect.width + 640), CutScene.rectTransform.rect.height * ((unitsToShow.Count - 1) / 2f - i));
				CutScene.StartCoroutine(CutScene.Act(unitsToShow[i]));
				yield return waitBetweenAttackers;
			}
			
			yield return new WaitForSeconds(ChainCutScene.moveDuration * 2 + ChainCutScene.waitTime - timeInterval);
			UI.DestroyAllChilds(CutSceneHolder.transform);
			CutSceneHolder.GetComponent<Image>().enabled = false;
		}
	}
}
