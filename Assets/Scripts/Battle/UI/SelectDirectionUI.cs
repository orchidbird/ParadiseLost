using UnityEngine;
using System.Collections.Generic;
using Enums;

namespace BattleUI{
	public class SelectDirectionUI : MonoBehaviour{
		public ArrowButton[] ArrowButtons;
		Dictionary<Direction, ArrowButton> arrowButtons;

		public void Initialize(){
			arrowButtons = new Dictionary<Direction, ArrowButton> ();
			arrowButtons [Direction.RightUp] = ArrowButtons [0];
			arrowButtons [Direction.LeftUp] = ArrowButtons [1];
			arrowButtons [Direction.RightDown] = ArrowButtons [2];
			arrowButtons [Direction.LeftDown] = ArrowButtons [3];
		}

		public void EnableOnlyThisDirection(Direction direction){
			foreach(var pair in arrowButtons) pair.Value.button.interactable = (pair.Key == direction);
		}
		public void EnableAllDirection(bool OnOff){
			foreach(var pair in arrowButtons) pair.Value.button.interactable = OnOff;
		}
		public void AddListenerToDirection(Direction direction, UnityEngine.Events.UnityAction action){
			arrowButtons [direction].button.onClick.AddListener (action);
		}
		public void RemoveListenerToDirection(Direction direction, UnityEngine.Events.UnityAction action){
			arrowButtons [direction].button.onClick.RemoveListener (action);
		}
	}
}
