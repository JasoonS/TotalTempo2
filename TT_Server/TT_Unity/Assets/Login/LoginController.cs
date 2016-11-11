using ExitGames.Client.Photon;

class LoginController : ViewController
{
    // Add any Login-scope operation handlers to the handlers array in the constructor.

    public LoginController(View controlledView, byte subOperationCode = 0) : base(controlledView, subOperationCode){}

    public void SendLogin(string username, string password)
    {
        SendOperation(new OperationRequest(), true, 0, false);
    }
}