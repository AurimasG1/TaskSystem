namespace TaskSystem.AdminCli.Config;

public class CliConfiguration
{
    public string ConnectionString { get; set; }

    public static CliConfiguration Load()
    {
        var conn = Environment.GetEnvironmentVariable("TASKSYSTEM_DB");

        if (string.IsNullOrWhiteSpace(conn))
        {
            Console.WriteLine("ERROR: TASKSYSTEM_DB is not set.");
            Environment.Exit(1);
        }

        return new CliConfiguration { ConnectionString = conn };
    }
}
