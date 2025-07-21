namespace ThingsGateway.Gateway.Application
{
    public interface IScheduledTask
    {
        bool Change(int dueTime, int period);
        void SetNext(int interval);
        void Start();
        void Stop();
        public Int32 Period { get; }
        bool Enable { get; }
    }


}