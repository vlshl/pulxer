namespace Common.Interfaces
{
    public interface IConsole
    {
        void Write(string text);
        void WriteLine(string text);
        void WriteSeparator();
        void WriteTitle(string title);
        void WriteError(string text);
        string ReadLine();
    }
}
