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
	public enum CollisionState {
		WALL,
		GROUND,
		AIR
	}

	public Player player;
	public Transform pillowSocket;
	public SingleUnityLayer playerLayer;
	public Transform visual;
	public float healthRegenPerSecond = 0.45f;

	public Animator pillowAttack;
	public Animator foxAnimator;
	public Transform respawnPoint;

	public float throwPressLength;

	[Header("Physics")]
	public float weight = 0.3f;
	public float walkAcceleration = 5;
	public float airMoveAcceleration = 5;
	public float jumpAcceleration = 50;
	public float wallJumpAcceleration = 50;
	public float gravityAcceleration = 9.81f;
	public float maxWalkVelocity = 10;
	public float maxFlyVelocity = 70f;
	public float maxWallVelocity = 70f;

	public float throwForce = 50;

	Dictionary<Collider2D, Collision2D> collisions = new Dictionary<Collider2D, Collision2D>();
	Rigidbody2D rb;
	float actionPressLength;
	float forwardY;
	Pillow pillow;
	float health = 1;
	CollisionState previousState;

	public float GetForce(float acceleration) {
		return acceleration * weight;
	}
	public Vector2 GetForce(Vector2 acceleration) {
		return acceleration * weight;
	}
	public Vector2 GetDragForce(float maxSpeed, float acceleration) {
		return GetForce(rb.velocity * (-acceleration / maxSpeed));
	}


	void Start() {
		rb = GetComponent<Rigidbody2D>();
		gameObject.layer = playerLayer.LayerIndex;
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild(i).gameObject.layer = playerLayer.LayerIndex;
		}
		Respawn();

	}

	//fixedupdate
	void ApplyFixedMoveForces(float walkDirection) {
		if (IsOnGround()) {
			Vector2 force = GetForce(walkDirection * Vector2.right * walkAcceleration);
			rb.AddForce(force);

			//forces on object
			foreach (var c in collisions) {
				if (c.Value.contacts[0].normal.y > 0.4f) { //ground
					Rigidbody2D body = c.Value.collider.GetComponent<Rigidbody2D>();
					if (body) {
						body.AddForceAtPosition(-force / 4, c.Value.contacts[0].point);
					}
				}
			}
		}
		else if (IsInAir()) {
			rb.AddForce(GetForce(walkDirection * Vector2.right * airMoveAcceleration));
		}
		else if (IsOnWall()) {

		}
	}

	void ApplyGravity() {
		rb.AddForce(GetForce(gravityAcceleration) * Vector2.down);
	}
	void ApplyDrag() {
		if (IsOnGround()) {
			rb.AddForce(GetDragForce(maxWalkVelocity, walkAcceleration));
		}
		else if (IsInAir()) {
			rb.AddForce(
					GetDragForce(maxFlyVelocity,
					new Vector2(airMoveAcceleration, gravityAcceleration).magnitude)
				);
		}
		else if (IsOnWall()) {
			rb.AddForce(GetDragForce(maxWalkVelocity, gravityAcceleration));
		}
		else {
			Debug.LogWarning("NOT ON WALL AIR OR GROUND! WHATS HAPPENING???");
		}
	}
	//renderupdate
	void ApplyJumpForces() {
		Vector2 WallNormal = (IsOnWall()) ? collitionNormal() : Vector2.zero;
		Vector2 force =
				GetForce(jumpAcceleration * Vector2.up) +
				GetForce(wallJumpAcceleration * WallNormal);
		rb.AddForce(
			 force
			);

		//forces on object
		if (IsOnWall()) {
			foreach (var c in collisions) {
				Rigidbody2D body = c.Value.collider.GetComponent<Rigidbody2D>();
				if (body) {
					body.AddForceAtPosition(-force, c.Value.contacts[0].point);
				}
			}
		}

	}

	void FixedUpdate() {

		//Debug.Log(IsOnGround() ? "ground" : (IsInAir() ? "air" : "wall"));
		ApplyFixedMoveForces(HorizontalInput);
		ApplyGravity();
		ApplyDrag();


	}
	void Update() {
		List<Collider2D> toRemove = collisions.Where(e => !e.Key.enabled || e.Key.gameObject.layer == playerLayer.LayerIndex).Select(e => e.Key).ToList();
		for (int i = 0; i < toRemove.Count; i++) {
			collisions.Remove(toRemove[i]);
		}
		Action();

		health += healthRegenPerSecond * Time.deltaTime;
		health = Mathf.Min(health, 1);

		foxAnimator.SetFloat("movementspeed", Mathf.Abs(rb.velocity.x));
		foxAnimator.SetBool("Ground", IsOnGround());
		foxAnimator.SetBool("Wall", IsOnWall());
		foxAnimator.SetBool("Jump", false);

		if (!IsInAir())
			JumpCheck();

		Debug.Log(IsOnGround() ? "ground" : (IsInAir() ? "air" : "wall"));
		if (IsOnGround()) {


			if (Mathf.Abs(rb.velocity.x) > 0.2f) {
				GoingLeft = rb.velocity.x < 0;

			}

			ForwardVector = Vector2.right;

			if (rb.velocity.y <= 0) {
				visual.right = (GoingLeft) ? -ForwardVector : ForwardVector;
				transform.position = transform.position.SetY(collisions.Select(e => e.Value.contacts[0].point.y).Min());
			}


			if (previousState == CollisionState.WALL) {
				transform.position = transform.position.ChangeX(
					x => x + ((collitionNormal().x <0) ? 1 : -1) * ((visual.GetComponent<BoxCollider2D>().size.x / 2) + visual.transform.localPosition.x )
					);
			}


			previousState = CollisionState.GROUND;
		}
		else if (IsInAir()) {
			GoingLeft = rb.velocity.x < 0;

			ForwardVector = rb.velocity;

			visual.right = Vector2.Lerp(visual.right, (GoingLeft) ? -ForwardVector : ForwardVector, 0.1f);


			if (previousState == CollisionState.WALL) {
				transform.position = transform.position.ChangeX(
					x => x + ((GoingLeft)?1:-1) * ((visual.GetComponent<BoxCollider2D>().size.x / 2) + visual.transform.localPosition.x)
					);
			}

			previousState = CollisionState.AIR;
		}
		else if (IsOnWall()) {
			GoingLeft = collitionNormal().x > 0;

			ForwardVector = -collitionNormal();

			visual.right = (GoingLeft) ? -ForwardVector : ForwardVector;

			if (previousState != CollisionState.WALL) {
				visual.right = (GoingLeft) ? -ForwardVector : ForwardVector;
				transform.position = transform.position.SetX(collisions.Select(
					e => e.Value.contacts[0].point.x
				).Aggregate(
					(p, n) => (GoingLeft) ? ((p > n) ? p : n) : ((p < n) ? p : n)
				));
			}

			previousState = CollisionState.WALL;
		}

	}

	float HorizontalInput { get { return Input.GetAxis("Horizontal" + (int)player); } }
	public void Damage(float amount) {
		health -= amount;

		Debug.Log("dmg: " + health);
		if (health < 0) {
			Respawn();
			health = 1;
		}
	}

	void Action() {
		if (!HeldPillow) {


			if (Input.GetButtonDown("Action" + (int)player) && collisions.Any(e => e.Value.collider.GetComponent<Pillow>())) {
				Debug.Log("takey");
				collisions.Where(e => e.Value.collider.GetComponent<Pillow>()).First().Value.collider.GetComponent<Pillow>().Take(this);
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

		if (Input.GetButtonDown(ActionButton)) {
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
			ApplyJumpForces();

			foxAnimator.SetBool("Jump", true);
		}
	}


	Vector2 collitionNormal() {
		if (collisions.Count == 0) return Vector2.zero;
		return collisions.Select(e => e.Value.contacts[0].normal).Aggregate((p, t) => p + t).normalized;
	}

	public void OnCollisionEnter2D(Collision2D collision) {
		try {
			collisions.Add(collision.collider, collision);
		}
		catch (Exception) { }
	}

	public void OnCollisionExit2D(Collision2D collision) {
		collisions.Remove(collision.collider);
	}

	void Respawn() {
		transform.position = respawnPoint.position;
		rb.velocity = Vector3.zero;
	}

	public Pillow HeldPillow {
		get {
			return pillow;
		}
		set {
			pillow = value;
		}
	}


	bool IsOnGround() {
		return collitionNormal().y > 0.4;
	}
	bool IsInAir() {
		return !IsOnGround() && collisions.Where(e=>!e.Value.collider.GetComponent<Pillow>()).Count() == 0 || collisions.All(e => e.Value.contacts[0].normal.y < -0.4f);
	}

	bool IsOnWall() {
		return !(IsOnGround() || IsInAir());
	}


	bool GoingLeft {
		get {
			return transform.localScale.x < 0;
		}
		set {
			transform.localScale = new Vector3((value) ? -1 : 1, 1, 1);
		}
	}
	Vector2 ForwardVector {
		get {
			return new Vector2(Mathf.Cos(Mathf.Asin(forwardY)) * ((GoingLeft) ? -1 : 1), forwardY);
		}
		set {
			forwardY = value.normalized.y;
		}
	}
}
