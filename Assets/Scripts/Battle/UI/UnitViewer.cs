using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Enums;
using GameData;
using UtilityMethods;

// This component is used in two UI : SelectedUnitViewer and UnitViewer.
namespace BattleUI{
    public class UnitViewer : UnitInfoUI{
        public Image unitImage;       
        public GameObject statusEffectIconPrefab;
        public GameObject statusEffectIconsParent;
        List<StatusEffectIcon> statusEffectIcons = new List<StatusEffectIcon>();
        public Image HpBar;
        public Image CurrentApBar;
        public Image NextApBar;
        public Image AfterApBar;
	    public bool isSelectedUnitViewer;

        public void UpdateUnitViewer(Unit unit){
            this.unit = unit;
            unit.UpdateStats();
	        unitImage.sprite = VolatileData.GetSpriteOf(SpriteType.Portrait, unit.CodeName);
	        if (unitImage.sprite.name == "notfound")
		        unitImage.sprite = unit.Sprites[Direction.LeftDown];
            UpdateHpBar(unit);
            UpdateApBar(unit);
            UpdateEffect(unit);
            BattleUIManager.Instance.DeactivateStatusEffectDisplayPanel();
            SetCommonUnitInfoUI();
        }

        public void ClearStatusEffectIconList() {
            foreach(var statusEffectIcon in statusEffectIcons)
                Destroy(statusEffectIcon.gameObject);
            statusEffectIcons.Clear();
        }

        void UpdateEffect(Unit unit){
            ClearStatusEffectIconList();
	        List<UnitStatusEffect> effectList = unit.VaildStatusEffects;
            int numberOfEffects = effectList.Count;

            for (int i = 0; i < numberOfEffects; i++){
                StatusEffectIcon statusEffectIcon = Instantiate(statusEffectIconPrefab, statusEffectIconsParent.transform).GetComponent<StatusEffectIcon>();
                statusEffectIcon.statusEffect = effectList[i];
                statusEffectIcon.UpdateVisual();
                float width = statusEffectIcon.GetComponent<RectTransform>().sizeDelta.x;
                statusEffectIcon.transform.localPosition = new Vector3(i * 1.25f * width, 0, 0);
                statusEffectIcons.Add(statusEffectIcon);
		        statusEffectIcon.GetComponent<Image>().raycastTarget = isSelectedUnitViewer;
            }
        }

        void UpdateHpBar(Unit unit){
			HpBar.color = HealthViewer.HpColorOfUnit(unit);
            HpBar.fillAmount = unit.GetHpRatio();
        }

        void UpdateApBar(Unit unit) {
            CurrentApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint());
            AfterApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint());
            NextApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint()+unit.GetStat(Stat.Agility));
        }

        public void PreviewAp(int costAP){
	        if (!unit.IsPC) return;
            AfterApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint() - costAP);
	        ApText.text = "<color=red>" + (unit.GetCurrentActivityPoint() - costAP) + "</color> / " + unit.GetStat(Stat.Agility);
	        BattleData.turnUnit.activeArrowIcon.transform.Find("ApText").GetComponent<TextMesh>().text = (unit.GetCurrentActivityPoint() - costAP).ToString();
        }

        public void OffPreviewAp(){
            AfterApBar.fillAmount = CurrentApBar.fillAmount;
	        ApText.text = _String.Fraction(unit.GetCurrentActivityPoint(), unit.GetStat(Stat.Agility));
	        BattleData.turnUnit.activeArrowIcon.transform.Find("ApText").GetComponent<TextMesh>().text = "";
        }
    }
}
