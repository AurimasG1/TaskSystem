using TaskSystem.AdminCli.Services;

namespace TaskSystem.AdminCli.Commands;

public class PromoteAdminCommand
{
    private readonly AdminPromotionService _service;

    public PromoteAdminCommand(AdminPromotionService service)
    {
        _service = service;
    }

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || args[0] != "admin" || args[1] != "promote")
        {
            PrintUsage();
            return;
        }

        var emailArg = args.FirstOrDefault(a => a.StartsWith("--email="));
        var idArg = args.FirstOrDefault(a => a.StartsWith("--userId="));

        if (emailArg != null)
        {
            var email = emailArg.Split("=")[1];
            await _service.PromoteByEmailAsync(email);
            return;
        }

        if (idArg != null)
        {
            var id = int.Parse(idArg.Split("=")[1]);
            await _service.PromoteByIdAsync(id);
            return;
        }

        PrintUsage();
    }

    private void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  admin promote --email=<email>");
        Console.WriteLine("  admin promote --userId=<id>");
    }
}
