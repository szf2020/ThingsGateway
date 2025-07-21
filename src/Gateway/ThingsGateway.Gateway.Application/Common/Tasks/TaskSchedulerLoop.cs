namespace ThingsGateway.Gateway.Application;

public class TaskSchedulerLoop
{
    private readonly List<IScheduledTask> Tasks;

    public TaskSchedulerLoop(List<IScheduledTask> tasks)
    {
        Tasks = tasks;
    }
    public int Count()
    {
        return Tasks.Count;
    }

    public void Start()
    {
        foreach (var task in Tasks)
        {
            task.Start();
        }
    }
    public bool Stoped => Tasks.All(a => !a.Enable);
    public void Stop()
    {
        foreach (var task in Tasks)
        {
            task.Stop();
        }
    }

    public void Add(IScheduledTask task)
    {
        Tasks.Add(task);
    }
    public void Remove(IScheduledTask task)
    {
        Tasks.Remove(task);
    }
}
