using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Character : MonoBehaviour {
	public enum Player {
		Player1 = 1,
		Player2 = 2
	}

	public Player player;
	Rigidbody2D rb;

	public float moveForce = 5;
	public float jumpForce = 50;
	public float wallForce = 50;

	Dictionary<Collider2D,Collision2D> collitions = new Dictionary<Collider2D, Collision2D>();

	void Start() {
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate() {
		if(!IsInAir())
			FixedMove();
	}
	void Update() {

		if (!IsInAir())
			UpdateMove();
	}
	void FixedMove() {
		rb.AddForce(Vector2.right * Input.GetAxis("Horizontal" + (int)player) * moveForce);
	}
	void UpdateMove() {

		Vector2 collitionNormal = collitions.Select(e => e.Value.contacts[0].normal).Aggregate((p, t) => p + t);
		collitionNormal.y = 0;
        Debug.DrawRay(transform.position, collitionNormal,Color.green);
		if (Input.GetButtonDown("Jump" + (int)player)) {
            rb.AddForce(jumpForce * Vector2.up + wallForce * collitionNormal);
		}
	}
	bool IsInAir() {
		return collitions.Count == 0;
    }
	public void OnCollisionEnter2D(Collision2D collision) {

		collitions.Add(collision.collider, collision);
	}

	public void OnCollisionExit2D(Collision2D collision) {
		collitions.Remove(collision.collider);
	}
}
