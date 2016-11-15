using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class LapTimer : MonoBehaviour {

	private static LapTimer _lapTime;

	// Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
	public static LapTimer Instance
	{
		get
		{
			if (!_lapTime)
			{
				_lapTime = FindObjectOfType(typeof(LapTimer)) as LapTimer;

				if (!_lapTime)
				{
					Debug.LogError("You need to have at least one active Game Instance data script in your scene.");
				} else
                {
                    _lapTime.Init();
                }
			}
			return _lapTime;
		}
	}
		
	// Use this for initialization
	private Text timerDisplay;
	private float startTime;
	private float ellapsedTime;

	void Awake(){
        Init();
	}

    void Init()
    {
        timerDisplay = GetComponent<Text>();
        startTime = Time.time;
        TimerStart();
    }
    // Update is called once per frame
    void Update () {
		TimerStart();

		float minutes = Mathf.Floor (ellapsedTime / 60);
		float seconds = Mathf.Floor (ellapsedTime - minutes * 60);
		float milliseconds = ellapsedTime - Mathf.Floor (ellapsedTime);
		milliseconds = Mathf.Floor (milliseconds * 1000.0f);

		timerDisplay.text = minutes.ToString () + ":" + seconds.ToString () + ":" + milliseconds.ToString();
	}
		
	void TimerStart(){
		ellapsedTime = Time.time - startTime;
	}
}
