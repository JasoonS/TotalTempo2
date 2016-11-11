using UnityEngine;

public class SoundControl : MonoBehaviour
{
    public GridDisplay Grid;
    public MusicSection[] MusicSection;
    private MusicSection _currentMusicSection;
    public static MusicSection CurrentMusicSection { get { return Instance._currentMusicSection; } }

    public double Frequency = 440;
    public double Gain = 1;
    public bool ManipulateSound = false;

    private double _increment;
    private double _phase;
    private double _sampling_frequency = 48000;

    private int _currentSection;
    //public static int CurrentSection { get { return Instance._currentSection; } }

    private static SoundControl _soundControl;

    public static SoundControl Instance
    {
        get
        {
            if (!_soundControl)
            {
                _soundControl= FindObjectOfType(typeof(SoundControl)) as SoundControl;

                if (!_soundControl)
                {
                    Debug.LogError("You need to have at least one active Event Manager script in your scene.");
                }
            }
            return _soundControl;
        }
    }

    void Start ()
    {
        _currentSection = 1;

        _currentMusicSection = MusicSection[0];
    }
    
    //TODO:: NOTE:: this is a placeholder function for until we have a more advanced system.

    public int nextSection()
    {
        //Debug.Log("NEXT SECTION CALLED!!");

        _currentSection = (_currentSection + 1) % 2;

        MusicSection[0].Play();

        return 191;
    }

    // NOTE: the below is purely experimental code. Unity allows you to apply audio filtering to sound.
    //       It is very cool. Just not something we need in this game. :)
    // Apply audio effects to the sound for certain instances.
    //void OnAudioFilterRead(float[] data, int channels)
    //{
    //  if (manipulateSound){
    //    // update increment in case frequency has changed
    //    increment = frequency * 2 * Math.PI / sampling_frequency;
    //    for (var i = 0; i < data.Length; i = i + channels)
    //    {
    //      phase = phase + increment;
    //    // this is where we copy audio data to make them “available” to Unity
    //      data[i] = data[i] * (float)(gain*Math.Sin(phase));
    //    // if we have stereo, we copy the mono data to each channel
    //      if (channels == 2) data[i + 1] = data[i];
    //      if (phase > 2 * Math.PI) phase = 0;
    //    }
    //  }
    //}
}