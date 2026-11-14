namespace SlipStream.Identity.Options;

public sealed class FrontendOptions
{
    public const string Section = "Frontend";
    public string Url { get; set; } = "http://localhost:5173";
}
