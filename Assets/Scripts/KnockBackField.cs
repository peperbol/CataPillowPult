using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBackField : MonoBehaviour {

	public float knockbackForce = 100;

	public void OnCollisionEnter2D(Collision2D collision) {
		collision.rigidbody.AddForce(- collision.contacts[0].normal * knockbackForce);
    }
}
