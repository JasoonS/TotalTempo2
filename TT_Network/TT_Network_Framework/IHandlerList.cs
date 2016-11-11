namespace TT_Network_Framework
{
    interface IHandlerList<T>
    {
        bool RegisterHandler(IHandler<T> handler);

        bool HandleMessage(IMessage message, T peer);
    }
}