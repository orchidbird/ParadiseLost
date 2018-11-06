using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Enums;
using GameData;
using UtilityMethods;
using Random = UnityEngine.Random;

public class HitInfo{
	public readonly Unit caster;
	public readonly ActiveSkill skill;
	public readonly int finalDamage;

	public HitInfo(Unit caster, ActiveSkill skill, int finalDamage){
		this.caster = caster;
		this.skill = skill;
		this.finalDamage = finalDamage;
	}
}

public class UnitInfo{
	public string codeName;
	public Dictionary<Stat, int> baseStats = new Dictionary<Stat, int>();
	public bool isAlly;
	public List<Skill> skills = new List<Skill>();
	
	private string GetRandomCharacterName{get{
		return Generic.PickRandom(new List<string> {"reina", "noel", "yeong", "lucius", "bianca", "lenien", "karldrich"});
	}}

	void AddSkill(List<Skill> potentialSkills){
		Debug.Assert(potentialSkills.Count > skills.Count);
		if (isAlly)
			potentialSkills = potentialSkills.FindAll(item => item.address[0] != 'L' && item.address[0] != 'E');
		Skill skill;

		do{
			skill = Generic.PickRandom(potentialSkills);
		}while(skills.Contains(skill) || (skill.RequiredSkill != null && !skills.Contains(skill.RequiredSkill)));
		
		skills.Add(skill);
	}
	public UnitInfo(bool isAlly){
		this.isAlly = isAlly;
		do{
			codeName = GetRandomCharacterName;
		} while (RecordData.units.Any(unit => unit.codeName == codeName));
		
		if (isAlly){
			baseStats.Add(Stat.MaxHealth, RandomIntOfVariation(100, 1.05f));
			baseStats.Add(Stat.Power, RandomIntOfVariation(20, 1.1f));
			baseStats.Add(Stat.Defense, RandomIntOfVariation(32, 1.15f));
			baseStats.Add(Stat.Agility, RandomIntOfVariation(50, 1.04f));
			baseStats.Add(Stat.Will, RandomIntOfVariation(100, 1.1f));
			baseStats.Add(Stat.Level, 1);
			skills.Add(TableData.ActiveSkills.Find(skill => skill.korName == "순간 이동"));
			//E로 시작하는 것은 시작 스킬로 주지 않도록 지정
			AddSkill(TableData.ActiveSkills.FindAll(skill => skill.GetCooldown() < 2).ConvertAll(skill => (Skill)skill));
		}else{
			baseStats.Add(Stat.MaxHealth, 300);
			baseStats.Add(Stat.Power, 35);
			baseStats.Add(Stat.Defense, 50);
			baseStats.Add(Stat.Agility, 60);
			baseStats.Add(Stat.Will, 100);
			baseStats.Add(Stat.Level, 1);
			skills.Add(TableData.ActiveSkills.Find(skill => skill.korName == "화염 폭발"));
			skills.Add(TableData.ActiveSkills.Find(skill => skill.korName == "쇄도"));
			AddSkill(TableData.ActiveSkills.ConvertAll(skill => (Skill)skill));
		}
		baseStats.Add(Stat.CurrentHP, baseStats[Stat.MaxHealth]);
		AddSkill(TableData.PassiveSkills.ConvertAll(skill => (Skill)skill));

		RecordData.units.Add(this);
	}
	
	int RandomIntOfVariation(int basePoint, float maxRatio){
		var minValue = (int)Math.Round(basePoint * maxRatio);
		var maxValue = (int)Math.Round(basePoint / maxRatio);
		return Random.Range(minValue, maxValue+1);
	}
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
