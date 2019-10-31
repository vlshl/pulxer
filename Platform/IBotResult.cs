namespace Platform
{
    public interface IBotResult
    {
        bool IsSuccess { get; }
        string Message { get; }
    }
}
