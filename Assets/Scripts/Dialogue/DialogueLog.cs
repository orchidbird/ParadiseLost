using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLog{
	public Sprite leftImage;
	public Sprite rightImage;
	public Sprite backGround;
	public Color leftColor;
	public Color rightColor;
	public string speakerName;
	public string content;
	public bool gray;
	public bool isLeftUnitOld;
	public int lineNumber;
	public DialogueLog(){
		var DM = DialogueManager.Instance;
		leftImage = DM.leftPortrait.sprite;
		rightImage = DM.rightPortrait.sprite;  
		backGround = DM.background.sprite;
		leftColor = DM.leftPortrait.color;
		rightColor = DM.rightPortrait.color;
		isLeftUnitOld = DM.isLeftUnitOld;
		gray = DM.gray;
		speakerName = DM.nameText.text;
		content = DM.dialogueText.text;
		lineNumber = DM.line;
	}
}
