namespace AgentTarget;

public static class Program
{
    public static void Main(string[] args)
    {
        var name = args.Length > 0 ? args[0] : Ask("Project name");
        var description = args.Length > 1 ? args[1] : Ask("Description of what you are building");
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Project name is required.");
            return;
        }

        var report = TargetBuilder.Build(name.Trim(), description.Trim(), Directory.GetCurrentDirectory());
        Console.WriteLine(report);
    }

    private static string Ask(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine() ?? "";
    }
}
