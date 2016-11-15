using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class VehicleManager : MonoBehaviour {
    private static VehicleManager _vehicleManager;

    private HoverMotor[] hoverMotors;
    private string[] ids;
    private string playerId;

    private Dictionary<string, float> _carPosition;

    public Text _playerDistDisplay;

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
                    Debug.LogError("You need to have at least one active Vehicle Manager script in your scene.");
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
    // The players _id is stored separately for convenience and player specific functionality use.
    public void Init()
    {
        hoverMotors = FindObjectsOfType(typeof(HoverMotor)) as HoverMotor[];

        ids = new string[hoverMotors.Length - 1];
        int idIndex = 0;
        for (int i = 0; i < hoverMotors.Length; ++i)
        {
            if (hoverMotors[i].isPlayer)
            {
                playerId = hoverMotors[i]._id;
            } else
            {
                ids[idIndex++] = hoverMotors[i]._id;
            }
        }

        // Initialise the distance
        string message = 0.0f + "/" + GameInstanceDataScript.NumLaps;
        Instance._playerDistDisplay.text = message;

        //TODO:: shouldn't be here, just needs to be called.
        //GameOverScript.EnableGameOverMenu(false, "");
    }

    public static void SetStatusPosition(string vehicleId, float distance)
    {
        //Debug.Log(vehicleId + "Distance" + distance);
        if (distance > /* 0.1)// */ GameInstanceDataScript.NumLaps)
        {
            string message;
            if (IsPlayerById(vehicleId))
            {
                GameOverScript.EnableGameOverMenu(true, "YOU WIN");
            } else
            {
                GameOverScript.EnableGameOverMenu(true, "YOU LOSE");
            }
        }

        if (IsPlayerById(vehicleId))
        {
            string message = Math.Round(distance, 2) + "/" + GameInstanceDataScript.NumLaps;
            Instance._playerDistDisplay.text = message;
        }
    }

    // Return the static instance's array of ids.
    public static string[] GetVehicleIds()
    {
        return Instance.ids;
    }

    // Check if the given _id is the _id of the player.
    public static bool IsPlayerById(string id)
    {
        return (Instance.playerId == id);
    }

    public static void SetVehicleState(string id, HoverMotor.VehicleStates state)
    {
        for (int i = 0; i < Instance.hoverMotors.Length; ++i)
        {
            if (Instance.hoverMotors[i]._id == id)
            {
                Instance.hoverMotors[i].SetState(state);
            }
        }
    }
}
