  ﻿using UnityEngine;
  using System.Collections;

  public class AestheticManager : MonoBehaviour
  {
    public Material[] skyboxes;
    private static AestheticManager _aestheticManager;

      // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
      public static AestheticManager Instance
      {
          get
          {
              if (!_aestheticManager)
              {
                  _aestheticManager = FindObjectOfType(typeof(AestheticManager)) as AestheticManager;

                  if (!_aestheticManager)
                  {
                      Debug.LogError("You need to have at least one active 'Aesthetic Manager' script in your scene.");
                  }
                  else
                  {
                      _aestheticManager.Init();
                  }
              }
              return _aestheticManager;
          }
      }

      void Start()
      {
          Init();
      }

      void Init()
      {
          int index = GameInstanceDataScript.TrackNumber;

          RenderSettings.skybox = skyboxes[index];
      }
  }
