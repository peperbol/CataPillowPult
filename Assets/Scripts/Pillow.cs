using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Pillow : MonoBehaviour {
	Character heldBy;
	Rigidbody2D rb;
	Collider2D col;
	public SingleUnityLayer pillowLayer;
	public ParticleSystem particleSystem;
	public int attackParticleCount= 20;
	public float particlesByVelocity = 0.5f;
	public bool isAttacking;
	public float hitForce;
	public float damage = 0.45f;
    void Start() {
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<Collider2D>();
	}

	public void Take(Character taker) {

		if (taker.HeldPillow) return;

		Drop();

		heldBy = taker;
		taker.HeldPillow = this;

		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = 0;

		gameObject.layer = taker.playerLayer.LayerIndex;
		//col.enabled = false;

		transform.SetParent(taker.pillowSocket);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
	public void Drop() {

		isAttacking = false;
		if (heldBy) {
			heldBy.HeldPillow = null;
		}

		rb.isKinematic = false;
		transform.SetParent(null);
		col.enabled = true;
		heldBy = null;
    }

	public void OnCollisionEnter2D(Collision2D collision) {
		if (isAttacking) {
			Rigidbody2D rb = collision.rigidbody;


			if (rb) {
				rb.AddForce(-collision.contacts[0].normal * hitForce);
			}

			Character other = collision.collider.GetComponent<Character>();
			if (other) {
				other.Drop();
				other.Damage(damage);
			}
			particleSystem.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
            particleSystem.Emit(attackParticleCount);

        }
		else {

			particleSystem.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
			particleSystem.Emit(Mathf.FloorToInt(collision.relativeVelocity.magnitude * particlesByVelocity));
		}


		if (!heldBy)
			gameObject.layer = pillowLayer.LayerIndex;
		isAttacking = false;
	}
}
