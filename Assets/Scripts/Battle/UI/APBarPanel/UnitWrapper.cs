using System;
using Enums;
using UnityEngine;
using System.Collections.Generic;

namespace BattleUI.APBarPanels{
	public class UnitWrapper{
		readonly Unit unit;
		readonly bool isPreview;

		public UnitWrapper(Unit unit, bool isPreview){
			this.unit = unit;
			this.isPreview = isPreview;
		}

		public Unit GetUnit() {return unit;}
		public GameObject GetGameObject() {return unit.gameObject;}

		// Preview unit has indication arrow, So distinguish from others
		public bool IsPreview() {return isPreview;}
		public bool WillActThisPhase{get{return UnitManager.Instance.unitsActThisPhase.Contains(unit) && unit != BattleData.turnUnit;}}
		public int GetAP{get{
			int AP = unit.GetCurrentActivityPoint();
			if (IsPreview() && BattleData.previewAPAction != null)
				AP -= BattleData.previewAPAction.requiredAP;
			return WillActThisPhase ? AP : Math.Min(AP, unit.ApDeletePoint) + (unit.IsActive ? unit.GetStat(Stat.Agility) : 0);
		}}
	}

	public class UnitWrapperFactory{
		readonly Unit selectedUnit;

		public UnitWrapperFactory(Unit selectedUnit){
			this.selectedUnit = selectedUnit;
		}

		public List<UnitWrapper> WrapUnits(List<Unit> units){
			List<UnitWrapper> wrappedUnits = new List<UnitWrapper>();
			foreach (Unit unit in units)
				wrappedUnits.Add(new UnitWrapper(unit, unit == selectedUnit));
			return wrappedUnits;
		}
	}
}
