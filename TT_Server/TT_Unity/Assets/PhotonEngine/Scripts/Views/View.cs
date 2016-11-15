using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class View : MonoBehaviour, IView
{
    public virtual void Awake()
    {
        Controller = new ViewController(this);
    }

    public virtual void OnApplicationQuit()
    {
        Controller.ApplicationQuit();
    }

    #region Implementation of IView

    public abstract IViewController Controller { get; protected set; }

    public void LogDebug(string message)
    {
        Debug.Log(message);
    }

    public void LogError(string message)
    {
        Debug.LogError(message);
    }

    public void LogError(Exception exception)
    {
        Debug.LogError(exception.ToString());
    }

    public void LogInfo(string message)
    {
        Debug.Log(message);
    }

    public void Disconnected(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log(message);
        }

        if (SceneManager.GetActiveScene() != SceneManager.GetSceneAt(1))
        {
            SceneManager.LoadScene(1);
        }
    }

    #endregion
}