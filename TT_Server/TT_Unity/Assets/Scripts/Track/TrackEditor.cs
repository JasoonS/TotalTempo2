using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]
public class TrackEditor : MonoBehaviour {

    private static TrackEditor _trackEditor;

    public List<TrackDetails> _points = new List<TrackDetails>();

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
    public static TrackEditor Instance
    {
        get
        {
            if (!_trackEditor)
            {
                _trackEditor = FindObjectOfType(typeof(TrackEditor)) as TrackEditor;

                if (!_trackEditor)
                {
                    Debug.LogError("You need to have at least one active 'Track Manager' script in your scene.");
                }
                else
                {
                    _trackEditor.Init();
                }
            }
            return _trackEditor;
        }
    }

    void Start()
    {
        Init();
        // Debug.Log("TRACK MANAGER::" + _points.Count);
    }

    void Init()
    {
        TrackDetails[] track_details = GetComponentsInChildren<TrackDetails>();

        _points.Clear();
        foreach (TrackDetails track in track_details)
        {
            _points.Add(track);
        }
    }
}
