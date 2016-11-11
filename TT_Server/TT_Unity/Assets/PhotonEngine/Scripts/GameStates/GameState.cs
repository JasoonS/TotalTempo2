using ExitGames.Client.Photon;

public class GameState : IGameState
{
    protected GameState() { }

    //Do nothing
    public virtual void OnUpdate() { }

    // Do nothing
    public virtual void SendOperation(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt) { }
}