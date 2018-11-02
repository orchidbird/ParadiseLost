using System.Collections.Generic;
using UtilityMethods;

public class Collectable{
    // 수집 가능한 오브젝트(Collectable)에 대한 정보들
    public Unit unit;
    public int phase;
    public int range;
	string actionNameKor;
	string actionNameEng;
	public string actionName{get { return Language.Select(actionNameKor, actionNameEng); }}
	string descriptionKor;
	string descriptionEng;
	public string Description{get { return Language.Select(descriptionKor, descriptionEng); }}

    public Collectable(Unit inputUnit, string[] data){
	    unit = inputUnit;
        phase = int.Parse(data[1]);
        range = int.Parse(data[2]);
	    actionNameKor = data[3];
	    actionNameEng = data[4];
	    descriptionKor = data[5];
	    descriptionEng = data[6];
    }
}
