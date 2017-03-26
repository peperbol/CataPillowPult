using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PillowCounterDisplay : MonoBehaviour {
	public PillowCounter counter;
	public Text text;
	public Animator animator;
	public float count;
	public void Update() {
		
		if (count != counter.pillows.Count) {
			count = counter.pillows.Count;
			text.text = count.ToString();
			animator.SetTrigger("refresh");
        }
    }
}
