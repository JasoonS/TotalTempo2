using ExitGames.Client.Photon;

public interface IGameState
{
    void OnUpdate();
    void SendOperation(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt);
}