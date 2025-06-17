namespace ThingsGateway.Gateway.Application;

public class TaskSchedulerLoop
{
    public readonly List<ScheduledTask> Tasks;

    public TaskSchedulerLoop(List<ScheduledTask> tasks)
    {
        Tasks = tasks;
    }

    public void Start()
    {
        foreach (var task in Tasks)
        {
            task.Start();
        }
    }

    public void Stop()
    {
        foreach (var task in Tasks)
        {
            task.Stop();
        }
    }
    public void Change(int dueTime, int period)
    {
        foreach (var task in Tasks)
        {
            task.Change(dueTime, period);
        }
    }
}
