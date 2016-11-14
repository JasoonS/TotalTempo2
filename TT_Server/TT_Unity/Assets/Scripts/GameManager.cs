using System.Collections.Generic;

using UnityEngine;

using UnityStandardAssets.Cameras;

public class GameManager : MonoBehaviour
{
    public bool IsOnline = false;

    public int AiCount = 0;

    public GameObject PlayerPrefabOffline;

    public GameObject AiVehiclePrefab;

    private GameObject _loginObject;

    private List<Transform> _spawnsAvailableOffline;

    private AutoCam _camera;

    void Awake()
    {
        _loginObject = transform.GetChild(0).gameObject;

        if (IsOnline)
        {
            _loginObject.SetActive(true);
        }

        else
        {
            _loginObject.SetActive(false);

            // Setup spawn positions.

            _spawnsAvailableOffline = new List<Transform>();

            Transform playerSpawns = transform.GetChild(1);

            for (int i = 0; i < playerSpawns.childCount; ++i)
            {
                _spawnsAvailableOffline.Add(playerSpawns.GetChild(i));
            }

            // Spawn player at spawn position 0.

            Transform playerSpawn = _spawnsAvailableOffline[0];

            _spawnsAvailableOffline.RemoveAt(0);

            GameObject player = (GameObject)Instantiate(PlayerPrefabOffline, new Vector3(playerSpawn.position.x, playerSpawn.position.y + 8, playerSpawn.position.z), playerSpawn.rotation);

            _camera = GameObject.Find("Cameras").transform.GetChild(0).GetComponent<AutoCam>();

            _camera.Target = player.transform;

            // Spawn AI vehicles at subsequent available spawn positions.

            for (int i = 0; i < AiCount; ++i)
            {
                playerSpawn = _spawnsAvailableOffline[0];

                _spawnsAvailableOffline.RemoveAt(0);

                GameObject aiVehicle = (GameObject)Instantiate(AiVehiclePrefab, new Vector3(playerSpawn.position.x, playerSpawn.position.y + 8, playerSpawn.position.z), playerSpawn.rotation);
            }
        }
    }
}