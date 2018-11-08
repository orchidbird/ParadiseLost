using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Enums {
	public static class EnumUtil{
		public static List<Direction> directions;
		public static List<Direction> nonTileDirections;
		public static List<StatusEffectType> ccTypeList = new List<StatusEffectType> {
			StatusEffectType.Bind, StatusEffectType.Silence,
			StatusEffectType.Faint, StatusEffectType.Taunt
		};

		public static List<Stat> statsToUpdate = new List<Stat>{
			Stat.MaxHealth, Stat.Power, Stat.Defense, Stat.Will
		};
        public static IconSprites GetccIcon(StatusEffectType ccType){
	        if (ccTypeList.Contains(ccType))
		        return (IconSprites)Enum.Parse(typeof(IconSprites), ccType.ToString());
	        return IconSprites.Transparent;
        }
		public static readonly Difficulty hardest = Difficulty.Legend;
		public static readonly Difficulty easiest = Difficulty.Intro;

		static EnumUtil(){
			directions = new List<Direction> ();
			directions.Add (Direction.LeftDown);
			directions.Add (Direction.RightDown);
			directions.Add (Direction.LeftUp);
			directions.Add (Direction.RightUp);
			nonTileDirections = new List<Direction> ();
			nonTileDirections.Add (Direction.Left);
			nonTileDirections.Add (Direction.Right);
			nonTileDirections.Add (Direction.Up);
			nonTileDirections.Add (Direction.Down);
		}
		public static IEnumerable<T> GetValues<T>(){
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static List<Color> GetColorList(ColorList listType){
			if (listType == ColorList.BW)
				return new List<Color> {Color.black, Color.white};
			if(listType == ColorList.CMY)
				return new List<Color> {Color.cyan, Color.yellow, Color.magenta};
			return new List<Color> {new Color(0.57f, 0.44f, 0.86f), new Color(0.3f, 0, 0.5f)};
		}
	}
	
	public enum ColorList{CMY, BW, ForChain}
	public enum TutorialState{None, Active, Passive} //Active는 스테이지 시작 시 직접 해보는 방식이고, Passive는 그냥 눌러서 보기만 하는 것.
    public enum GameMode { Story, Test, AllStageTest, Challenge };
	public enum StageInfoType{POV = 2, Title = 4, Intro = 6, Summary = 8}
	public enum Difficulty {Intro = -1, Adventurer = 0, Tactician = 1, Legend = 2} //지략가(보통), 탐험가(쉬움), 전설(어려움)
	public enum Lang{Kor, Eng}
	public enum Side{ Ally, Enemy, Neutral }
	public enum ShowType{On, Off, Blink}
	public enum UnitClass{ None, Melee, Magic }
	public enum ObjectTag { None, Deathrattle, Collectable }
	public enum Element{ None, Fire, Water, Plant, Metal }
    public enum Aspect { North, East, South, West } //임의로 지정해둔 방향. North가 기본 방향((0, 0)이 화면의 맨 왼쪽)임
    public enum FogType { None = 0, Gray = 1, Black = 2 }
	// 앞 4개만이 실제 게임의 타일 방향, 뒤 4개는 개발용 변수
	public enum Direction{RightDown, RightUp, LeftUp, LeftDown, Right, Up, Left, Down, None }
	public enum RangeType{Point, Route, Auto} //지정형, 경로형, 자동형

	public enum StageNum{Invalid=0, S1_1=11, S1_2=12, S2_1=21, S3_1=31, S4_1=41, S5_1=51, S6_1=61, S7_1=71, S8_1=81, S9_1=91, S10_1=101, S10_2=102, S11_1=111, S12_1=121, S13_1=131, S14_1=141, S15_1=151, S16_1=161, S18_1=181, S19_1=191, S20_1=201}

	//트리거 관련
	public enum TrigResultType{Win, Lose, Bonus, Trigger, Spawn, Script, MoveCamera, Zoom, Info}
	/*enum 번호는 BattleTrigger 생성자에서 사용됨.
	코드번호 1~9 : Actor/Target 모두 생략됨
	코드번호 11~19 : Actor/Target 중 하나만 입력 *18~22번은 subType 필요
	코드번호 21~29 : Actor/Target 모두 입력*/
	public enum TrigActionType{Phase = 1, Extra = 2, Start = 3, 
		BackAttack = 12, Escape = 13, UnderCount = 15, Cast = 16, SideAttack = 17, 
		StepOnTile = 18, MultiAttack = 21, Effect = 22,
		Collect = 23, Kill = 24, Retreat = 25, Damage = 26, Neutralize = 27}
	public enum TrigUnitType{Enemy, Any, Name, Ally, AllyNPC, AllyPC, NeutralChar, EnemyChar, PC, SpecialEnemy}
    public enum TrigUnitCheckType{Actor, Target}
    public enum ActionButtonType { Skill, Standby, Collect, Absent };
	
    public enum Stat{
        MaxHealth = 1, Power = 2, Defense = 3, Agility = 4,
        CurrentHP = 6, Will = 7, CurrentAP = 8, // 이 셋은 엄밀히 말해 Stat은 아니지만 대미지/효과 계산에 포함되므로 추가
        Level = 9, 
	    Exp = 10, MaxExp = 11, None = 12 // 대미지 없음, 고정값, 또는 기타 특수한 경우
    }

	public enum RangeForm {Front, AllDirection, Straight, Diamond, Square, Triangle, Cross, Diagonal, Sector, Global, One}

	public enum SkillApplyType{
		// 스킬 타입의 효과 우선 순위: Tile > Damage > Heal > Debuff > Buff > Move
		Tile, DamageHealth, DamageAP, HealHealth, HealAP, Debuff, Buff, Trap, Move, Etc
	}

	public enum StatusEffectCategory{Buff, Debuff, Etc, All}
	public enum DirectionCategory {Front,Side,Back}

    public enum StatusEffectChangeType {Attach, Remove, AmountChange, DurationChange, StackChange}
	public enum StatusEffectType{
        MaxHealthChange, PowerChange, DefenseChange, AgilityChange, WillChange, EvasionChange,
        Smite, Shield,
        RequireMoveAPChange, RequireSkillAPChange, // 각각 이동/기술에 필요한 행동력 소모 증감('의지'의 하위 개념)
		MoveCostAccDecrease, //  이동 소모 행동력의 거리 가중치가 1 감소(유진-여행자의 발걸음)
		EaseMoveHeightConstraint, // 이동할 때 높이차 무시
        DamageChange, TakenDamageChange, // 주는/받는 피해량 증감
        HealChange, TakenHealChange, // 주는/받는 회복량 증감
		DamageOverPhase, HealOverPhase, // 지속 피해/회복
		RetreatHPChange, // 이탈 기준 HP 변경
		Overload, // 기술 과부하(같은 턴에 여러 번 사용하면 소모 행동력 증가) 또는 기술 시전으로 인해 자기한테 붙는 불리한 효과. 보호막이 있어도 무조건 붙음
		//이 위까지는 값이 0일 때 무효 처리되며, 아래부터는 0이어도 유효하다
		Silence, Bind, Faint, Collect, //침묵/속박/기절/수집
        Stealth, Trap, MagicCircle, Aura, AllyAura, EnemyAura, Taunt,
		Reflect, // 반사: 받는 피해의 일부만큼 공격자에게 피해
		AllImmune, ForceMoveImmune, // 각각 물리/마법/모든 피해/강제이동 면역
		ZOC, // 효과의 주인에게 적일 경우 해당 효과가 붙은 타일로 이동 불가(세피아 고유 특성)
		Pacifist, // 이 효과가 붙은 유닛이 공격해서 체력이 0 이하가 된 유닛은 처치 대신 이탈(노엘의 '언령' 효과)
        Etc // 위 분류에 해당하지 않는 효과
	}
	
	//높이가 더 낮은 타일까지의 거리를 측정할 때 어떤 방식을 쓸지. Flat은 높이를 무시, Bonus는 더 가깝게 계산(유리하게), Penalty는 더 멀게 계산(불리하게).
	public enum LowerTileDistCalcType{Flat = 0, Bonus = -1, Penalty = 1}

	public enum FormulaVarType{
		None, Power, Defense, Resistance, Agility, MaxHealth, Level,
        Stack, // 스택 비례일 경우
		LostHpPercent, // 잃은 체력 %
		Etc // 기타 변수
	}

    public enum IconSprites {Transparent, Black, Bind, Silence, Faint, Collect, Chain, Standby, PointSkill, AutoSkill, Crown, LeftArrow, RightArrow, Spotlight, Taunt, Deathrattle, Fear}
    public enum SpriteType {Portrait, Illust, UnitImage, TileImage, Icon}

	public enum WillChangeType {Pain, Exhausted, Cheer, Disturbed, Shocked, DirectNeutralize, Angry, Guardian, Saviour, Cooperative, Chain, Blood, Etc}
	public enum WillCharacteristic {Tough, Fragile, Exhausted, Calm, Combative, Angry, ValueHonor, Sympathy, Bloodlust, Guardian, Saviour, Cooperative, Detached}
	public enum BigSmall {None, Big, Small}

	public enum Faction {Pintos, Haskell, Aster}

	public enum CameraWorkType { Zoom, MoveCamera, WaitForClickingConditionPanel, Script, AutoUnitGen, AutoUnitGenOnlyPC, ManualUnitGen, UIInitialize,
				MoveCameraToTileAveragePosAndZoomOut, MoveCameraToPCAveragePos, ZoomDefault, MoveLock, MoveUnlock }

    public enum GlossaryType{Person = 0, Group = 1, Location = 2, Etc = 3}
	public enum BattleEndType { Win, Lose, Restart }
    public class EnumConverter {
        public static Stat ToStat(StatusEffectType statusEffectType) {
            switch (statusEffectType) {
            case StatusEffectType.PowerChange:
                return Stat.Power;
            case StatusEffectType.DefenseChange:
                return Stat.Defense;
            case StatusEffectType.AgilityChange:
                return Stat.Agility;
			case StatusEffectType.WillChange:
				return Stat.Will;
            case StatusEffectType.MaxHealthChange:
                return Stat.MaxHealth;
            }
            return Stat.None;
        }
        public static StatusEffectType ToStatusEffectType(Stat stat) {
            switch (stat) {
            case Stat.Power:
                return StatusEffectType.PowerChange;
            case Stat.Defense:
                return StatusEffectType.DefenseChange;
            case Stat.Agility:
                return StatusEffectType.AgilityChange;
			case Stat.Will:
				return StatusEffectType.WillChange;
            case Stat.MaxHealth:
                return StatusEffectType.MaxHealthChange;
            }
            return StatusEffectType.Etc;
        }

        public static Direction GetOpposite(Direction direction) {
            switch(direction) {
            case Direction.Right:
                return Direction.Left;
            case Direction.Up:
                return Direction.Down;
            case Direction.Left:
                return Direction.Right;
            case Direction.Down:
                return Direction.Up;
            case Direction.RightUp:
                return Direction.LeftDown;
            case Direction.LeftUp:
                return Direction.RightDown;
            case Direction.LeftDown:
                return Direction.RightUp;
            case Direction.RightDown:
                return Direction.LeftUp;
            }
            return default(Direction);
        }
        public static Stat ToStat(FormulaVarType type) {
            switch(type) {
            case FormulaVarType.Power:
                return Stat.Power;
            case FormulaVarType.Defense:
                return Stat.Defense;
            case FormulaVarType.Agility:
                return Stat.Agility;
            case FormulaVarType.MaxHealth:
                return Stat.MaxHealth;
            case FormulaVarType.Level:
                return Stat.Level;
            }
            return Stat.None;
        }
    }
}
