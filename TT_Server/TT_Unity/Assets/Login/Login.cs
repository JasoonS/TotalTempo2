public class Login : View
{
    private LoginController _controller;

    public override IViewController Controller { get { return _controller; } protected set { _controller = value as LoginController; } }

    public string ServerAddress = "127.0.0.1:5055";
    public string ApplicationName = "TT_Server";

    public bool isServer = true;

    //public bool loggingIn = false;

    void Start()
    {
        _controller = new LoginController(this);

        PhotonEngine.UseExistingOrCreateNewPhotonEngine(ServerAddress, ApplicationName, isServer);

        //string[] arglist = new string[0];

        //if (Application.srcValue.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
        //{
        //    arglist = Application.srcValue.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        //}

        //if (arglist.Length == 2)
        //{
        //    _controller.SendLogin(arglist[0], arglist[1]);

        //    loggingIn = true;
        //}
    }

    public override void Awake()
    {
        // Leave this defined as empty.
    }
}