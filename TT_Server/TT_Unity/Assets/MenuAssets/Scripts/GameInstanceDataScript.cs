using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameInstanceDataScript : MonoBehaviour {

	private static GameInstanceDataScript _vehicleManager;

	// Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
	public static GameInstanceDataScript Instance
	{
		get
		{
			if (!_vehicleManager)
			{
				_vehicleManager = FindObjectOfType(typeof(GameInstanceDataScript)) as GameInstanceDataScript;

				if (!_vehicleManager)
				{
					Debug.LogError("You need to have at least one active Game Instance data script in your scene.");
				}
			}
			return _vehicleManager;
		}
	}
		
	// Single Player Input Field Variables
	// ###################################
	[SerializeField] private int trackNo;
	[SerializeField] private int noOfAI;
	[SerializeField] private int noOfSelectedTokens;

	[SerializeField] private Dropdown trackDropdown;
	[SerializeField] private Dropdown AIDropdown;
	[SerializeField] private Dropdown tokenDropdown;

	// Create Server Input Field Variables
	// ###################################
	[SerializeField] private string gameName;

	[SerializeField] private InputField create_server_gameName; 
	[SerializeField] private Dropdown create_server_trackDropdown;
	[SerializeField] private Dropdown create_server_AIDropdown;

	// Connect to Server Input Field Variables
	// ###################################
	[SerializeField] private string IPAddress;

	[SerializeField] private InputField connect_server_ipAddress; 



	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}

	void Start() {
		// Single Player Input Field Options
		// #################################
		trackDropdown.onValueChanged.AddListener(delegate {
			trackDropdownValueChangedHandler(trackDropdown);
		});

		AIDropdown.onValueChanged.AddListener(delegate {
			aiDropdownValueChangedHandler(AIDropdown);
		});

		tokenDropdown.onValueChanged.AddListener(delegate {
			tokenDropdownValueChangedHandler(tokenDropdown);
		});


		// Create Server Input Field Options
		// #################################
		create_server_trackDropdown.onValueChanged.AddListener(delegate {
			create_server_trackDropdownValueChangedHandler(create_server_trackDropdown);
		});

		create_server_AIDropdown.onValueChanged.AddListener(delegate {
			create_server_aiDropdownValueChangedHandler(create_server_AIDropdown);
		});

		create_server_gameName.onValueChanged.AddListener(delegate {
			create_server_gameNameValueChangedHandler(create_server_gameName);
		});


		// Connect to Server Input Field Options
		// #####################################
		connect_server_ipAddress.onValueChanged.AddListener(delegate {
			connect_server_ipAddressValueChangedHandler(connect_server_ipAddress);
		});
	}



	// Single Player Input Field Options
	// #################################
	private void trackDropdownValueChangedHandler(Dropdown target) {
		Debug.Log("selected: "+target.value);
		trackNo = target.value;
	}
	private void aiDropdownValueChangedHandler(Dropdown target) {
		Debug.Log("selected: "+target.value);
		noOfAI = target.value;
	}
	private void tokenDropdownValueChangedHandler(Dropdown target) {
		Debug.Log("selected: "+target.value);
		noOfSelectedTokens = target.value;
	}



	// Create Server Input Field Options
	// #################################
	private void create_server_trackDropdownValueChangedHandler(Dropdown target) {
		Debug.Log("selected: "+target.value);
		trackNo = target.value;
	}
	private void create_server_aiDropdownValueChangedHandler(Dropdown target) {
		Debug.Log("selected: "+target.value);
		noOfAI = target.value;
	}
	private void create_server_gameNameValueChangedHandler(InputField target) {
		Debug.Log("selected: "+target.text);
		gameName = target.text;
	}



	// Connect to Server Input Field Options
	// #####################################
	private void connect_server_ipAddressValueChangedHandler(InputField target) {
		Debug.Log("selected: "+target.text);
		IPAddress = target.text;
	}


	public void SetDropdownIndex(int index) {
		trackDropdown.value = index;
	}
}
