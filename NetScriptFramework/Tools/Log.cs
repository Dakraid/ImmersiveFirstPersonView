using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScriptFramework.Tools
{
#region LogFile class

    /// <summary>
    /// Implement helper class to write log file using default settings. Log file methods are thread-safe.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class LogFile : IDisposable
    {
    #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        /// <param name="keyword">The keyword of log file. This should be the same as your plugin's DLL name without the extension.</param>
        /// <param name="flags">The flags for log file.</param>
        /// <exception cref="System.ArgumentNullException">keyword</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">keyword</exception>
        public LogFile(string keyword, LogFileFlags flags)
        {
            if (keyword == null)
                throw new ArgumentNullException("keyword");
            keyword = keyword.Trim().ToLower();
            if (keyword.Length == 0)
                throw new ArgumentOutOfRangeException("keyword");

            Keyword = keyword;
            if (Main.Config != null)
            {
                var value = Main.Config.GetValue(Main._Config_Plugin_Path);
                if (value != null)
                    Path = value.ToString();
            }

            Flags = flags;

            if ((Flags & LogFileFlags.DelayedOpen) == LogFileFlags.None)
                OpenFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFile"/> class.
        /// </summary>
        internal LogFile()
        {
            Keyword = Main.FrameworkName;
            Path    = Main.FrameworkPath;
            Flags   = LogFileFlags.AutoFlush | LogFileFlags.IncludeTimestampInLine;
        }

        /// <summary>
        /// The keyword of log file.
        /// </summary>
        public readonly string Keyword;

        /// <summary>
        /// The options for this log file.
        /// </summary>
        public readonly LogFileFlags Flags;

        /// <summary>
        /// Get or set prefix of file manually.
        /// </summary>
        private string Prefix { get; set; } = "";

        /// <summary>
        /// Get or set suffix of file manually.
        /// </summary>
        private string Suffix { get; set; } = "log";

        /// <summary>
        /// Get or set path of file manually.
        /// </summary>
        private string Path { get; set; } = "";

        /// <summary>
        /// Closes this instance and the underlying file if it is open.
        /// </summary>
        public void Close() { CloseFile(); }

        /// <summary>
        /// Appends the specified text to log file without writing a newline at the end. This will re-open the file if it is not opened!
        /// </summary>
        /// <param name="text">The text to append.</param>
        /// <exception cref="System.ArgumentNullException">text</exception>
        public void Append(string text)
        {
            // Must have text!
            if (text == null)
                throw new ArgumentNullException("text");

            lock (Locker)
            {
                // Append timestamp if newline.
                if (isNewLine && (Flags & LogFileFlags.IncludeTimestampInLine) != LogFileFlags.None)
                    text = "[" + DateTime.Now.ToLogTimestampString(true) + "] " + text;

                // Write to file.
                OpenFile();
                file.Write(text);
                if ((Flags & LogFileFlags.AutoFlush) != LogFileFlags.None)
                    file.Flush();

                // Set newline status.
                isNewLine = text.EndsWith("\n") || text.EndsWith("\r");
            }
        }

        /// <summary>
        /// Appends the specified exception information to log.
        /// </summary>
        /// <param name="e">The exception.</param>
        public void Append(Exception e)
        {
            var lines = GetExceptionText(e);
            foreach (var x in lines)
                AppendLine(x);
        }

        /// <summary>
        /// Gets the exception text lines.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="wrap">Wrap exception to this length.</param>
        /// <param name="tab">The tab to use when multiple exceptions are involved.</param>
        /// <returns></returns>
        internal static List<string> GetExceptionText(Exception e, int wrap = 140, string tab = "  ")
        {
            if (e == null)
                return new List<string>() {"null"};

            if (wrap < 40)
                wrap = 40;

            var full = new List<Exception>();
            while (e != null)
            {
                full.Add(e);
                e = e.InnerException;
            }

            full.Reverse();

            var result = new List<string>();
            for (var i = 0; i < full.Count; i++)
            {
                var thisTab = string.Empty;
                if (i > 0 && !string.IsNullOrEmpty(tab))
                {
                    var tabStr = new StringBuilder();
                    for (var j = 0; j < i; j++)
                        tabStr.Append(tab);
                    thisTab = tabStr.ToString();
                }

                e = full[i];

                var curWrap = wrap - thisTab.Length;
                if (curWrap < 0)
                    curWrap = 0;

                {
                    var line = "(" + e.GetType().Name + "): \"" + (e.Message ?? string.Empty) + "\" at";
                    var ind  = ("(" + e.GetType().Name + "): ").Length;
                    if (curWrap - ind > 10)
                    {
                        var ls = ConfigEntry.Wrap(line, curWrap, ind);
                        foreach (var x in ls)
                            result.Add(thisTab + x);
                    }
                    else { result.Add(thisTab + line); }
                }

                if (curWrap > 0)
                    result.Add(new string('-', curWrap));
                var trace = (e.StackTrace ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n").Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var x in trace)
                    result.Add(thisTab + x);
                if (curWrap > 0)
                    result.Add(new string('-', curWrap));
            }

            return result;
        }

        /// <summary>
        /// Appends the specified text to log file and writes a newline at the end. This will re-open the file if it is not opened!
        /// </summary>
        /// <param name="text">The text to append.</param>
        public void AppendLine(string text)
        {
            // Must have text!
            if (text == null)
                throw new ArgumentNullException("text");

            lock (Locker)
            {
                // Append timestamp if newline.
                if (isNewLine && (Flags & LogFileFlags.IncludeTimestampInLine) != LogFileFlags.None)
                    text = "[" + DateTime.Now.ToLogTimestampString(true) + "] " + text;

                // Write to file.
                OpenFile();
                file.WriteLine(text);
                if ((Flags & LogFileFlags.AutoFlush) != LogFileFlags.None)
                    file.Flush();

                // Set newline status.
                isNewLine = true;
            }
        }

    #endregion

    #region Internal members

        /// <summary>
        /// The locker for log file.
        /// </summary>
        private readonly object Locker = new object();

        /// <summary>
        /// Generate file path with current settings.
        /// </summary>
        /// <returns></returns>
        private string GenerateFilePath()
        {
            var strFile = new StringBuilder();
            if (!string.IsNullOrEmpty(Prefix))
                strFile.Append(Prefix + ".");
            strFile.Append(Keyword);
            if (!string.IsNullOrEmpty(Suffix))
                strFile.Append("." + Suffix);
            if ((Flags & LogFileFlags.IncludeTimestampInFileName) != LogFileFlags.None)
            {
                var t = DateTime.Now;
                strFile.Append("." + t.Year + "-" + t.Month.ToString("00") + "-" + t.Day.ToString("00") + "-" + t.Hour.ToString("00") + "-" + t.Minute.ToString("00") + "-" +
                               t.Second.ToString("00"));
            }

            strFile.Append(".txt");

            var fullPath = strFile.ToString();
            if (!string.IsNullOrEmpty(Path))
                fullPath = System.IO.Path.Combine(Path, fullPath);

            return fullPath;
        }

        /// <summary>
        /// The opened file.
        /// </summary>
        private StreamWriter file = null;

        /// <summary>
        /// Is log currently at a new line?
        /// </summary>
        private bool isNewLine = true;

        /// <summary>
        /// Opens the file for writing.
        /// </summary>
        private void OpenFile()
        {
            lock (Locker)
            {
                if (file != null)
                    return;

                file = new StreamWriter(GenerateFilePath(), (Flags & LogFileFlags.AppendFile) != LogFileFlags.None);
                lock (AllLocker) { All.AddLast(this); }
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        private void CloseFile()
        {
            lock (Locker)
            {
                if (file == null)
                    return;
                file.Close();
                file = null;
                lock (AllLocker) { All.Remove(this); }
            }
        }

        /// <summary>
        /// Closes all log files.
        /// </summary>
        internal static void CloseAll()
        {
            lock (AllLocker)
            {
                while (All.Count != 0)
                    All.First.Value.CloseFile();
            }
        }

        /// <summary>
        /// All opened log files, we use this to make sure all log files are closed at the end.
        /// </summary>
        private static readonly LinkedList<LogFile> All = new LinkedList<LogFile>();

        /// <summary>
        /// Locker for all log files list.
        /// </summary>
        private static readonly object AllLocker = new object();

    #endregion

    #region IDisposable interface

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { Close(); }

    #endregion
    }

#endregion

#region DateTimeStringConverter class

    /// <summary>
    /// Helper function to convert date time to short string.
    /// </summary>
    public static class DateTimeStringConverter
    {
        /// <summary>
        /// Convert this date time instance to a short string for log file (or other use).
        /// </summary>
        /// <param name="t">The time.</param>
        /// <param name="milliseconds">if set to <c>true</c> then display milliseconds in the string.</param>
        /// <returns></returns>
        public static string ToLogTimestampString(this DateTime t, bool milliseconds = true)
        {
            if (milliseconds)
                return string.Format("{0:00} {1} {2} {3:00}:{4:00}:{5:00}.{6:000}", t.Day, Months[t.Month], t.Year, t.Hour, t.Minute, t.Second, t.Millisecond);
            return string.Format("{0:00} {1} {2} {3:00}:{4:00}:{5:00}", t.Day, Months[t.Month], t.Year, t.Hour, t.Minute, t.Second);
        }

        /// <summary>
        /// The month names for timestamp string.
        /// </summary>
        private static string[] Months = new[] {"Nul", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
    }

#endregion

#region LogFile enums

    /// <summary>
    /// List of options for log file.
    /// </summary>
    [Flags]
    public enum LogFileFlags : uint
    {
        /// <summary>
        /// No options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Include timestamp (at the time of opening the file) in the file name.
        /// </summary>
        IncludeTimestampInFileName = 1,

        /// <summary>
        /// Include timestamp before each line written into the file.
        /// </summary>
        IncludeTimestampInLine = 2,

        /// <summary>
        /// Append log file instead of overwriting it.
        /// </summary>
        AppendFile = 4,

        /// <summary>
        /// Automatically flush to file after each write. This can be slower but ensures that all data is written immediately, when
        /// application unexpectedly closes the unwritten lines in file buffer is not lost.
        /// </summary>
        AutoFlush = 8,

        /// <summary>
        /// Don't open the log file until first write is requested.
        /// </summary>
        DelayedOpen = 0x10
    }

#endregion
}