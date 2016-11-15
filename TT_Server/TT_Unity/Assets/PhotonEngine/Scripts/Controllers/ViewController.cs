using System;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

using ExitGames.Client.Photon;

public class ViewController: IViewController
{
    private readonly View _controlledView;

    private readonly byte _subOperationCode;

    public View ControlledView { get { return _controlledView; } }

    private readonly Dictionary<byte, IPhotonOperationHandler> _operationHandlers = new Dictionary<byte, IPhotonOperationHandler>();
    private readonly Dictionary<byte, IPhotonEventHandler> _eventHandlers = new Dictionary<byte, IPhotonEventHandler>();

    public Dictionary<byte, IPhotonOperationHandler> OperationHandlers { get { return _operationHandlers; } }
    public Dictionary<byte, IPhotonEventHandler> EventHandlers { get { return _eventHandlers; } }

    public ViewController(View controlledView, byte subOperationCode = 0)
    {
        _controlledView = controlledView;
        _subOperationCode = subOperationCode;

        if (PhotonEngine.Instance == null)
        {
            SceneManager.LoadScene(1);
        }

        else
        {
            PhotonEngine.Instance.Controller = this;
        }

        _operationHandlers.Add(0, new PeerIdHandler(this));
        _operationHandlers.Add(1, new PlayerInputHandler(this));
        _operationHandlers.Add(2, new PlayerTransformHandler(this));
    }

    #region Implementation of IViewController

    public bool IsConnected { get { return PhotonEngine.Instance.State is Connected; } }

    public void ApplicationQuit()
    {
        PhotonEngine.Instance.Disconnect();
    }
        
    public void Connect()
    {
        if (!IsConnected)
        {
            PhotonEngine.Instance.Initialise();
        }
    }

    public void SendOperation(OperationRequest request, bool sendReliable, byte channelId, bool encrypt)
    {
        PhotonEngine.Instance.SendOp(request, sendReliable, channelId, encrypt);
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        _controlledView.LogDebug(string.Format("{0} - {1}", level, message));
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        IPhotonOperationHandler handler;

        if ((operationResponse.Parameters.ContainsKey(_subOperationCode) && (OperationHandlers.TryGetValue(Convert.ToByte(operationResponse.Parameters[_subOperationCode]), out handler))))
        {
            handler.HandleResponse(operationResponse);
        }

        else
        {
            OnUnexpectedOperationResponse(operationResponse);
        }
    }

    public void OnEvent(EventData eventData)
    {
        IPhotonEventHandler handler;

        if (EventHandlers.TryGetValue(eventData.Code, out handler))
        {
            handler.HandleEvent(eventData);
        }

        else
        {
            OnUnexpectedEvent(eventData);
        }
    }

    public void OnUnexpectedEvent(EventData eventData)
    {
        _controlledView.LogError(string.Format("Unexpected Event {0}", eventData.Code));
    }

    public void OnUnexpectedOperationResponse(OperationResponse operationResponse)
    {
        _controlledView.LogError(string.Format("Unexpected Operation Error {0} from operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode));
    }

    public void OnUnexpectedStatusCode(StatusCode statusCode)
    {
        _controlledView.LogError(string.Format("Unexpected Status {0}", statusCode));
    }

    public void OnDisconnected(string message)
    {
        _controlledView.Disconnected(message);
    }

    #endregion
}