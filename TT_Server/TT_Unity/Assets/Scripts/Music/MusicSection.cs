using System;
using UnityEngine;

public class MusicSection : MonoBehaviour
{
    
    public AudioSource[] Drums;
    public AudioSource[] Melody;
    public AudioSource BasicClick;

    public int IndexOne;
    public int IndexTwo;

    public bool SoundOnePlay;
    public bool SoundTwoPlay;

    public int SectionLength;

    public int sectionNum;
    
	void Start()
    {
        IndexOne = 0;
        IndexTwo = 0;

        SoundOnePlay = false;
        SoundTwoPlay = false;
    }
	
	void Update()
    {   
	}

    public int Play()
    {
        Debug.Log("SECTION:: " + sectionNum + " is playing");
        // TODO :: make this use switch statements to chose exactly which track to play (actually better yet, use a different function.

        //Metronome.BeatsInSection = 191;

        //Debug.Log("PLAYYINNNGGGGG SECTION");

        bool noMusic = true;

        // TODO: implement some kind of dictionary? maybe ["player"l.TrackEnabler[0]Counter
        if (GridDisplay.GetPlayerTokens(0) > 0)
        {
            //Debug.Log("Drums Play Again");

            IndexOne = getIndexFromColour(GridDisplay.GetPlayerTokenColour(0));

            Drums[IndexOne].time = 0.0f;
            Drums[IndexOne].Play();

            SoundOnePlay = true;
            noMusic = false;
        }
        if (GridDisplay.GetPlayerTokens(1) > 0)
        {
            //Debug.Log("MelodyPlays Again");

            IndexTwo = getIndexFromColour(GridDisplay.GetPlayerTokenColour(1));

            Melody[IndexTwo].time = 0.0f;
            Melody[IndexTwo].Play();

            SoundTwoPlay = true;
            noMusic = false;
        }

        if (noMusic && !(SoundOnePlay || SoundTwoPlay))
        {
            BasicClick.Play();
            //Debug.Log("Play BASIC CLICK!!");
        }

        return SectionLength;
    }

    internal void StopAll()
    {
        BasicClick.Stop();
        Drums[IndexOne].Stop();
        Melody[IndexTwo].Stop();
    }

    internal void startSound(int number, TokenSpawner.Colours colour)
    {
        //Debug.Log("START SOUND!!");

        BasicClick.Stop();
        //Debug.Log("stop BasicCLick!!");

        int newIndex = getIndexFromColour(colour);

        if (number == 0)
        {
            //Debug.LogFormat("START::drums: section:{0}, Genre:{1}", newIndex, number);

            Drums[IndexOne].Stop();
            Drums[newIndex].time = (float)(AudioSettings.dspTime - Metronome.SectionStartTime);

            Drums[newIndex].Play();

            SoundOnePlay = true;
            IndexOne = newIndex;
        }

        else
        {
            //Debug.LogFormat("START::melody: {0}, {1}", newIndex, number);

            Melody[IndexTwo].Stop();
            Melody[newIndex].time = (float)(AudioSettings.dspTime - Metronome.SectionStartTime);

            Melody[newIndex].Play();
            SoundTwoPlay = true;
            IndexTwo = newIndex;
        }
    }

    internal void stopSound(int number)
    {
        bool playBackground = false;
        if (number == 0)
        {
            Drums[IndexOne].Stop();
            //Debug.LogFormat("STOP::drums: Genre:{1}", number);
            SoundOnePlay = false;

            if (!SoundTwoPlay)
                playBackground = true;
        }
        else
        {
            Melody[IndexTwo].Stop();
            //Debug.LogFormat("STOP::melody: {1}", number);
            SoundTwoPlay = false;

            if (!SoundOnePlay)
                playBackground = true;
        }

        //Debug.Log("STOOOP SOUND");

        if (playBackground)
        {
            BasicClick.time = (float)(AudioSettings.dspTime - Metronome.SectionStartTime); ;
            BasicClick.Play();
        }
    }

    int getIndexFromColour(TokenSpawner.Colours type)
    {
        switch (type)
        {
            case TokenSpawner.Colours.RED:
                return 0;
                break;
            case TokenSpawner.Colours.GREEN:
                return 1;
                break;
            case TokenSpawner.Colours.YELLOW:
                return 2;
                break;
            case TokenSpawner.Colours.BLUE:
                return 3;
                break;
            default:
                return -1;
                break;
        }
    }
}