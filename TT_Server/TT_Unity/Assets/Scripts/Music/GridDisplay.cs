using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*TODO RENAME*/

// TODO:: make grid display work with all the players - this object NEEDS to be synced well over the network!

// Class that contains and controlls information pertaining to tokens hit by each vehicle and the resulting state (over time) as it is updated by the metronome.
public class GridDisplay : MonoBehaviour
{
    private VehicleTokenStatus[] _trackEnabler;

    private Dictionary<string, VehicleTokenStatus[]> _vehicleDictionary;

    static RectTransform ParentPanelStat;
    public RectTransform ParentPanel;

    public Slider CountSliderOne;
    public Slider CountSliderTwo;

    private static GridDisplay _gridDisplay;

    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static GridDisplay Instance
    {
        get
        {
            if (!_gridDisplay)
            {
                _gridDisplay = FindObjectOfType(typeof(GridDisplay)) as GridDisplay;

                if (!_gridDisplay)
                {
                    Debug.LogError("You need to have at least one active Event Manager script in your scene.");
                }
                else
                {
                    //_gridDisplay.Initialise(); // TODO:: remove the 'Start' method.
                }
            }
            return _gridDisplay;
        }
    }

    void Start()
    {
		Initialise();
    }

	public void Initialise()
	{
        _trackEnabler = new VehicleTokenStatus[2];

        // TODO:: do this for each car, store in a more convenient 2D array or something
        _trackEnabler[0] = new VehicleTokenStatus(0, CountSliderOne);
        _trackEnabler[1] = new VehicleTokenStatus(1, CountSliderTwo);

        _trackEnabler[0].ColourChange = GameObject.FindWithTag("BarFillOne").GetComponent<Image>();
        _trackEnabler[1].ColourChange = GameObject.FindWithTag("BarFillTwo").GetComponent<Image>();

        string[] ids = VehicleManager.GetVehicleIds();

        if (_vehicleDictionary == null)
        {
            _vehicleDictionary = new Dictionary<string, VehicleTokenStatus[]>();
        }

        foreach (string id in ids)
        {
            VehicleTokenStatus[] _vehicleStatus = new VehicleTokenStatus[2];
            _vehicleStatus[0] = new VehicleTokenStatus(0);
            _vehicleStatus[1] = new VehicleTokenStatus(1);
            _vehicleDictionary.Add(id, _vehicleStatus);
        }
    }

    public static int GetPlayerTokens (int token)
    {
        return Instance._trackEnabler[token].Counter;
    }

    // TODO:: refacter tokenSpawn.Colours (why is it there?)
    public static TokenSpawner.Colours GetPlayerTokenColour(int token)
    {
        return Instance._trackEnabler[token].CurrColour;
    }

    // TODO:: implement this properrly for different Vehicles. (ie 'string vehicle' is just a placeholder for the time being)
    public static void HitTokenUpdate(string vehicle, int token, TokenSpawner.Colours tokenColour)
    {
        if (VehicleManager.IsPlayerById(vehicle))
        {
            Instance._trackEnabler[token].TokenHit(tokenColour);
        } else
        {
            Instance._vehicleDictionary[vehicle][token].TokenHit(tokenColour);
        }
    }

    public class VehicleTokenStatus
    {
		public Slider CountSlider;

        public int ButtonNum;
        public bool Selected = false;

        ColorBlock SelectedColour;
        ColorBlock NormalColour;

        public GameObject ButtonGameObject;
        public Button ButtonDisplayIcon;

        public TokenSpawner.Colours CurrColour;

        public int Counter;

        public Image ColourChange;

        public VehicleTokenStatus(int id)
        {
            ButtonNum = id;
            Counter = -1;
            EventManager.StartListening("UpdateBeat", beatSectionUpdate);
        }

		public VehicleTokenStatus(int id, Slider countSlide)
        {
            ButtonNum = id;
            Counter = -1;
            CountSlider = countSlide;
            EventManager.StartListening("UpdateBeat", beatSectionUpdate);
		}


        public void TokenHit(TokenSpawner.Colours tokenColour)
        {
            if (Counter < 0 || tokenColour != CurrColour)
            {
                Counter = 20;
            } else
            {
                Counter += 5;
            }

            CurrColour = tokenColour;

            if (CountSlider != null)
            {
                CountSlider.maxValue = Counter;
                CountSlider.value = Counter;

                SetColours(tokenColour);

                SoundControl.CurrentMusicSection.startSound(ButtonNum, tokenColour);
            }
        }

        void SetColours(TokenSpawner.Colours tokenColour)
        {
            switch (tokenColour)
            {
            case TokenSpawner.Colours.RED:
                ColourChange.color = Color.red;
                break;
            case TokenSpawner.Colours.GREEN:
                ColourChange.color = Color.green;
                break;
            case TokenSpawner.Colours.YELLOW:
                ColourChange.color = Color.yellow;
                break;
            case TokenSpawner.Colours.BLUE:
                ColourChange.color = Color.blue;
                break;
            default:
                SelectedColour = ButtonDisplayIcon.colors;
                break;
            }
				
        }

        public void beatSectionUpdate()
        {
            --Counter;
            if (CountSlider != null)
            {
                if (Counter > 0)
                {
                    CountSlider.value = Counter;

                    //ButtonDisplayIcon.GetComponentInChildren<Text>().text = counter.ToString();
                }

                else if (Counter == 0)
                {
                    CountSlider.value = Counter;

                    //ButtonDisplayIcon.GetComponentInChildren<Text>().text = counter.ToString();

                    SoundControl.CurrentMusicSection.stopSound(ButtonNum);

                    //ButtonDisplayIcon.image.color = Color.white;
                }
            }
        }
    };
}