using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Character : MonoBehaviour {
	public enum Player {
		Player1 = 1,
		Player2 = 2
	}

	public Player player;
	public Transform pillowSocket;
	public SingleUnityLayer playerLayer;
	Pillow pillow;
	float health = 1;
	public float healthRegenPerSecond = 0.45f;

	public float moveForce = 5;
	public float airMoveForce = 5;
	public float jumpForce = 50;
	public float wallForce = 50;
	public float throwForce = 50;
	public float maxVelocity = 10;
	public float walkdrag =  0.2f;
	public float standDrag = 70f;
	public Animator pillowAttack;
	public Animator foxAnimator;
	public Transform respawnPoint;
	PhysicsMaterial2D physicsMaterial;

	Dictionary<Collider2D, Collision2D> collitions = new Dictionary<Collider2D, Collision2D>();
	Rigidbody2D rb;
    float actionPressLength;

	public float throwPressLength;

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		gameObject.layer = playerLayer.LayerIndex;
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild(i).gameObject.layer = playerLayer.LayerIndex;
		}
		Respawn();

		physicsMaterial = new PhysicsMaterial2D();
		rb.sharedMaterial = physicsMaterial;
    }
	void Respawn() {
		transform.position = respawnPoint.position;
		rb.velocity = Vector3.zero;
    }

	void FixedUpdate() {
		FixedMove();
		if (HorizontalInput != 0) {
			GoingLeft = HorizontalInput < 0;
		}

		physicsMaterial.friction = (Mathf.Abs(HorizontalInput) > 0.5) ? walkdrag : standDrag;
		//rb.sharedMaterial = physicsMaterial;
	}
	void Update() {
		List< Collider2D > toRemove = collitions.Where(e => !e.Key.enabled || e.Key.gameObject.layer == playerLayer.LayerIndex).Select(e=>e.Key).ToList();
		for (int i = 0; i < toRemove.Count; i++) {
			collitions.Remove(toRemove[i]);
        }
        Action();
        if (!IsInAir())
			JumpCheck();

		health += healthRegenPerSecond * Time.deltaTime;
		health = Mathf.Min(health, 1);
		foxAnimator.SetFloat("movementspeed", Mathf.Abs(rb.velocity.x));

    }
	public void Damage( float amount) {
		health -= amount;

		Debug.Log("dmg: " + health);
        if (health < 0) {
			Respawn();
			health = 1;
		}
    }
	void FixedMove() {
		if (rb.velocity.magnitude < maxVelocity) {
			if (IsOnGround() || IsInAir()) {
				rb.AddForce(Vector2.right * HorizontalInput * (IsOnGround() ? moveForce : airMoveForce));
			}
		}
    }
	float HorizontalInput { get { return Input.GetAxis("Horizontal" + (int)player); } }

	void Action() {
        if (!HeldPillow) {


			if (Input.GetButtonDown("Action" + (int)player) && collitions.Any(e => e.Value.collider.GetComponent<Pillow>())) {
				Debug.Log("takey");
				collitions.Where(e => e.Value.collider.GetComponent<Pillow>()).First().Value.collider.GetComponent<Pillow>().Take(this);
            }
		}
		else {
			Hit();
		}
	}
	string ActionButton {
		get {
			return "Action" + (int)player;
		}
	}
	public void Drop() {
		if (HeldPillow) {
			HeldPillow.Drop();
		}
	}
	void Hit() { 

		pillowAttack.SetBool("Attack", false);

		if (Input.GetButtonDown(ActionButton) ){
			actionPressLength = 0;
		}
		if (Input.GetButton(ActionButton)) {
			actionPressLength += Time.deltaTime;
        }

		if (Input.GetButtonUp(ActionButton)) {
			if (actionPressLength > throwPressLength) {
				Pillow pillow = HeldPillow;
				pillow.Drop();
				pillow.isAttacking = true;
				pillow.GetComponent<Rigidbody2D>().AddForce((GoingLeft ? 1 : -1) * Vector2.left * throwForce);
            }
			else {
				pillowAttack.SetBool("Attack", true);
			}

			actionPressLength = 0;
		}
    }
	void JumpCheck() {
		Vector2 WallNormal = collitionNormal();
		WallNormal.y = 0;
		Debug.DrawRay(transform.position, WallNormal, Color.green);
		if (Input.GetButtonDown("Jump" + (int)player)/*|| Input.GetButtonUp("Jump" + (int)player)*/) {
			Jump();
		}
	}

	void Jump() {
		Vector2 WallNormal = (IsOnGround()) ? Vector2.zero :collitionNormal();
		WallNormal.y = 0;
		rb.AddForce(jumpForce * Vector2.up + wallForce * WallNormal);
	}
	Vector2 collitionNormal() {
		if (collitions.Count == 0) return Vector2.zero;
		return collitions.Select(e => e.Value.contacts[0].normal).Aggregate((p, t) => p + t).normalized;
	}

	bool IsOnGround() {
		return collitionNormal().y > 0.4;
	}
	bool IsInAir() {
		return collitions.Count == 0;
	}

	bool GoingLeft {
		get {
			return transform.localScale.x < 0;
        }
		set {
			transform.localScale = new Vector3((value) ? -1 : 1, 1, 1);
		}
	}
	public void OnCollisionEnter2D(Collision2D collision) {
		try {
			collitions.Add(collision.collider, collision);
		}
		catch (Exception) { }
	}

	public void OnCollisionExit2D(Collision2D collision) {
		collitions.Remove(collision.collider);
	}
	
	public Pillow HeldPillow {
		get {
			return pillow;
		}
		set {
			pillow = value;
		}
	}
	
}
