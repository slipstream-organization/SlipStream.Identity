namespace SlipStream.Identity.Common;

public static class ReturnUrl
{
    public static bool IsValid(string? url) =>
        !string.IsNullOrEmpty(url)
        && url.StartsWith('/')
        && !url.StartsWith("//")
        && !url.StartsWith("/\\")
        && !url.Contains("://");
}
