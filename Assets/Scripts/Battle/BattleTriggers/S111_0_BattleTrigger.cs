using System.Collections.Generic;
using System.Linq;
using Enums;

class S111_0_BattleTrigger : BattleTrigger {
    // Stage 11에서 jailLever가 파괴되면 doorJail들이 모두 열리는(사라지는) 트리거
    public S111_0_BattleTrigger(TrigResultType resultType, StringParser commaParser) : base(resultType, commaParser) {
        TriggerAction = () => {
            LogManager logManager = LogManager.Instance;
			List<Unit> bars = UnitManager.GetAllUnits().FindAll(unit => unit.CodeName == "doorJail");
            foreach (var bar in bars) {
                logManager.Record(new UnitDestroyLog(bar, TrigActionType.Kill));
			}
			List<Unit> prisoners = UnitManager.GetAllUnits().FindAll(unit => unit.CodeName == "prisoner");
			foreach(Unit prisoner in prisoners){
				prisoner.GetAI()._AIData.SetActive(true);
			}
        };
    }
}
