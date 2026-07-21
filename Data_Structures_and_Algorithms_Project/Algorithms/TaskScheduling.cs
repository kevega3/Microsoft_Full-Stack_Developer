namespace Algorithms;

public sealed record ScheduledTask(string Id, int Priority, DateTimeOffset ScheduledAt, Action Work);

public sealed class TaskSchedulerQueue
{
    private readonly PriorityQueue<ScheduledTask, TaskPriority> _queue = new();
    private long _sequence;

    public int Count => _queue.Count;

    public void Enqueue(ScheduledTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        _queue.Enqueue(task, new TaskPriority(task.Priority, task.ScheduledAt, _sequence++));
    }

    public ScheduledTask Dequeue() => _queue.Dequeue();

    public bool TryDequeue(out ScheduledTask? task) => _queue.TryDequeue(out task, out _);

    private readonly record struct TaskPriority(int Priority, DateTimeOffset ScheduledAt, long Sequence) : IComparable<TaskPriority>
    {
        public int CompareTo(TaskPriority other)
        {
            var priorityComparison = other.Priority.CompareTo(Priority);
            if (priorityComparison != 0)
                return priorityComparison;

            var dateComparison = ScheduledAt.CompareTo(other.ScheduledAt);
            return dateComparison != 0 ? dateComparison : Sequence.CompareTo(other.Sequence);
        }
    }
}

public sealed record TaskExecutionResult(string Id, bool Succeeded, string? ErrorMessage);

public sealed class TaskExecutor(Action<string>? log = null)
{
    private readonly Action<string> _log = log ?? (_ => { });

    public IReadOnlyList<TaskExecutionResult> Execute(TaskSchedulerQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        var results = new List<TaskExecutionResult>(queue.Count);

        while (queue.TryDequeue(out var task))
        {
            try
            {
                task!.Work();
                results.Add(new TaskExecutionResult(task.Id, true, null));
            }
            catch (Exception exception)
            {
                _log($"Task '{task!.Id}' failed: {exception.Message}");
                results.Add(new TaskExecutionResult(task.Id, false, exception.Message));
            }
        }

        return results;
    }
}
