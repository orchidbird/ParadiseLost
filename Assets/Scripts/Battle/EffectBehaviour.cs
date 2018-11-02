using System.Collections.Generic;
using UnityEngine;

public class EffectBehaviour : MonoBehaviour {
	public GameObject rotator;
	public bool RotateEachParticle;
	public List<Vector3> rotations = new List<Vector3> {
		new Vector3(0, 0, 0),
		new Vector3(180, 0, 0),
		new Vector3(180, 180, 0),
		new Vector3(0, 180, 0)
	};
}
