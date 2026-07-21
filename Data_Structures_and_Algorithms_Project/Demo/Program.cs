using System.Diagnostics;
using Algorithms;

var tree = new AvlTree<int>();
for (var value = 1000; value >= 1; value--)
    tree.Add(value);

Console.WriteLine($"AVL: {tree.Count} elementos, altura {tree.Height}, balanceado: {tree.IsBalanced}");

var queue = new TaskSchedulerQueue();
var now = DateTimeOffset.UtcNow;
queue.Enqueue(new ScheduledTask("generar-informe", 1, now.AddMinutes(2), () => { }));
queue.Enqueue(new ScheduledTask("validar-datos", 3, now, () => { }));
queue.Enqueue(new ScheduledTask("publicar-resultados", 2, now.AddMinutes(1), () => throw new InvalidOperationException("Servicio no disponible")));

var logs = new List<string>();
var results = new TaskExecutor(logs.Add).Execute(queue);
Console.WriteLine($"Ejecucion: {results.Count(r => r.Succeeded)} correctas, {results.Count(r => !r.Succeeded)} con error");
foreach (var log in logs)
    Console.WriteLine($"Registro: {log}");

const int size = 20_000;
var random = new Random(2026);
var input = Enumerable.Range(0, size).Select(_ => random.Next()).ToArray();

// Una sola medicion reproducible para mostrar la diferencia de orden de crecimiento.
var quadraticInput = (int[])input.Clone();
var optimizedInput = (int[])input.Clone();
var quadraticTime = Measure(() => QuadraticSorter.Sort(quadraticInput));
var optimizedTime = Measure(() => OptimizedSorter.Sort(optimizedInput));
Console.WriteLine($"Benchmark ({size:N0} elementos): insercion O(n2) {quadraticTime:F2} ms; Array.Sort O(n log n) {optimizedTime:F2} ms");

static double Measure(Action operation)
{
    var stopwatch = Stopwatch.StartNew();
    operation();
    stopwatch.Stop();
    return stopwatch.Elapsed.TotalMilliseconds;
}
