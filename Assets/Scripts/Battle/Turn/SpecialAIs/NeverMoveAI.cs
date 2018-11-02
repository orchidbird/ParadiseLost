using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Turn{
	public class NeverMoveAI : AI{
		public override void SetFirstState (){
			state = "NeverMoveCastingLoop";
		}
	}
}