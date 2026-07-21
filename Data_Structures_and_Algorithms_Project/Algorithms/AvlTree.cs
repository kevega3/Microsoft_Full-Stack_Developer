namespace Algorithms;

public sealed class AvlTree<T> where T : IComparable<T>
{
    private Node? _root;

    public int Count { get; private set; }

    public int Height => HeightOf(_root);

    public bool IsBalanced => IsBalancedAt(_root);

    public void Add(T value)
    {
        var added = false;
        _root = Insert(_root, value, ref added);
        if (added)
            Count++;
    }

    public bool Contains(T value)
    {
        var current = _root;
        while (current is not null)
        {
            var comparison = value.CompareTo(current.Value);
            if (comparison == 0)
                return true;

            current = comparison < 0 ? current.Left : current.Right;
        }

        return false;
    }

    public IReadOnlyList<T> InOrder()
    {
        var values = new List<T>(Count);
        VisitInOrder(_root, values);
        return values;
    }

    private static Node Insert(Node? node, T value, ref bool added)
    {
        if (node is null)
        {
            added = true;
            return new Node(value);
        }

        var comparison = value.CompareTo(node.Value);
        if (comparison < 0)
            node.Left = Insert(node.Left, value, ref added);
        else if (comparison > 0)
            node.Right = Insert(node.Right, value, ref added);
        else
            return node;

        UpdateHeight(node);
        return Balance(node);
    }

    private static Node Balance(Node node)
    {
        var balance = BalanceFactor(node);
        if (balance > 1)
        {
            if (BalanceFactor(node.Left!) < 0)
                node.Left = RotateLeft(node.Left!);
            return RotateRight(node);
        }

        if (balance < -1)
        {
            if (BalanceFactor(node.Right!) > 0)
                node.Right = RotateRight(node.Right!);
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

    private static int BalanceFactor(Node node) => HeightOf(node.Left) - HeightOf(node.Right);

    private static int HeightOf(Node? node) => node?.Height ?? 0;

    private static void UpdateHeight(Node node) => node.Height = Math.Max(HeightOf(node.Left), HeightOf(node.Right)) + 1;

    private static void VisitInOrder(Node? node, ICollection<T> values)
    {
        if (node is null)
            return;

        VisitInOrder(node.Left, values);
        values.Add(node.Value);
        VisitInOrder(node.Right, values);
    }

    private static bool IsBalancedAt(Node? node)
    {
        if (node is null)
            return true;

        return Math.Abs(BalanceFactor(node)) <= 1 && IsBalancedAt(node.Left) && IsBalancedAt(node.Right);
    }

    private sealed class Node(T value)
    {
        public T Value { get; } = value;
        public int Height { get; set; } = 1;
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }
}
