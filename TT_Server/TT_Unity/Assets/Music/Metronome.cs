using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Metronome : MonoBehaviour
{
    private static Metronome _metronome;

    public double Bpm;
    public int BeatsPerBar;
    public SoundControl SoundPlayer;

    private int _currentBeat = 0;
    private int _prevBeat = 0;
    private double _startTime = -1.0F;
    private bool _running = false;
    private bool _isBeatOne = false;

    public static int CurTokenCycle;

    public static double SectionStartTime;

    public Text TokenCycle;

    public int NumTokenBeats;

    private int _currentTokenBeat;

    public static int BeatsInSection;
    public int CurrentCountInSection;

    public List<List<string>> Listeners;

    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static Metronome Instance
    {
        get
        {
            if (!_metronome)
            {
                _metronome = FindObjectOfType(typeof(Metronome)) as Metronome;

                if (!_metronome)
                {
                    Debug.LogError("You need to have at least one active Event Manager script in your scene.");
                }
            }
            return _metronome;
        }
    }

    public void Awake()
    {
        //currentCountInSection = beatsInSection; //TODO:: NOTE:: this is temporary since we want it to initialise play during the update loop.

        _running = true;

        CurTokenCycle = 0;
        _startTime = AudioSettings.dspTime;// + (5*(60.0F / bpm));

        _currentTokenBeat = 0;

        // TODO:: set this to be the size of the longest passage.
        Listeners = new List<List<string>>(500);

        for (int i = 0; i < Listeners.Capacity; ++i)
            Listeners.Add(new List<string>());
    }

    void Update()
    {
        _prevBeat = _currentBeat;
        _currentBeat = (int)((AudioSettings.dspTime - _startTime) / (60.0F / Bpm)) % BeatsPerBar;

        _isBeatOne = (_prevBeat == BeatsPerBar - 1 && _currentBeat == 0);

        // this only enteres the loop on a beat (when there is a change of beat)
        if (_prevBeat != _currentBeat)
        {

            //TODO:: make sure there aren't any of by one errors etc.. // TODO:: we could also use bars instead.
            if (BeatsInSection == CurrentCountInSection++)
            {
                CurrentCountInSection = 0;
                SectionStartTime = AudioSettings.dspTime;
                BeatsInSection = SoundPlayer.nextSection();
            }

            if (_currentBeat == BeatsPerBar)
            {
                _currentTokenBeat = (_currentTokenBeat + 1) % NumTokenBeats;
            }

            if (_currentBeat == 1)
            {
                // TODO:: use eventManager (( haha, I wrote this ages ago, before I even knew what evet manager was. I think I was misguided, rather just use a static method in Metronome)
                // TODO:: move this into the HUD... (Nikhil...)
                EventManager.TriggerEvent("UpdateBeat");

                // TODO:: create a function in TokenSpawner that does this your you. Keep the metronome clean of such logic.

                for (int i = 0; i < TokenSpawner.Instance.Tokens.Count; i++)
                {
                    TokenSpawner.Instance.Tokens[i].TokenScript.enableToken();
                }

                CurTokenCycle = (CurTokenCycle + 1) % 2;

                //DynamicElements.updateDynamicElements();

                TokenCycle.text = ":" + CurTokenCycle.ToString() + ":";
            }
            if (_currentBeat == 0)
            {
                // TODO:: use EventManager, refactor.
                for (int i = 0; i < TokenSpawner.Instance.Tokens.Count; i++)
                {
                    TokenSpawner.Instance.Tokens[i].TokenScript.disableToken();
                }
            }

            if (Listeners[CurrentCountInSection] != null)
            {
                foreach (string item in Listeners[CurrentCountInSection])
                {
                    EventManager.TriggerEvent(item);
                }
                Listeners[CurrentCountInSection] = new List<string>();
            }
        }
    }

    public static void addSpawner(string id)
    {
        int spawnBeat = (Instance.CurrentCountInSection + (8 - Instance._currentBeat)) % BeatsInSection;
        Instance.Listeners[spawnBeat].Add(id);
    }
}