using Algorithms;

namespace Algorithms.Tests;

public class AvlTreeTests
{
    [Fact]
    public void MaintainsBalanceAndFindsInsertedValues()
    {
        var tree = new AvlTree<int>();

        foreach (var value in new[] { 30, 20, 10, 25, 40, 50 })
            tree.Add(value);

        Assert.Equal(new[] { 10, 20, 25, 30, 40, 50 }, tree.InOrder());
        Assert.True(tree.Contains(25));
        Assert.False(tree.Contains(99));
        Assert.True(tree.IsBalanced);
        Assert.True(tree.Height <= 3);
    }

    [Fact]
    public void IgnoresDuplicateValues()
    {
        var tree = new AvlTree<int>();

        tree.Add(7);
        tree.Add(7);

        Assert.Equal(1, tree.Count);
    }
}

public class TaskSchedulerQueueTests
{
    [Fact]
    public void DequeuesHighestPriorityThenEarliestDate()
    {
        var queue = new TaskSchedulerQueue();
        var later = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);
        var earlier = later.AddMinutes(-1);

        queue.Enqueue(new ScheduledTask("low", 1, earlier, () => { }));
        queue.Enqueue(new ScheduledTask("urgent-later", 10, later, () => { }));
        queue.Enqueue(new ScheduledTask("urgent-earlier", 10, earlier, () => { }));

        Assert.Equal("urgent-earlier", queue.Dequeue().Id);
        Assert.Equal("urgent-later", queue.Dequeue().Id);
        Assert.Equal("low", queue.Dequeue().Id);
    }
}

public class SortingTests
{
    [Fact]
    public void OptimizedSortOrdersValuesWithoutChangingTheirContents()
    {
        var values = new[] { 8, 3, 5, 3, -1, 10 };

        OptimizedSorter.Sort(values);

        Assert.Equal(new[] { -1, 3, 3, 5, 8, 10 }, values);
    }
}

public class TaskExecutorTests
{
    [Fact]
    public void RecordsFailureAndContinuesWithFollowingTasks()
    {
        var queue = new TaskSchedulerQueue();
        var executed = new List<string>();
        queue.Enqueue(new ScheduledTask("fails", 2, DateTimeOffset.UtcNow, () => throw new InvalidOperationException("broken")));
        queue.Enqueue(new ScheduledTask("succeeds", 1, DateTimeOffset.UtcNow, () => executed.Add("succeeds")));

        var results = new TaskExecutor().Execute(queue);

        Assert.False(results[0].Succeeded);
        Assert.Equal("broken", results[0].ErrorMessage);
        Assert.True(results[1].Succeeded);
        Assert.Equal(new[] { "succeeds" }, executed);
    }
}
