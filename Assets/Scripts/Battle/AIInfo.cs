using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Enums;
using System.Linq;
using Battle.Turn;

public class VigilArea {
	TileRange range;
	Vector2Int mid;
	Direction dir;
	List<TrigUnitType> unitTypes;
	bool near;
	public List<Tile> area;

	public VigilArea(StringParser parser, bool near){
		mid = parser.ConsumeVector2();
		dir = parser.ConsumeEnum<Direction>();
		range = new TileRange(parser.ConsumeString());
		unitTypes = parser.PeekList<TrigUnitType>() ?
			parser.ConsumeList<TrigUnitType>() : new List<TrigUnitType> { TrigUnitType.Enemy };
		this.near = near;
	}
	public void InstatiateArea(Unit unit){
		if (near) mid += unit.Pos;
		if (dir == Direction.None) dir = unit.GetDir();
		area = TileManager.Instance.TilesInRange(range, mid, dir, -1);
	}

	public bool Check(Unit unit) {
		var aroundUnits = Utility.DeleteNull(area.ConvertAll(tile => tile.GetUnitOnTile()));
		foreach(var unitType in unitTypes){
			switch (unitType) {
			case TrigUnitType.Enemy:
				if(aroundUnits.Any(anyUnit => AIUtil.IsSecondUnitEnemyToFirstUnit(unit, anyUnit)))
					return true;
				break;
			case TrigUnitType.Any:
				if(aroundUnits.Count > 0)
					return true;
				break;
			case TrigUnitType.Ally:
				if(aroundUnits.Any(anyUnit => anyUnit.IsAllyTo(unit)))
					return true;
				break;
			case TrigUnitType.AllyNPC:
				if(aroundUnits.Any(anyUnit => !anyUnit.IsPC && anyUnit.IsAllyTo(unit)))
					return true;
				break;
			case TrigUnitType.AllyPC:
				if(aroundUnits.Any(anyUnit => anyUnit.IsPC && anyUnit.IsAllyTo(unit)))
					return true;
				break;
			case TrigUnitType.PC:
				if(aroundUnits.Any(anyUnit => anyUnit.IsPC))
					return true;
				break;
			}
		}
		return false;
	}
}

public class AIInfo {
	public string codeName; //이 AI를 사용하는 유닛의 이름
	public bool actOnStart;
	public bool actOnExternal;
	public bool acute;
	public int actPhase;
	public List<VigilArea> vigilAreas = new List<VigilArea>(); //감시 영역. 이 안에 자신(NPC)의 반대 진영이 들어오면 활성화
    int nearAreaCount;
    int otherAreaCount;
	public string cloneData; //나중에 복제하기 위해 생성자로 받은 data를 들고 있다가 나중에 넘겨줌.

    public AIInfo(string data){
	    cloneData = data;
        StringParser parser = new StringParser(data, '\t');
        codeName = parser.ConsumeString();
        //기본적인 활성화 조건(시작, 외부기술, 페이즈)을 입력.
        string activation = parser.ConsumeString();
        if(activation == "X"){/*아무것도 하지 않음*/}
		else if(activation == "S") actOnStart = true;
		else if(activation[0] == 'E'){
            actOnExternal = true;
            if(activation != "E"){
				if (activation.Substring(0, 2) == "EA") {
					acute = true;
					if(activation != "EA") actPhase = int.Parse(activation.Substring(2, activation.Length - 2));
				}else actPhase = int.Parse(activation.Substring(1, activation.Length - 1));
            }
        }
		else actPhase = int.Parse(activation);

        //감시 영역을 입력.
        nearAreaCount = parser.ConsumeInt();
        otherAreaCount = parser.ConsumeInt();
        for (int i = 0; i < nearAreaCount; i++)
			vigilAreas.Add(new VigilArea(parser, true));
        for (int i = 0; i < otherAreaCount; i++)
			vigilAreas.Add(new VigilArea(parser, false));
    }
	
    public void InstantiateVigilAreas(Unit unit){
	    foreach (var vigilArea in vigilAreas)
		    vigilArea.InstatiateArea(unit);
    }
	public bool CheckIfThereIsUnitInVigilAreas(Unit unit) {
		return vigilAreas.Any(area => area.Check(unit));
	}
}
