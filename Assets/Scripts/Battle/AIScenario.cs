using UnityEngine;
using Enums;

public class AIScenario{
	public enum ScenarioAct{ Move, UseSkill, Standby, SkipTurn }
	public ScenarioAct act;
	public string functionName;
	public int skillIndex;
	public Vector2Int relativeTargetPos;
	public Direction direction;

	public AIScenario(string data){
		StringParser parser = new StringParser(data, '\t');
		act = parser.ConsumeEnum<ScenarioAct>();

		if (act == ScenarioAct.Move) {
			relativeTargetPos = new Vector2Int (parser.ConsumeInt (), parser.ConsumeInt ());
			functionName = "MoveToThePosition";
		} else if (act == ScenarioAct.UseSkill) {
			skillIndex = parser.ConsumeInt ();
			relativeTargetPos = parser.ConsumeVector2();
			direction = parser.ConsumeEnum<Direction> ();
			functionName = "UseSkill";
		} else if (act == ScenarioAct.Standby) {
			functionName = "EndTurn";
		} else if (act == ScenarioAct.SkipTurn) {
			functionName = "SkipTurn";
		} else {
			Debug.LogError ("Invalid AI scenario action name");
		}
	}
}
