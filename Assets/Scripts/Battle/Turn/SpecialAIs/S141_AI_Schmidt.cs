using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enums;

namespace Battle.Turn{
	public class S141_AI_Schmidt : AI{
		public override void SetFirstState() {
			state = "Schmidt";
		}

		public IEnumerator Schmidt(){
			yield return ChaseSomeone ("curi", "InitialState", "NeverMoveCastingLoop");
		}
	}
}
