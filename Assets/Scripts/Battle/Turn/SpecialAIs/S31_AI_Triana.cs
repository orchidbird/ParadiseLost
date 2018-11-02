using System;
using System.Collections;
using Enums;
using UnityEngine;
using UtilityMethods;

namespace Battle.Turn{
	public class S31_AI_Triana : AI{
		public override void SetFirstState (){
			state = "Triana_Rest";
		}

		public IEnumerator Triana_Rest(){
			if (2 * unit.GetHP > unit.GetMaxHealth ())
				yield return ChaseSomeone ("yeong", "EndTurn", "EndTurn");
			else
				state = "InitialState";
		}

		public override float GetTargetValue(Unit target){
			return (target.GetSide() == Side.Neutral || target == unit) ? 0
				: -(float)Math.Pow(0.5f, Calculate.Distance(unit, target)) * 100;
		}
	}
}
