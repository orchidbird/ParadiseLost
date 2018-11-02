using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using BattleUI;
using Object = UnityEngine.Object;

public class TutorialScenario{
	public static TutorialController TutorialController;
	public static SelectDirectionUI selectDirectionUI;

	public int index;
	public enum Mission{Standby, SelectTile, SelectTileLong, SelectDirection, SelectAnyDirection, SelectSkill, SelectSkillByKey, OpenDetailInfo, CloseDetailInfo, CancelMoveOrSkill, Collect, End }
	public Mission mission;
	public Vector3 mouseMarkPos;
	public bool IsEndMission { get { return mission == Mission.End; } }
	Direction missionDirection;
	public Action SetMissionCondition = () => {};
	public Action ResetMissionCondition = () => {};
	public string korText;
	public string engText;

	public TutorialScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		index = parser.ConsumeInt();
		mission = parser.ConsumeEnum<Mission>();
		UnityEngine.Events.UnityAction ToNextStep = () => {
			TutorialController.ToNextStep();
		};

		var BM = BattleManager.Instance;
		var TM = TileManager.Instance;
		var UIM = BattleUIManager.Instance;

		if (mission == Mission.SelectTile){
			Vector2Int missionTilePos = new Vector2Int (parser.ConsumeInt (), parser.ConsumeInt ());
			List<Tile> clickableTiles = new List<Tile>{TM.GetTile (missionTilePos)};
			SetMissionCondition = () => {
				TM.PreselectTilesForTutorial(clickableTiles);
				BM.readyCommandEvent.AddListener (ToNextStep);
				BattleData.longClickLock = true;
			};
		}else if(mission == Mission.SelectTileLong){
			Tile missionTile = TM.GetTile (new Vector2Int (parser.ConsumeInt (), parser.ConsumeInt ()));
			List<Tile> clickableTiles = new List<Tile> ();
			clickableTiles.Add (missionTile);
			SetMissionCondition = () => {
				TM.PreselectTilesForTutorial(clickableTiles);
				BM.readyCommandEvent.AddListener (ToNextStep);
				BattleData.shortClickLock = true;
			};
		}else if(mission == Mission.OpenDetailInfo){
			Tile missionTile = TM.GetTile (new Vector2Int (parser.ConsumeInt (), parser.ConsumeInt ()));
			List<Tile> clickableTiles = new List<Tile> ();
			clickableTiles.Add (missionTile);
			SetMissionCondition = () => {
				UIM.TurnOffAllActions();
				UIM.ActionButtonOnOffLock = true;
				UIM.activateDetailInfoEvent.AddListener (ToNextStep);
				TM.PreselectTilesForTutorial(clickableTiles);

				BattleData.deactivateDetailInfoLock = true;
				BattleData.detailInfoLock = false;
				BattleData.activateDetailInfoUnit = missionTile.GetUnitOnTile();
			};
		}else if (mission == Mission.CloseDetailInfo) {
			SetMissionCondition = () => {
				UIM.TurnOffAllActions();
				UIM.ActionButtonOnOffLock = true;
				BattleUIManager.Instance.deactivateDetailInfoEvent.AddListener (ToNextStep);
				BattleData.activateDetailInfoUnit = null;
				BattleData.rightClickLock = false;
			};
		}else if(mission == Mission.CancelMoveOrSkill){
			SetMissionCondition = () => {
				UIM.TurnOffAllActions();
				UIM.ActionButtonOnOffLock = true;
				selectDirectionUI.EnableAllDirection(false);
				TM.DepreselectTilesForTutorial(true);
				BM.readyCommandEvent.AddListener (ToNextStep);
				BattleData.rightClickLock = false;
			};
		}else if (mission == Mission.SelectDirection) {
			missionDirection = parser.ConsumeEnum<Direction> ();
			SetMissionCondition = () => {
				UIM.TurnOffAllActions();
				UIM.ActionButtonOnOffLock = true;
				BattleData.longClickLock = true;
				selectDirectionUI.EnableOnlyThisDirection ((Direction)((4 + (int)missionDirection + BattleData.aspect - Aspect.North) % 4));
				BM.readyCommandEvent.AddListener (ToNextStep);
			};
		} else if (mission == Mission.SelectAnyDirection) {
			SetMissionCondition = () => {
				UIM.TurnOffAllActions();
				UIM.ActionButtonOnOffLock = true;
				BattleData.longClickLock = true;
				BM.readyCommandEvent.AddListener (ToNextStep);
			};
		} else if (mission == Mission.SelectSkill || mission == Mission.SelectSkillByKey) {
            int missionSkillIndex = parser.ConsumeInt();
            SetMissionCondition = () => {
				UIM.TurnOnOnlyOneAction (missionSkillIndex);
				UIM.ControlListenerOfActionButton(missionSkillIndex, true, ToNextStep);
				TM.DepreselectTilesForTutorial(true);
				if(mission == Mission.SelectSkillByKey){
					Setting.clickEnable = false;
				} else{
					mouseMarkPos = UIM.GetActionButtonPosition(missionSkillIndex);
				}
			};
		} else if (mission == Mission.Collect){
			int missionSkillIndex = BattleData.turnUnit.GetActiveSkillList().Count+1;
			SetMissionCondition = () => {
				UIM.TurnOnOnlyOneAction (missionSkillIndex);
				UIM.ControlListenerOfActionButton(missionSkillIndex, true, ToNextStep);
				TM.DepreselectTilesForTutorial(true);
				mouseMarkPos = UIM.GetActionButtonPosition(missionSkillIndex);
			};
		}else if (mission == Mission.Standby){
            SetMissionCondition = () => {
                int standbyButtonIndex = BattleData.turnUnit.GetActiveSkillList().Count;
                mouseMarkPos = UIM.GetActionButtonPosition(standbyButtonIndex);
                UIM.TurnOnOnlyOneAction(standbyButtonIndex);
				BattleManager.Instance.readyCommandEvent.AddListener (ToNextStep);
				TM.DepreselectTilesForTutorial(true);
			};
		}

		if (parser.Remain > 1){
			korText = parser.ConsumeString();
			engText = parser.ConsumeString();
		}

		ResetMissionCondition = () => {
			BattleData.longClickLock = false;
			BattleData.shortClickLock = false;
			BattleData.rightClickLock = true;
			BattleData.detailInfoLock = true;
			BattleData.deactivateDetailInfoLock = false;
			UIM.ActionButtonOnOffLock = false;
			UIM.SetActionButtons();
			for(int i = 0; i < 8; i++){
				UIM.ControlListenerOfActionButton(i, false, ToNextStep);
			}
			BattleManager.Instance.readyCommandEvent.RemoveListener(ToNextStep);
			BattleUIManager.Instance.activateDetailInfoEvent.RemoveListener(ToNextStep);
			BattleUIManager.Instance.deactivateDetailInfoEvent.RemoveListener(ToNextStep);
			TM.SetPreselectLock(false);
			TM.DepreselectTilesForTutorial(false);
			selectDirectionUI.EnableAllDirection(true);
			Setting.clickEnable = true;
			if(TutorialController.Instance.mark != null)
				Object.Destroy (TutorialController.Instance.mark.gameObject);
		};
	}

	public void UpdateAspect(){ // direction = 1 : 반시계 방향, direction = -1 : 시계방향
		if(mission == Mission.SelectDirection){
			selectDirectionUI.EnableOnlyThisDirection((Direction)((4 + (int)missionDirection + BattleData.aspect - Aspect.North) % 4));
		}
	}
}
