# Optimized Data Structures and Algorithms

## Project Title

**Optimized Data Structures and Algorithms**

## Optimized Binary Tree Implementation

```csharp
public sealed class AvlTree<T> where T : IComparable<T>
{
    private Node? _root;

    public void Add(T value) => _root = Insert(_root, value);

    private static Node Insert(Node? node, T value)
    {
        if (node is null) return new Node(value);

        var comparison = value.CompareTo(node.Value);
        if (comparison < 0) node.Left = Insert(node.Left, value);
        else if (comparison > 0) node.Right = Insert(node.Right, value);
        else return node;

        UpdateHeight(node);
        return Balance(node); // Keeps insertion and search at O(log n).
    }

    private static Node Balance(Node node)
    {
        var factor = Height(node.Left) - Height(node.Right);
        if (factor > 1)
        {
            if (Height(node.Left!.Left) < Height(node.Left.Right))
                node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }
        if (factor < -1)
        {
            if (Height(node.Right!.Right) < Height(node.Right.Left))
                node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }
        return node;
    }

    private static Node RotateRight(Node node)
    {
        var pivot = node.Left!;
        node.Left = pivot.Right;
        pivot.Right = node;
        UpdateHeight(node);
        UpdateHeight(pivot);
        return pivot;
    }

    private static Node RotateLeft(Node node)
    {
        var pivot = node.Right!;
        node.Right = pivot.Left;
        pivot.Left = node;
        UpdateHeight(node);
        UpdateHeight(pivot);
        return pivot;
    }

    private static int Height(Node? node) => node?.Height ?? 0;
    private static void UpdateHeight(Node node) =>
        node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;

    private sealed class Node(T value)
    {
        public T Value { get; } = value;
        public int Height { get; set; } = 1;
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }
}
```

## Optimized Task Scheduling Algorithm

```csharp
public sealed record ScheduledTask(
    string Id, int Priority, DateTimeOffset ScheduledAt, Action Work);

public sealed class TaskSchedulerQueue
{
    private readonly PriorityQueue<ScheduledTask, TaskPriority> _queue = new();
    private long _sequence;

    public void Enqueue(ScheduledTask task) =>
        _queue.Enqueue(task, new(task.Priority, task.ScheduledAt, _sequence++));

    public bool TryDequeue(out ScheduledTask? task) =>
        _queue.TryDequeue(out task, out _); // O(log n), without re-sorting.

    private readonly record struct TaskPriority(
        int Priority, DateTimeOffset Date, long Sequence) : IComparable<TaskPriority>
    {
        public int CompareTo(TaskPriority other)
        {
            var priority = other.Priority.CompareTo(Priority);
            if (priority != 0) return priority;
            var date = Date.CompareTo(other.Date);
            return date != 0 ? date : Sequence.CompareTo(other.Sequence);
        }
    }
}
```

## Optimized Sorting Algorithm

```csharp
public static void Sort(int[] values)
{
    ArgumentNullException.ThrowIfNull(values);
    Array.Sort(values); // Runtime-optimized, average O(n log n).
}
```

## Debugged Task Execution Code

```csharp
public static IReadOnlyList<TaskExecutionResult> Execute(TaskSchedulerQueue queue)
{
    var results = new List<TaskExecutionResult>();
    while (queue.TryDequeue(out var task))
    {
        try
        {
            task!.Work();
            results.Add(new(task.Id, true, null));
        }
        catch (Exception exception)
        {
            // A failed task is recorded without stopping the remaining tasks.
            results.Add(new(task!.Id, false, exception.Message));
        }
    }
    return results;
}

public sealed record TaskExecutionResult(
    string Id, bool Succeeded, string? ErrorMessage);
```

## Performance Report and Annotated Code

| Component | Before | Optimized |
|---|---|---|
| Binary tree | O(n) worst-case search | AVL: O(log n) |
| Scheduling | Repeated O(n log n) sorting | Priority queue: O(log n) |
| Sorting | Insertion sort: O(n2) | Array.Sort: average O(n log n) |
| Execution | One error stopped the batch | Errors are isolated per task |

```csharp
const int size = 20_000;
var random = new Random(2026); // Fixed seed for reproducibility.
var input = Enumerable.Range(0, size).Select(_ => random.Next()).ToArray();

var baseline = (int[])input.Clone(); // Identical inputs ensure a fair comparison.
var optimized = (int[])input.Clone();
var baselineTime = Measure(() => QuadraticSorter.Sort(baseline));
var optimizedTime = Measure(() => OptimizedSorter.Sort(optimized));

static double Measure(Action operation)
{
    var stopwatch = Stopwatch.StartNew();
    operation();
    stopwatch.Stop();
    return stopwatch.Elapsed.TotalMilliseconds;
}
```

**Result:** Insertion sort took **400.37 ms**, while `Array.Sort` took **11.94 ms** for 20,000 integers.

## LLM Contribution Reflection

Microsoft Copilot helped identify AVL balancing, priority-queue scheduling, optimized sorting, exception isolation, and reproducible benchmarking. I reviewed every suggestion and validated the final solution with xUnit tests, complexity analysis, and measured results.
