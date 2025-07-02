namespace ThingsGateway.Gateway.Application
{
    public interface IScheduledTask
    {
        void SetNext(int interval);
        void Start();
        void Stop();

    }




}