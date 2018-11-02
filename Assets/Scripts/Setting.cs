using System.Collections.Generic;
using Enums;

public class Setting{
	//기능 통제
	public static readonly StageNum directionOpenStage = StageNum.S1_1;
	public static readonly StageNum cameraMoveOpenStage = StageNum.S1_2;
	public static readonly StageNum passiveOpenStage = StageNum.S1_2;
	public static readonly StageNum statusEffectOpenStage = StageNum.S1_2;
	public static readonly StageNum detailInfoOpenStage = StageNum.S1_2;
	public static readonly StageNum classOpenStage = StageNum.S2_1;
	public static readonly StageNum elementOpenStage = StageNum.S3_1;
	public static readonly StageNum retreatOpenStage = StageNum.S4_1;
	public static readonly StageNum heightOpenStage = StageNum.S4_1;
	public static readonly StageNum WillChangeOpenStage = StageNum.S4_1;
	public static readonly StageNum chainOpenStage = StageNum.S5_1;
	public static readonly StageNum readySceneOpenStage = StageNum.S6_1; //능력 선택과 동시에 개방
	public static readonly StageNum challengeModeOpenStage = StageNum.S6_1;
	public static readonly StageNum WillCharacteristicOpenStage = StageNum.S7_1;
	public static readonly StageNum manualGenerationOpenStage = StageNum.S9_1;
	public static readonly StageNum pintosVSHaskellStage = StageNum.S20_1;
	public static readonly List<StageNum> agilityChangingStage = new List<StageNum>{StageNum.S6_1, StageNum.S11_1};
	public static bool clickEnable = true; //ActionButton에 대한 마우스 입력 허용 여부. 단축키를 강제할 때만 false로 두면 됨
	public static bool shortcutEnable = true;
	public static bool showMotionPC = true;
	public static bool FastAITurn = false;

	//변수 통제(전투 규칙)
	public static readonly float retreatHPFloat = 0.2f;
	public static readonly int moveCostAcc = 2; //이동할 때 타일당 추가로 붙는 계차값
	public static readonly float sideAttackBonus = 1.1f;
	public static readonly float backAttackBonus = 1.25f; 
	
	//변수 통제(시간)
	public static readonly float fadeInWaitingTime = 0.3f;
	public static readonly int fastDialogueFrameLag = 4; //대화 모드에서 빨리 넘길 때(CTRL) 몇 프레임마다 한 번씩 넘길지 설정
	public static readonly float phasePassTime = 0.5f; //페이즈 시작 및 종료 시 대기시간
	public static readonly float basicCameraMoveDuration = 0.2f;
	public static readonly float cameraZoomDuration = 0.5f;
	public static readonly float moveDuration = 0.3f;
	public static readonly float slideUIFastDuration = 0.5f;
	public static readonly float slideUISlowDuration = 1.5f;
	public static readonly float AILagMaxDuration = 0.05f;
	public static readonly float bloodyScreenDurationWhenHit = 0.5f; // 피격시 붉은 색조 연출의 지속시간
	public static readonly float unitFadeOutDuration = 0.75f;         // 죽었을 때 붉은 색조 연출이 나타나고 유닛이 사라지는 효과의 지속 시간
	public static readonly float unitFadeInDuration = 0.25f;       // 죽었을 때 사라진 후 붉은 색조 연출이 다시 완전히 없어지기까지의 시간
	public static readonly float clickEffectDuration = 0.4f;
	public static readonly float longClickDurationThreshold = 1;
	public static readonly float noInputTimeToBeConsideredAsAFK = 30; // PlayLog에 기록할 때 유저가 얼마나 오랫동안 아무것도 하지 않아야 잠수로 간주하는지
	
	//변수 통제(시각 요소)
	public static readonly float tileImageHeight = 0.5f;
	public static readonly float tileImageWidth = 1.0f;
    public static readonly float bloodyScreenHPThreshold = 0.5f;     // 화면의 붉은 색조 연출이 시작되는 체력 비율
    public static readonly float bloodynessWhenHit = 0.5f;           // 피격시 붉은 색조 연출의 정도
    public static readonly float smallConditionTextSize = 0.7f; // (원래 크기와 비교했을 때 상대적인)사이즈
    public static readonly float minCameraSize = 1.2f;
    public static readonly float maxCameraSize = 10.8f;

	//성능에 영향을 미치므로 디버그에 필요할 때만 켜고, 빌드할 때는 반드시 끌 것.
	public static readonly bool LogDebuggingForMeaning = false; //규칙상 유의미한 로그를 볼 경우.
	public static readonly bool LogDebuggingForElse = false; //기타 무의미하거나 연출용 로그를 볼 경우.
}
