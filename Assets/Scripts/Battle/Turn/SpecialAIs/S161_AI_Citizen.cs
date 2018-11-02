using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class S161_AI_Citizen : AI{
		public override void SetFirstState() {
			state = "CantiumCitizen";
		}

		public IEnumerator CantiumCitizen(){
			yield return ChaseSomeone ("triana", "EndTurn", "EndTurn");
		}
	}
}
