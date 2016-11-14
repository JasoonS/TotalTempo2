using ExitGames.Client.Photon;

public class WaitingForConnection : GameState
{
    public WaitingForConnection() : base() { }

    public override void OnUpdate()
    {
        PhotonEngine.Instance.Peer.Service();
    }

    public override void SendOperation(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
    {
        PhotonEngine.Instance.Peer.OpCustom(operationRequest, sendReliable, channelId, encrypt);
    }
}