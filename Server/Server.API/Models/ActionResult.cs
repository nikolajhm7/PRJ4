namespace Server.API.Models
{
    public record ActionResult(bool Success, string? Msg);
    public record ActionResult<T>(bool Success, string? Msg, T? Value);
}
