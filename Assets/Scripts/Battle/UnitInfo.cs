using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Remoting.Messaging;
using Enums;
using GameData;
using UtilityMethods;

public class UnitInfo{
	public string nameKor;
	public string codeName;
	public string connectedName; //연결된 유닛 이름. 유닛이 파괴될 때 이 이름을 가진 모든 유닛이 같이 파괴된다.
	public string holdName; //붙잡은 유닛 이름.
	public Side side;
	public Vector2 size = new Vector2(1, 1);
	UnitClass unitClass;
	public UnitClass GetUnitClass{get { return !VolatileData.OpenCheck(Setting.classOpenStage) ? UnitClass.None : unitClass; }}
	public Element element;
	public Element GetElement{get { return (unitClass == UnitClass.None || !VolatileData.OpenCheck(Setting.elementOpenStage)) ? Element.None : element; }}
	public bool isObject;
	public ObjectTag objectTag = ObjectTag.None;
	public bool isNamed;
	public Dictionary<Stat, int> baseStats = new Dictionary<Stat, int>();
	public Dictionary<WillCharacteristic, bool> WillCharacteristics = new Dictionary<WillCharacteristic, bool>();
	public static float HpFactor;
	public static float PowerFactor;
	public static float AgilityFactor;

	public UnitInfo (string data, bool isPC){ //PC의 경우 data에 codeName만 넣으면 됨
		var parser = new StringParser(data, '\t');
		
		if(isPC){
			codeName = data;
			nameKor = ConvertName(data);
			side = Side.Ally;

			for(var i = 1; i < 6; i++){
				baseStats.Add((Stat)i, GetStatForPC(codeName, (Stat)i));
			}
			baseStats.Add(Stat.Will, 100);

			unitClass = GetProperty<UnitClass>(codeName);
			element = GetProperty<Element>(codeName);
			
			WillCharacteristics = GetWillCharacteristics (codeName);
			isNamed = true;
		}else{ 
			codeName = parser.ConsumeString();
			nameKor = ConvertName(codeName);
			side = parser.ConsumeEnum<Side>();

			for(var i = 1; i < 6; i++){
				baseStats.Add((Stat)i, parser.ConsumeInt());
			}
			baseStats.Add(Stat.Will, 100);

			unitClass = parser.ConsumeEnum<UnitClass>();
			element = parser.ConsumeEnum<Element>();
			
			isObject = parser.ConsumeBool();
			isNamed = parser.ConsumeBool();

			if(parser.PeekList<ObjectTag>())
				objectTag = parser.ConsumeEnum<ObjectTag>();

			while (parser.Remain > 0){
				var additiveType = parser.ConsumeString();
				if (additiveType == "size")
					size = parser.ConsumeVector2();
				else if (additiveType == "connect")
					connectedName = parser.ConsumeString();
				else if (additiveType == "hold")
					holdName = parser.ConsumeString();
			}
		}

		if (side != Side.Enemy) return;
		baseStats[Stat.MaxHealth] = (int)(baseStats[Stat.MaxHealth]*HpFactor);
		baseStats[Stat.Power] = (int) (baseStats[Stat.Power] * PowerFactor);
		baseStats[Stat.Agility] = (int) (baseStats[Stat.Agility] * AgilityFactor);
	}
	public bool IsLarge{get{return size != new Vector2(1, 1);}}

	//Class/Element의 openstage를 확인 후 미만이면 해당 요소를 None으로 변경.
	public void CheckPropertyStage(){
		unitClass = Utility.CheckStageAndSetNone(unitClass);
		element = Utility.CheckStageAndSetNone(element);
	}

	//아래의 2개 메소드는 PCStatData에 정보가 있는 유닛에게만 사용 가능(18.04.15)
	static string PCStatData;
	static string PCWillCharacteristicData;
	static List<string[]> statCoefTable;
	public static int GetStatForPC(string unitName, Stat type, bool actual = true){
		//이하는 Resource 로딩.
        int level = RecordData.level;
        if(type == Stat.Level) return level;
        if (PCStatData == null) PCStatData = Resources.Load<TextAsset>("Data/PCStatData").text;
        if(statCoefTable == null) statCoefTable = Parser.GetMatrixTableFrom("Data/StatCoefTable");

        int RelativePoint = 0;
        if((int)type <= 5)
		    RelativePoint = int.Parse(Parser.FindRowDataOf(PCStatData, unitName)[(int)type]);

		if (!actual) return RelativePoint;
			
		float acc = 0;
		if((int)type < 3)
			acc = float.Parse(statCoefTable[(int)type][RelativePoint+5]);

		float coef = 0;
        if ((int)type <= 5)
            coef = float.Parse(statCoefTable[(int)type+2][RelativePoint+5]);

		float basepoint = 0;
        if ((int)type <= 5)
            basepoint = float.Parse(statCoefTable[(int)type+7][RelativePoint+5]);
		return Convert.ToInt32(acc*level*(level-1)/2+coef*level+basepoint);
	}
	public static T GetProperty<T>(string unitName){
        if (PCStatData == null)
            PCStatData = Resources.Load<TextAsset>("Data/PCStatData").text;
        var table = Parser.FindRowDataOf(PCStatData, unitName);

		string propName = "";
		if(typeof(T) == typeof(UnitClass)) {propName = table[6];}
		else if(typeof(T) == typeof(Element)) {propName = table[7];}
		else {Debug.LogError("Invalid Input Type!");}

		return (T)Enum.Parse(typeof(T), propName);
	}

	public  static Dictionary<WillCharacteristic, bool> GetWillCharacteristics(string unitName){
		if (PCWillCharacteristicData == null)
			PCWillCharacteristicData = Resources.Load<TextAsset>("Data/WillCharacteristic").text;
		var tableData = Parser.FindRowDataOf (PCWillCharacteristicData, unitName);

		Dictionary<WillCharacteristic, bool> table = new Dictionary<WillCharacteristic, bool> ();
		if (tableData == null) {
			foreach (WillCharacteristic c in EnumUtil.GetValues<WillCharacteristic>()) {
				table.Add (c, false);
			}
		} else {
			foreach (WillCharacteristic c in EnumUtil.GetValues<WillCharacteristic>()) {
				bool truthValue = (tableData [(int)c + 1] == "O");
				table.Add (c, truthValue);
			}
		}
		return table;
	}

    public static List<string[]> unitNameTable;
	public static string ConvertName(string codeName, bool fullName = false){
		//Debug.Log(codeName + "을 변환");
		if (codeName.StartsWith("?")) return "???";
        if (unitNameTable == null) unitNameTable = Parser.GetMatrixTableFrom("Data/NameData");
		codeName = _String.GeneralName(codeName);
            
        var row = Parser.FindRowOf(unitNameTable, codeName);
		return row == null ? "NoName"
			: row[(int)VolatileData.language + 1 + (fullName ? Enum.GetNames(typeof(Lang)).Length : 0)];
	}

	public static List<string[]> difficultyModifierTable;
	public static void SetDifficultyFactors(bool reset){
		if (reset){
			HpFactor = 1;
			PowerFactor = 1;
			AgilityFactor = 1;
			return;
		}
			
		if (difficultyModifierTable == null) difficultyModifierTable = Parser.GetMatrixTableFrom("Data/DifficultyMultiplier");
		var difficultyRow = difficultyModifierTable.Find(row => row[0] == VolatileData.difficulty.ToString());
		Debug.Assert(difficultyRow != null, "난이도를 찾을 수 없음!");

		HpFactor = float.Parse(difficultyRow[1]);
		PowerFactor = float.Parse(difficultyRow[2]);
		AgilityFactor = VolatileData.stageData.IsAgiligyChangingStage ? float.Parse(difficultyRow[3]) : 1;
	} 

	public Faction GetFaction(){
		return Utility.PCNameToFaction (codeName);
	}

	public static UnitInfo FindByGenInfo(UnitGenInfo genInfo){
		return VolatileData.stageData.GetUnitInfos().Find(unitinfo => unitinfo.codeName == genInfo.CodeName);
	}
}
