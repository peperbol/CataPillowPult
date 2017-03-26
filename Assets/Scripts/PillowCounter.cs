using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillowCounter : MonoBehaviour {
	public List<Pillow> pillows = new List<Pillow>();

	public void OnTriggerEnter2D(Collider2D collision) {
		Pillow p = collision.GetComponent<Pillow>();
		if (p) {
			pillows.Add(p);
        }
    }

	public void OnTriggerExit2D(Collider2D collision) {
		Pillow p = collision.GetComponent<Pillow>();
		if (p) {
			pillows.Remove(p);
		}
	}
}
