using UnityEngine;
using System;
using System.Collections.Generic;
using Enums;
using Convert = System.Convert;
using Language = UtilityMethods.Language;

public class TileInfo {
	public Vector2 tilePosition;
	int tileAPAtStandardHeight;
	int tileHeight; // 추후 높이 시스템 구현되면 사용.
	Element tileElement;
	int tileTypeIndex;
	bool isEmptyTile;
	string displayName;
    FogType fogType = FogType.None;

	public Vector2 GetTilePosition() { return tilePosition; }
	public Element GetTileElement() { return tileElement; }
	public int GetTileAPAtStandardHeight() { return tileAPAtStandardHeight; }
	public int GetTileHeight() { return tileHeight; }
	public int GetTileIndex() { return tileTypeIndex; }
	public bool IsEmptyTile() { return isEmptyTile; }
	public string GetDisplayName() { return displayName; }
    public FogType GetFogType() { return fogType;}

	public TileInfo(Vector2 tilePosition, string tileInfoString, int height = 0, FogType fogType = FogType.None){
		if (tileInfoString.Length == 0 || tileInfoString[0] == '-'){
			isEmptyTile = true;
			return;
		}
		this.tilePosition = tilePosition;

        tileElement = ReadTileElementChar (tileInfoString[0]);

		string tileTypeIndexSubstring = tileInfoString.Substring(1,2);
		int number;
		if (Int32.TryParse (tileTypeIndexSubstring, out number))
			this.tileTypeIndex = Convert.ToInt32(tileTypeIndexSubstring);
		else
			Debug.LogError ("Undefined tileTypeIndex: <" + tileTypeIndexSubstring + ">" + "at" + tilePosition);


        if (TileLibrary == null)
            LoadTileLibrary();

        string tileIdentifier = tileInfoString.Substring(0, 3);
        this.tileAPAtStandardHeight = TileLibrary[tileIdentifier].baseAPCost;
        this.displayName = TileLibrary[tileIdentifier].Name;

        if (tileInfoString.Length >= 4) {
            string tileHeightSubstring = tileInfoString.Substring(3, 2);
            if (Int32.TryParse(tileHeightSubstring, out number))
                tileHeight = Convert.ToInt32(tileHeightSubstring);
            else
                Debug.LogError("Undefined tileHeight: <" + tileHeightSubstring + ">" + "at" + tilePosition);
        }
        else tileHeight = height;

        if (tileInfoString.Length >= 6) {
            try {
                fogType = (FogType)int.Parse(tileInfoString.Substring(5, 1));
            }
            catch { }
        }
        this.fogType = fogType;
	}

	public static Element ReadTileElementChar(char tileElementChar){
		if (tileElementChar == 'F')
			return Element.Fire;
		else if (tileElementChar == 'W')
			return Element.Water;
		else if (tileElementChar == 'P')
			return Element.Plant;
		else if (tileElementChar == 'M')
			return Element.Metal;
		else if (tileElementChar == 'N')
			return Element.None;
		else {
			Debug.LogError ("Undefined tileType: <" + tileElementChar + ">");
			return Element.None;
		}
	}

    static string tileColorData;
    public static Dictionary<string, TileTypeData> TileLibrary;

	public class TileTypeData{
		public string identifier;
		public string korName;
		public string engName;
		public int baseAPCost;

		public TileTypeData(string dataLine)
		{
			StringParser commaParser = new StringParser(dataLine, ',');
			korName = commaParser.ConsumeString();
			engName = commaParser.ConsumeString();
			identifier = commaParser.ConsumeString();
			baseAPCost = commaParser.ConsumeInt();
		}

		public string Name {get { return Language.Select(korName, engName); } 
	}
}
	private static void LoadTileLibrary (){
		TileLibrary = new Dictionary<string, TileTypeData> ();

		TextAsset csvFile;
		csvFile = Resources.Load<TextAsset>("Data/TileLibrary");
		string csvText = csvFile.text;
		string[] unparsedTileInfoStrings = csvText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 1; i < unparsedTileInfoStrings.Length; i++) {
			TileTypeData tileType = new TileTypeData (unparsedTileInfoStrings [i]);
			TileLibrary [tileType.identifier] = tileType;
		}
	}
}
