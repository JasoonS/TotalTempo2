using ExitGames.Client.Photon;

public class Connected : GameState
{
    public Connected() : base() { }

    public override void OnUpdate()
    {
        PhotonEngine.Instance.Peer.Service();
    }

    public override void SendOperation(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
    {
        PhotonEngine.Instance.Peer.OpCustom(operationRequest, sendReliable, channelId, encrypt);
    }
}