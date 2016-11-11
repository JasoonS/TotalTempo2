namespace TT_Network_Framework
{
    public interface IConnectionCollection<Server, Client>
    {
        void OnConnect(Server serverPeer);

        void OnDisconnect(Server serverPeer);

        void OnClientConnect(Client clientPeer);

        void OnClientDisconnect(Client clientPeer);

        Server GetServerByType(int serverType);
    }
}