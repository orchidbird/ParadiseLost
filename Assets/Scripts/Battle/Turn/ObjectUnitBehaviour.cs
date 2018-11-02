using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn
{
	public class ObjectUnitBehaviour{
		public static IEnumerator AllObjectUnitsBehave(){
			while (true){
				//오브젝트 때문에 오브젝트가 죽을 수도 있으니 하나 행동 끝날 때마다 매번 오브젝트유닛 목록을 다시 받아온다
				Unit selectedObjectUnit = GetNotAlreadyBehavedObjectUnit (BattleData.GetObjectUnitsList());
				if (selectedObjectUnit == null) break;
				if(selectedObjectUnit.GetComponent<AI> () != null){
					BattleData.turnUnit = selectedObjectUnit;
					yield return AnObjectUnitBehave (selectedObjectUnit);
				}
				
				selectedObjectUnit.SetAlreadyBehavedObject(true);
			}
		}

		private static Unit GetNotAlreadyBehavedObjectUnit(List<Unit> objectUnits){
			Unit notAlreadyBehavedObjectUnit = null;
			foreach (var unit in objectUnits) {
				if (!unit.IsAlreadyBehavedObject ()) {
					notAlreadyBehavedObjectUnit = unit;
					break;
				}
			}
			return notAlreadyBehavedObjectUnit;
		}

		public static IEnumerator AnObjectUnitBehave(Unit objectUnit){
			BattleData.turnUnit = objectUnit;
            if (objectUnit.CodeName == "controller") {
                BattleUIManager.Instance.SlideUIsOut(Setting.slideUIFastDuration);
                yield return BattleManager.Instance.cameraMover.ZoomOutCameraToViewMap(Setting.cameraZoomDuration);
                yield return new WaitForSeconds(0.3f);
                BattleUIManager.Instance.UpdateApBarUI();
				yield return ControllerAttack (objectUnit);
                yield return new WaitForSeconds(0.3f);
                UnitManager.Instance.DeactivateAllDamageTextObjects();
                BattleUIManager.Instance.SlideUIsIn(Setting.slideUIFastDuration);
                yield return BattleManager.Instance.cameraMover.ZoomInBack(Setting.cameraZoomDuration);
            }
		}
        static GameObject controllerEffect;
		static IEnumerator ControllerAttack(Unit objectUnit){
			BattleUIManager.Instance.SetSkillNamePanelUI("침입자 제거");

			SoundManager.Instance.PlaySE("ControllerGrawl");
            
            if(controllerEffect == null) controllerEffect = Resources.Load("VisualEffect/Prefab/ControllerActive") as GameObject;
            LogManager.Instance.Record(new WaitForSecondsLog(Configuration.NPCBehaviourDuration));
			LogManager.Instance.Record(new VisualEffectLog(objectUnit, controllerEffect, Configuration.NPCBehaviourDuration * 5));

			var targets = UnitManager.GetAllUnits().FindAll(unit => unit.GetSide() == Side.Ally);
			foreach(var target in targets){
				float damageAmount = target.GetMaxHealth()*0.15f;
				target.ApplyDamageByNonCasting (damageAmount, objectUnit, true, -target.GetStat (Stat.Defense));
			}

			yield return LogManager.Instance.ExecuteLastEventLogAndConsequences();
			BattleUIManager.Instance.HideSkillNamePanelUI ();
		}		
	}
}
