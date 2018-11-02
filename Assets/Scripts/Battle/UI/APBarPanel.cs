using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using BattleUI.APBarPanels;
using Enums;
using GameData;

namespace BattleUI{
	public class APBarPanel : MonoBehaviour{
		public GameObject bigProfile;
		List<GameObject> otherProfiles = new List<GameObject>();

		private readonly int seperationSpace = 20;

		const int maxUICount = 15;
		public Sprite aiAllyBg;
		public Sprite allyBg;
		public Sprite neutralBg;
		public Sprite namedBossBg; // boss orange(for test)
		public Sprite bossBg;

		public void UpdateAPDisplay(List<Unit> allUnits){
			Debug.Assert(allUnits.Count > 0);
			//행동할 수 있고 시야에 보이는 유닛만 대기열에 표시
			var units = allUnits.FindAll(unit => unit.HasAction && !unit.IsHiddenUnderFogOfWar());
			Unit turnUnit = BattleData.turnUnit;
			SetBigProfile(turnUnit);
			UnitWrapperFactory wrapperFactory = new UnitWrapperFactory(turnUnit);
			List<UnitWrapper> otherUnits = wrapperFactory.WrapUnits(units);
			otherUnits = SortUnits(otherUnits);

			if(otherUnits.Count > maxUICount)
				otherUnits = otherUnits.GetRange(0, maxUICount);
			
			SetProfiles(otherUnits);
		}
		void SetBigProfile(Unit unit){
			bigProfile.GetComponent<Image>().enabled = true;
			bigProfile.GetComponent<Image>().sprite = VolatileData.GetSpriteOf(SpriteType.Portrait, unit.CodeName);
			GameObject apTextGO = bigProfile.transform.parent.parent.Find("apText").gameObject;
			apTextGO.GetComponent<CustomNumberText>().PrintText(unit.GetCurrentActivityPoint().ToString(), Color.white);
		}
		void SetProfiles(List<UnitWrapper> otherUnits){
			if (otherUnits.Count == 0) return;

			bool willActThisPhase_Prev = true;
			for(int index = 0; index < maxUICount; index += 1){
				otherProfiles[index].GetComponent<DefaultPosition>().ResetPosition();
				if (index >= otherUnits.Count){
					DisableProfile(otherProfiles[index].transform);
					continue;
				}
				
				GameObject profileGameObject = otherProfiles[index];
				SetProfile(profileGameObject.transform, otherUnits[index], otherUnits[index].WillActThisPhase != willActThisPhase_Prev);
				willActThisPhase_Prev = otherUnits[index].WillActThisPhase;
			}
			//Does not make space no units left in current turn.
			if(UnitManager.Instance.unitsActThisPhase.Count == 0) return;
			
			for(int index = 0; index < otherUnits.Count; index += 1)
				if(!otherUnits[index].WillActThisPhase)
					otherProfiles[index].GetComponent<RectTransform>().anchoredPosition += new Vector2(seperationSpace, 0);
		}
		private void SetProfile(Transform profile, UnitWrapper unitWrapper, bool division){
			profile.gameObject.SetActive(true);
			var portraitImage = profile.Find("UnitPortraitMask").Find("PortraitImage").GetComponent<Image>();
			
			if (!profile.Find("Frame").GetComponent<Image>().enabled){
				profile.Find("Frame").GetComponent<Image>().enabled = true;
				profile.Find("UnitPortraitMask").Find("PortraitBg").GetComponent<Image>().enabled = true;
				portraitImage.enabled = true;
			}

			Unit unit = unitWrapper.GetUnit();
			portraitImage.sprite = VolatileData.GetSpriteOf(SpriteType.Portrait, unit.CodeName);
			portraitImage.material =
				unit.IsAI && !unit.GetAI()._AIData.isActive ? Resources.Load<Material>("Shader/grayscale") : null;	
			
			profile.Find("UnitPortraitMask").Find("PortraitBg").GetComponent<Image>().sprite = BgSpriteOfUnit(unit);
			profile.GetComponent<OrderPortraitSlot> ().SetUnit (unitWrapper.GetUnit ());
			GameObject apTextGO = profile.Find("apText").gameObject;
			apTextGO.GetComponent<CustomNumberText>().PrintText(unitWrapper.GetAP.ToString(), Color.white);
			
			profile.Find("bar").gameObject.SetActive(division);
			profile.Find("arrow").GetComponent<Image>().enabled = unitWrapper.IsPreview();
			
			unit.UpdateCCIcon();
		}
		
		public Sprite BgSpriteOfUnit(Unit unit){
			if(unit.myInfo.side == Side.Ally)
				return unit.IsAI ? aiAllyBg : allyBg;
			if(unit.myInfo.side == Side.Neutral)
				return neutralBg;
			return unit.IsNamed ? namedBossBg : bossBg;
		}

		void Start(){
			for(int i = 1; i <= maxUICount; i++)
				otherProfiles.Add(transform.Find("Profiles").Find("profile" + i).gameObject);
			foreach (GameObject profile in otherProfiles)
				DisableProfile(profile.transform);
		}
		void DisableProfile(Transform profile){
			profile.GetComponent<DefaultPosition>().ResetPosition();
			
			profile.gameObject.SetActive(false);
		}
		
		List<UnitWrapper> SortUnits(List<UnitWrapper> units){
			var thisTurnUnits = units.FindAll(wrapper => wrapper.WillActThisPhase);
			var nextTurnUnits = units.FindAll(wrapper => !wrapper.WillActThisPhase);

			var result = new List<UnitWrapper>();
			result.AddRange(thisTurnUnits.OrderByDescending(wrapper => wrapper.GetAP * 200 + wrapper.GetUnit().GetStat(Stat.Agility)));
			result.AddRange(nextTurnUnits.OrderByDescending(wrapper => wrapper.GetAP * 200 + wrapper.GetUnit().GetStat(Stat.Agility)));
			return result;
		}
	}
}
