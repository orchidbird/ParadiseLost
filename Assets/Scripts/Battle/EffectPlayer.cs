using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Skills;
using UnityEngine;
using Enums;
using DG.Tweening;

public static class EffectPlayer {
	public static void ApplySoundEffect(ActiveSkill skill){
		string soundEffectName = skill.castSeName;
        if (soundEffectName != null && soundEffectName != "-") {
            SoundManager.Instance.PlaySE(soundEffectName);
        }
	}

	static void SetRotation(EffectBehaviour effectBehav, Direction dir){
		//Debug.Log("화면 방향: " + (int)BattleData.aspect + ", 시전 방향: " + dir + "(" + (int)dir + ")");
		var actualDir = ((int)BattleData.aspect + (int)dir) % 4;
		
		if(!effectBehav.RotateEachParticle)
			effectBehav.rotator.transform.rotation = Quaternion.Euler(effectBehav.rotations[actualDir]);
		else {
			foreach (var particle in effectBehav.rotator.GetComponentsInChildren<ParticleSystem>()) {
				ParticleSystem.MainModule main = particle.main;
				Vector3 rotation = effectBehav.rotations[actualDir];
				main.startRotation3D = true;
				main.startRotationX = rotation.x;
				main.startRotationY = rotation.y;
				main.startRotationZ = rotation.z;
			}
		}
		
		//0일 경우 회전시키지 않음(그대로 우하 방향)
	}

	static GameObject InstantiateAndRotateVE(Casting casting){
		var prefab = GameData.VolatileData.GetVisualEffectPrefab(casting.Skill.castVeName);
		if (prefab == null) return null;
		
		var VE = GameObject.Instantiate(prefab, prefab.transform.position, prefab.transform.localRotation);
		if (casting.Skill.castVeDir){
			Debug.Assert(VE.GetComponent<EffectBehaviour>() != null, prefab.gameObject.name + "은 회전형이지만 EffectBehaviour가 없음!");
			SetRotation(VE.GetComponent<EffectBehaviour>(), casting.Location.Dir);
		}
		return VE;
	}

	public static IEnumerator ApplyVisualEffect(Casting casting) {
        var unit = casting.Caster;
        var secondRange = casting.RealRange;
		string visualEffectName = casting.Skill.castVeName;
		if (visualEffectName == "-")
			yield break;
		var VEPrefab = GameData.VolatileData.GetVisualEffectPrefab(visualEffectName);
		if (VEPrefab == null)
			yield break;

		float EFFECTTIME = GetEffectDuration(casting, false);
		
		// 범위형 이펙트
		if(casting.Skill.castVeArea)
			yield return ShowAreaEffect(casting, unit.TileUnderUnit, casting.Skill.castVeMove ? casting.Location.TargetTile : unit.TileUnderUnit, EFFECTTIME);
		else{// 개별 대상 이펙트.
			Vector3 startPos = unit.realPosition + Vector3.back * 0.01f;
			var endPosList = new List<Vector3>();
			foreach (var tileObject in secondRange){
				var tile = tileObject;
				if (!tile.IsUnitOnTile()) continue;
				if(casting.Skill.korName == "대지의 비늘" && !tile.GetUnitOnTile().IsAllyTo(casting.Caster)) 
					continue;

				endPosList.Add(tile.GetUnitOnTile().realPosition + Vector3.back * 0.1f);
			}

			foreach (var endPos in endPosList) {
				GameObject VE = InstantiateAndRotateVE(casting);
				VE.transform.position = casting.Skill.castVeMove ? startPos : endPos;
				GameObject.Destroy(VE, EFFECTTIME * 1.6f); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}

			yield return new WaitForSeconds(EFFECTTIME);
        }

		if (casting.Skill.korName != "에테르 샷") yield break;
		foreach (var kv in Darkenir_EtherShot_SkillLogic.tileData){
			foreach (var tile in Darkenir_EtherShot_SkillLogic.tileData.Where(item => item.Value == kv.Value + 1).Select(item => item.Key).ToList())
				BattleManager.Instance.StartCoroutine(ShowAreaEffect(casting, kv.Key, tile, EFFECTTIME * 0.5f));
		}
	}

	static IEnumerator ShowAreaEffect(Casting casting, Tile startTile, Tile endTile, float duration){
		GameObject VE = InstantiateAndRotateVE(casting);
		if(VE == null) yield break;
		
		VE.transform.position = startTile.realPosition + Vector3.back * 0.5f;
		yield return new WaitForSeconds (0.4f * duration);
		Tween tw = VE.transform.DOMove(endTile.realPosition + Vector3.back * 0.5f, 0.6f * duration);
		yield return tw.WaitForCompletion();
		GameObject.Destroy(VE, duration);
	}

	public static float GetEffectDuration(Casting casting, bool forWaitingFeedback){
		var VePrefab = GameData.VolatileData.GetVisualEffectPrefab(casting.Skill.castVeName);
		if (VePrefab == null) return 0;
		return VePrefab.GetComponentInChildren<ParticleSystem>().main.duration * (BattleData.turnUnit.IsAI ? 0.5f : 1) * (VePrefab.name == "EtherShot" && forWaitingFeedback ? 2 : 1);
	}
}
