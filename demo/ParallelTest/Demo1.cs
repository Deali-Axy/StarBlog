using Dumpify;

namespace ParallelTest;

public class Demo1 {
    public void Run() {
        var root = new KdNode {
            Value = 1,
            Left = new KdNode {
                Value = 2,
                Left = new KdNode()
            }
        };

        Console.WriteLine("初始状态");
        root.Dump();

        Console.WriteLine("执行了 Parallel 任务之后的状态");
        Parallel.Invoke(() => BuildTree(root.Left));
        root.Dump();
    }

    void BuildTree(KdNode node) {
        node.Value += 1;
        if (node.Left != null) {
            node.Left.Value += 1;
            node.Left.Left = new KdNode { Value = 100 };
        }
    }
}

class KdNode {
    public int Value { get; set; }
    public KdNode? Left { get; set; }
}