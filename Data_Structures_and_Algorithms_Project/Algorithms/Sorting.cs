namespace Algorithms;

public static class QuadraticSorter
{
    public static void Sort(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        for (var i = 1; i < values.Length; i++)
        {
            var current = values[i];
            var position = i - 1;
            while (position >= 0 && values[position] > current)
            {
                values[position + 1] = values[position];
                position--;
            }

            values[position + 1] = current;
        }
    }
}

public static class OptimizedSorter
{
    public static void Sort(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        Array.Sort(values);
    }
}
