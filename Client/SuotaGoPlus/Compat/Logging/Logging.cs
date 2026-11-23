using System.Diagnostics;

namespace Prism.Logging
{
    /// <summary>
    /// Category/Priority/ILoggerFacade reproduce the API of the Prism.Logging
    /// package, which was removed in Prism 8. The ported code logs through this.
    /// </summary>
    public enum Category
    {
        Debug,
        Info,
        Warn,
        Exception
    }

    public enum Priority
    {
        None,
        Low,
        Medium,
        High
    }

    public interface ILoggerFacade
    {
        void Log(string message, Category category, Priority priority);
    }

    public class DebugLogger : ILoggerFacade
    {
        public void Log(string message, Category category, Priority priority)
        {
            Debug.WriteLine($"[{category}] ({priority}) {message}");
        }
    }
}
