using UnityEngine;
using System;
using System.Collections;

public enum DataType
{

}

public class DialogueData{
	//**할당된 int의 범위로 불러오는 경우가 있기 때문에, SubType이 있는 것이 항상 없는 것보다 먼저 와야 한다!!
	public enum CommandType {App = 0, Disapp = 1, BGM = 2, BG = 3, SE = 4, Script = 5, Battle = 6, Glos = 8, Quiver = 9, Tutorial=10, MovingFade = 11,
	FO = 12, FI = 13, //SubType이 있을 수도, 없을 수도 있음(기본값은 1초고 별도 지정 가능) 
	//이하는 SubType이 필요없는 것들.
	Gray = 14, Colorful = 15, Quake=16, Return=17, Title = 18, AskTitle = 19, Era = 20, Else};
	public CommandType Command;

	bool isEffect;
	string emotion;
	string dialogue;
	string commandSubType;
	string nameInCode;
	//float additiveValue; //Fade 시간이나 효과음의 볼륨(이건 구현예정) 등 float으로 처리 가능한 부가 정보.

	public bool IsEffect { get { return isEffect; } }
	public string GetNameInCode() { return nameInCode; }
	public string GetEmotion() { return emotion; }
	public string GetDialogue() { return dialogue; }
	public string GetCommandSubType() { return commandSubType; }

	public DialogueData (string unparsedDialogueDataString){
		StringParser parser = new StringParser(unparsedDialogueDataString, '\t');
		string inputType = parser.ConsumeString();
		if(inputType == "*"){
			isEffect = true;
			Command = parser.ConsumeEnum<CommandType>();
			
			if ((int) Command <= (int) CommandType.MovingFade)
				commandSubType = parser.ConsumeString();
			else if ((int) Command <= (int) CommandType.FI && parser.Remain > 0)
				commandSubType = parser.ConsumeString();
			
			if(Command == CommandType.App)
				nameInCode = parser.ConsumeString();
		}else{
			parser.ResetIndex();
			nameInCode = parser.ConsumeString();
			emotion = parser.ConsumeString();
			dialogue = parser.ConsumeString().Replace ("^", "\n");
		}
	}
}
