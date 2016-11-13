using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleManager : MonoBehaviour {
    private static VehicleManager _vehicleManager;

    private HoverMotor[] hoverMotors;
    private string[] ids;
    private string playerId;

<<<<<<< HEAD
=======
    private Dictionary<string, float> _carPosition;

>>>>>>> Dev
    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static VehicleManager Instance
    {
        get
        {
            if (!_vehicleManager)
            {
                _vehicleManager = FindObjectOfType(typeof(VehicleManager)) as VehicleManager;

                if (!_vehicleManager)
                {
                    Debug.LogError("You need to have at least one active Event Manager script in your scene.");
                }
                else
                {
                    _vehicleManager.Init();
                }
            }
            return _vehicleManager;
        }
    }

    // Initialise the instance. In particular the ids of all the vehicles are assigned and stored to be accessible.
    // The players id is stored separately for convenience and player specific functionality use.
    public void Init()
    {
        hoverMotors = FindObjectsOfType(typeof(HoverMotor)) as HoverMotor[];

        ids = new string[hoverMotors.Length - 1];
        int idIndex = 0;
        for (int i = 0; i < hoverMotors.Length; ++i)
        {
            if (hoverMotors[i].isPlayer)
            {
                playerId = hoverMotors[i].id;
            } else
            {
                ids[idIndex++] = hoverMotors[i].id;
            }
        }
    }

    public static void SetStatusPosition(string vehicleId, float distance)
    {
        Debug.Log(vehicleId + "Distance" + distance);
    }

    // Return the static instance's array of ids.
    public static string[] GetVehicleIds()
    {
        return Instance.ids;
    }

    // Check if the given id is the id of the player.
    public static bool IsPlayerById(string id)
    {
        return (Instance.playerId == id);
    }
}
