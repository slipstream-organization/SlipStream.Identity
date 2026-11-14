namespace SlipStream.Identity.Domain.Users;

public static class RacingReference
{
    public static readonly IReadOnlySet<string> Teams = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "alpine", "aston_martin", "audi", "cadillac", "ferrari", "haas",
        "mclaren", "mercedes", "racing_bulls", "red_bull", "williams"
    };

    public static readonly IReadOnlySet<string> Drivers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "VER", "HAD", "NOR", "PIA", "LEC", "HAM", "RUS", "ANT", "ALO", "STR", "GAS",
        "COL", "OCO", "BEA", "LAW", "LIN", "ALB", "SAI", "HUL", "BOR", "PER", "BOT"
    };

    public static bool IsKnownTeam(string teamId) => Teams.Contains(teamId);
    public static bool IsKnownDriver(string driverCode) => Drivers.Contains(driverCode);
}
