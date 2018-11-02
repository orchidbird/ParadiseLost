using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class UnitGenInfo{
	public string CodeName;
	public Vector2Int Position;
	public readonly Direction Direction;
	public Dictionary<Stat, int> InitStatChanges = new Dictionary<Stat, int>();
	public bool alreadyGenerated = false;
	
	public UnitGenInfo(Vector2Int position, Direction direction) {
		Position = position;
		Direction = direction;
	}

	public UnitGenInfo(string line){
		var parser = new StringParser(line, '\t');
		CodeName = parser.ConsumeString();
		int posX = parser.ConsumeInt();
		int posY = parser.ConsumeInt();
		Position = new Vector2Int(posX, posY);
		Direction = parser.ConsumeEnum<Direction>();
		
		int StatChangeCount = parser.ConsumeInt();
		for(int i = 0; i < StatChangeCount; i++){
			InitStatChanges.Add(parser.ConsumeEnum<Stat>(), parser.ConsumeInt());
		}
	}

	public bool IsNonFixedPosPC{
		get { return CodeName == "selected"; }
	}

	public bool IsFixedPosPC{
		get { return CodeName.StartsWith("PC"); }
	}
	
	public bool Additive{
		get { return Position == new Vector2(0, 0); }
	}
}
