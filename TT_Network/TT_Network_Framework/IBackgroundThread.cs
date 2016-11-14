namespace TT_Network_Framework
{
    public interface IBackgroundThread
    {
        void Setup();

        void Run(object threadContext);

        void Stop();
    }
}