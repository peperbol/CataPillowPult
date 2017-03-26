using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilizer : MonoBehaviour {
	public Vector3 rotation;

	public void Update() {
		 transform.rotation =  Quaternion.Euler(rotation);
	}
}
