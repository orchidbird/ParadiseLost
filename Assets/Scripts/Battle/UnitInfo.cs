using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Remoting.Messaging;
using Enums;
using GameData;
using UtilityMethods;

public class UnitInfo{//아래의 2개 메소드는 PCStatData에 정보가 있는 유닛에게만 사용 가능(18.04.15)
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
}
