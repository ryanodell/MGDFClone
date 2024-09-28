using MGDFClone.Features;
using Serilog;

namespace MGDFClone; 
internal static class Program {
    [STAThread]
    static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/game-log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        PerlinNoiseV2.Run();

        using MainGame game = new();
        Log.Information("Game Initialized");
        game.Run();
        Log.Information("Game Shutdown");
        Log.CloseAndFlush();
    }
}
