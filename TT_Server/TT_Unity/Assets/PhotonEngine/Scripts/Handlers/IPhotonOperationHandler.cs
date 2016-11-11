// Interface to setup network operation handler functionality.

using ExitGames.Client.Photon;

public interface IPhotonOperationHandler
{
    byte Code { get; }
    void HandleResponse(OperationResponse response);
    void OnHandleResponse(OperationResponse response);
}