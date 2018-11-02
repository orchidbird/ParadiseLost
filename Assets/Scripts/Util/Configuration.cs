
using System.Collections.Generic;

public class Configuration {
    public static float textObjectDuration = 0.5f;            // 피해 및 회복 텍스트 표시되는 시간(최대 1초)
    public static float SEVolume = 1;                // 효과음 크기
    public static float BGMVolume = 1;                        // 배경음악 크기
    public static float NPCBehaviourDuration = 0.3f;          // NPC 행동 연출 시간(최대 0.5초)
	
	public static bool showRealBlood;
	public static Dictionary<Warning.WarningType, bool> ignoreWarning = new Dictionary<Warning.WarningType, bool>(){
		{Warning.WarningType.InvalidCasting, false},
		{Warning.WarningType.DifficultyChange, false},
		{Warning.WarningType.Restart, false},
		{Warning.WarningType.Title, false}
	};
	
	public static float TextDisplayWaitTime{get { return 0.3f + 0.2f*textObjectDuration; }} //0으로 맞춰도 짧은 시간 동안은 보이도록 하기 위함.
}
