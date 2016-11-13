using UnityEngine;
using System.Collections;

public class ColorLerp : MonoBehaviour {

	public Color[] colors;
	public Color color;

	public int currentIndex;
	private int nextIndex;

	public float changeColorTime = 1.0f;
	private float lastChange = 0.0f;
	private float timer = 0.0f;

	// Use this for initialization
	void Start () {
		nextIndex = (currentIndex + 1)	% colors.Length;
	}
	
	// Update is called once per frame
	void Update () {
		timer = Time.deltaTime;
		if (timer > changeColorTime) {
			currentIndex = (currentIndex + 1) % colors.Length;
			nextIndex = (currentIndex + 1) % colors.Length;
			timer = 0.0f;
		}
		GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().color = new Color (255.0f, 0.0f, 0.0f, 1.0f);
	}
}
