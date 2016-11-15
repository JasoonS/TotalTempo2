using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TrackManager : MonoBehaviour {

    private static TrackManager _trackManager;

    public List<TrackDetails> _points = new List<TrackDetails>();

    //public int NumLaps;

    // I just made it public so you can see it in the editor...
    public int _trackIndex = 0;
    public static int TrackIndex {
        get { return Instance._trackIndex; }
        set { Instance._trackIndex = value; }
    }

    public static TrackDetails Track {
      get { return Instance._points[Instance._trackIndex]; }
    }

    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static TrackManager Instance
    {
        get
        {
            if (!_trackManager)
            {
                _trackManager = FindObjectOfType(typeof(TrackManager)) as TrackManager;

                if (!_trackManager)
                {
                    Debug.LogError("You need to have at least one active 'Track Manager' script in your scene.");
                }
                else
                {
                    _trackManager.Init();
                }
            }
            return _trackManager;
        }
    }

    void Start()
    {
        Init();
        // Debug.Log("TRACK MANAGER::" + _points.Count);
    }

    void Init()
    {
        _trackIndex = GameInstanceDataScript.TrackNumber;
        TrackDetails[] track_details = GetComponentsInChildren<TrackDetails>();

        _points.Clear();
        foreach (TrackDetails track in track_details)
        {
            _points.Add(track);
        }
    }
}
