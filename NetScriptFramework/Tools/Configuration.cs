using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScriptFramework.Tools
{
#region ConfigEntry class

    /// <summary>
    /// One entry in configuration settings.
    /// </summary>
    public sealed class ConfigEntry
    {
    #region Constructors

        /// <summary>
        /// Create a new entry.
        /// </summary>
        internal ConfigEntry(string keyword, Value defaultValue, string name = null, string description = null, ConfigEntryFlags flags = ConfigEntryFlags.None)
        {
            // Missing keyword.
            if (keyword == null)
                throw new ArgumentNullException("keyword");
            keyword = keyword.Trim();
            if (keyword.Length == 0)
                throw new ArgumentOutOfRangeException("keyword");

            // Default value is missing.
            if (defaultValue == null)
                throw new ArgumentNullException("defaultValue");

            // Set keyword.
            Keyword = keyword;

            // Set flags.
            Flags = flags;

            // Set as default and current value.
            DefaultValue = defaultValue;
            CurrentValue = defaultValue;

            // Set as name and description.
            Name        = name;
            Description = description;
        }

    #endregion

    #region ConfigEntry members

        /// <summary>
        /// Keyword of entry.
        /// </summary>
        public readonly string Keyword;

        /// <summary>
        /// Name of entry.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Description of entry.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Options for configuration entry.
        /// </summary>
        public readonly ConfigEntryFlags Flags;

        /// <summary>
        /// Get default value of setting.
        /// </summary>
        public Value DefaultValue
        {
            get => _defaultValue;
            internal set => _defaultValue = value;
        }

        /// <summary>
        /// Get current value of setting.
        /// </summary>
        public Value CurrentValue
        {
            get => _currentValue;
            internal set => _currentValue = value;
        }

        /// <summary>
        /// Add a custom valid range of values. If value is not in valid value list and not in valid range list then it is not
        /// allowed to set this value. This method is not valid for strings and will throw an exception. If no valid values
        /// are not added then this check is ignored.
        /// </summary>
        /// <param name="min">Minimum allowed value.</param>
        /// <param name="max">Maximum allowed value.</param>
        public void AddValidRange(Value min, Value max)
        {
            // Invalid.
            if (min == null && max == null)
                return;

            // Add to list.
            _validRange.Add(new KeyValuePair<Value, Value>(min, max));
        }

        /// <summary>
        /// Add a custom valid value. If value is not in valid value list and not in valid range list then it is not
        /// allowed to set this value. If no valid values are not added then this check is ignored.
        /// </summary>
        /// <param name="value">Value to add.</param>
        public void AddValidValue(Value value)
        {
            // Invalid.
            if (value == null)
                return;

            // Add to list.
            _validValue.Add(value);
        }

    #endregion

    #region Internal members

        /// <summary>
        /// Try to set value of this entry.
        /// </summary>
        /// <param name="text">Value in string form.</param>
        /// <returns></returns>
        internal bool TrySetValue(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            Value newValue = null;
            if (!Value.TryParse(text, DefaultValue.GetTypeCode(), out newValue))
                return false;

            if (CurrentValue.Equals(newValue))
                return true;

            CurrentValue = newValue;
            IsModified   = true;
            return true;
        }

        /// <summary>
        /// Try to get value of this entry in string form.
        /// </summary>
        /// <returns></returns>
        internal string TryGetValue()
        {
            var text = CurrentValue.ToString();
            if (DefaultValue.GetTypeCode() == TypeCode.String || DefaultValue.GetTypeCode() == TypeCode.Char)
                return "\"" + text + "\"";

            if ((Flags & ConfigEntryFlags.PreferHex) != ConfigEntryFlags.None)
            {
                var ok = false;
                switch (DefaultValue.GetTypeCode())
                {
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        ok = true;
                        break;
                }

                if (ok)
                    switch (CurrentValue.GetTypeCode())
                    {
                        case TypeCode.Byte:
                        {
                            byte v = 0;
                            if (byte.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.SByte:
                        {
                            sbyte v = 0;
                            if (sbyte.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.Int16:
                        {
                            short v = 0;
                            if (short.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.UInt16:
                        {
                            ushort v = 0;
                            if (ushort.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.Int32:
                        {
                            var v = 0;
                            if (int.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.UInt32:
                        {
                            uint v = 0;
                            if (uint.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.Int64:
                        {
                            long v = 0;
                            if (long.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;

                        case TypeCode.UInt64:
                        {
                            ulong v = 0;
                            if (ulong.TryParse(text, out v))
                                return "0x" + v.ToString("X");
                        }
                            break;
                    }
            }

            return text;
        }

        /// <summary>
        /// Generate full comment block.
        /// </summary>
        /// <returns></returns>
        internal List<string> GenerateCommentBlock()
        {
            if ((Flags & ConfigEntryFlags.VeryShortComment) != ConfigEntryFlags.None)
            {
                var cls = new List<string>();
                if (!string.IsNullOrEmpty(Description))
                {
                    var spl = Wrap(Description, GetLineLength());
                    if (spl != null && spl.Length != 0)
                        cls.AddRange(spl);
                    while (cls.Count != 0 && string.IsNullOrEmpty(cls[0]))
                        cls.RemoveAt(0);
                    while (cls.Count != 0 && string.IsNullOrEmpty(cls[cls.Count - 1]))
                        cls.RemoveAt(cls.Count - 1);

                    for (var i = 0; i < cls.Count; i++)
                        cls[i] = "# " + cls[i];
                }

                return cls;
            }

            var started = false;
            var lines   = new List<string>();

            if (!string.IsNullOrEmpty(Name))
            {
                if (!started)
                {
                    lines.Add(ConfigLines.CommentBlockTop);
                    started = true;

                    for (var i = 0; i < ConfigLines.CommentBlockTopMargin; i++)
                        WriteCommentBlock("", lines);
                }

                WriteCommentBlock("  " + Name, lines);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                if (!started)
                {
                    lines.Add(ConfigLines.CommentBlockTop);
                    started = true;

                    for (var i = 0; i < ConfigLines.CommentBlockTopMargin; i++)
                        WriteCommentBlock("", lines);
                }
                else { WriteCommentBlock("", lines); }

                WriteCommentBlock(Description, lines);
            }

            if (started)
            {
                WriteCommentBlock("", lines);

                var tn = DefaultValue.GetTypeCode().ToString().ToLower();
                if (tn == "single")
                    tn = "float";
                WriteCommentBlock("   Type: (" + tn + ")", lines);
                var valid = new List<string[]>();
                foreach (var x in _validRange)
                {
                    var vl = new string[2];
                    if (x.Key == null)
                        vl[0] = "...";
                    else
                        vl[0] = x.Key.ToString();

                    if (x.Value == null)
                        vl[1] = "...";
                    else
                        vl[1] = x.Value.ToString();

                    valid.Add(vl);
                }

                foreach (var x in _validValue)
                {
                    var vl = new string[1];
                    vl[0] = x.ToString();

                    valid.Add(vl);
                }

                var strVal = new StringBuilder();
                foreach (var x in valid)
                    if (x.Length == 1)
                    {
                        if (strVal.Length != 0)
                            strVal.Append("; ");
                        strVal.Append(x[0]);
                    }
                    else if (x.Length == 2)
                    {
                        if (strVal.Length != 0)
                            strVal.Append("; ");
                        strVal.Append(x[0]);
                        if (x[0] != "..." && x[1] != "...")
                            strVal.Append("...");
                        strVal.Append(x[1]);
                    }

                if (strVal.Length != 0)
                    WriteCommentBlock("   Range: " + strVal.ToString(), lines);

                for (var i = 0; i < ConfigLines.CommentBlockBottomMargin; i++)
                    WriteCommentBlock("", lines);

                lines.Add(ConfigLines.CommentBlockBottom);
            }

            return lines;
        }

        /// <summary>
        /// Generate setting line.
        /// </summary>
        /// <returns></returns>
        internal string GenerateSetting() { return Keyword + " = " + TryGetValue(); }

        /// <summary>
        /// Get or set whether entry has been modified since last save.
        /// </summary>
        internal bool IsModified
        {
            get => _modified;
            set => _modified = value;
        }

        /// <summary>
        /// Write this entry to file.
        /// </summary>
        /// <param name="writer">File stream.</param>
        internal void WriteToFile(StreamWriter writer)
        {
            if (!ShouldWriteToFile())
                return;

            var lines = GenerateCommentBlock();
            if (lines.Count != 0 && (Flags & ConfigEntryFlags.VeryShortComment) == ConfigEntryFlags.None)
                lines.Add("");
            writer.Flush();
            if (writer.BaseStream.Position != 0)
                if (lines.Count != 0 || (Flags & ConfigEntryFlags.NoNewLineBefore) == ConfigEntryFlags.None)
                    lines.Insert(0, "");
            lines.Add(GenerateSetting());

            foreach (var x in lines)
                writer.WriteLine(x);
        }

        /// <summary>
        /// Check if entry should be written to file.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldWriteToFile()
        {
            // Don't write this to file.
            if ((Flags & ConfigEntryFlags.NoSave) != ConfigEntryFlags.None)
                return false;

            // No need to write to file.
            if ((Flags & ConfigEntryFlags.Hidden) != ConfigEntryFlags.None && CurrentValue.Equals(DefaultValue))
                return false;

            // Should write.
            return true;
        }

        /// <summary>
        /// Try to validate value.
        /// </summary>
        /// <param name="value">Value to validate.</param>
        private void Validate(Value value)
        {
            // Invalid.
            if (value == null)
                throw new ArgumentNullException("value");

            // Check range.
            {
                var isValid = false;
                foreach (var o in _validValue)
                    if (o.Equals(value))
                    {
                        isValid = true;
                        break;
                    }

                if (!isValid)
                {
                    foreach (var x in _validRange)
                    {
                        if (x.Key != null && value.CompareTo(x.Key) < 0)
                            continue;
                        if (x.Value != null && value.CompareTo(x.Value) > 0)
                            continue;

                        isValid = true;
                        break;
                    }

                    if (!isValid && (_validValue.Count != 0 || _validRange.Count != 0))
                        throw new ArgumentOutOfRangeException("value", "Value is out of range!");
                }
            }

            // Seems ok.
        }

        /// <summary>
        /// Wrap text into multiple lines if it's longer than specified.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <param name="length">Maximum length of the new lines.</param>
        /// <param name="indent">Indent length. This will add this many spaces to any line after the first.</param>
        /// <returns></returns>
        internal static string[] Wrap(string text, int length, int indent = 0)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (text.Length <= length)
                return text.Split(new[] {"\r\n", "\n", "\r"}, StringSplitOptions.None);

            indent = Math.Max(0, Math.Min(indent, length - 1));

            var lines     = new StringBuilder();
            var lastOk    = -1;
            var lastWrite = 0;
            var len       = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (len == length)
                {
                    if (text[i] == ' ' || text[i] == '\t' || text[i] == '\n' || text[i] == '\r')
                        lastOk = i;

                    if (lastOk == -1)
                    {
                        if (lines.Length > 0)
                        {
                            lines.Append("\n");
                            if (indent > 0)
                                lines.Append(' ', indent);
                        }

                        lines.Append(text.Substring(lastWrite, i - lastWrite));
                        lastWrite = i;
                        len       = indent;
                    }
                    else
                    {
                        if (lines.Length > 0)
                        {
                            lines.Append("\n");
                            if (indent > 0)
                                lines.Append(' ', indent);
                        }

                        lines.Append(text.Substring(lastWrite, lastOk - lastWrite));
                        lastWrite = lastOk;
                        while (lastWrite < text.Length && (text[lastWrite] == ' ' || text[lastWrite] == '\t'))
                            lastWrite++;
                        lastOk = -1;
                        len    = indent;
                        i      = lastWrite - 1;
                        continue;
                    }
                }

                switch (text[i])
                {
                    case '\n':
                    case '\r':
                        if (lines.Length > 0)
                        {
                            lines.Append("\n");
                            if (indent > 0)
                                lines.Append(' ', indent);
                        }

                        lines.Append(text.Substring(lastWrite, i - lastWrite));
                        lastWrite = i + 1;
                        len       = indent;
                        lastOk    = -1;
                        continue;
                }

                len++;
                if (_wrapChars.Contains(text[i]))
                    lastOk = i;
            }

            if (lastWrite < text.Length)
            {
                if (len > indent && lines.Length > 0)
                {
                    lines.Append("\n");
                    if (indent > 0)
                        lines.Append(' ', indent);
                }

                lines.Append(text.Substring(lastWrite, text.Length - lastWrite));
            }

            return lines.ToString().Split(new[] {'\n'}, StringSplitOptions.None);
        }

        /// <summary>
        /// Wrap characters.
        /// </summary>
        private static char[] _wrapChars = new char[] {' ', ',', '.', '?', '!', ':', ';', ')', ']', '}', '-', '>', '\t'};

        /// <summary>
        /// Write a comment block line.
        /// </summary>
        /// <param name="text">Text to write.</param>
        /// <param name="lines">Lines.</param>
        private static void WriteCommentBlock(string text, List<string> lines)
        {
            var wrp = Wrap(text, GetLineLength());
            if (wrp.Length == 0)
                wrp = new string[] {""};

            foreach (var x in wrp)
                /*if (x.IndexOfAny(new[] { '\r', '\n' }) >= 0)
                    {
                        var spl = x.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                        foreach(var y in spl)
                        {
                            StringBuilder str = new StringBuilder();
                            str.Append(ConfigLines.CommentBlockLeft);
                            str.Append(' ', ConfigLines.CommentBlockLeftMargin);
                            str.Append(y);
                            str.Append(' ', GetLineLength() - y.Length);
                            str.Append(' ', ConfigLines.CommentBlockRightMargin);
                            str.Append(ConfigLines.CommentBlockRight);
                            lines.Add(str.ToString());
                        }
                    }
                    else*/
            {
                var str = new StringBuilder();
                str.Append(ConfigLines.CommentBlockLeft);
                str.Append(' ', ConfigLines.CommentBlockLeftMargin);
                str.Append(x);
                str.Append(' ', GetLineLength() - x.Length);
                str.Append(' ', ConfigLines.CommentBlockRightMargin);
                str.Append(ConfigLines.CommentBlockRight);
                lines.Add(str.ToString());
            }
        }

        /// <summary>
        /// Get comment block max line length.
        /// </summary>
        /// <returns></returns>
        private static int GetLineLength()
        {
            return 120 - (ConfigLines.CommentBlockLeft.Length + ConfigLines.CommentBlockLeftMargin + ConfigLines.CommentBlockRight.Length +
                          ConfigLines.CommentBlockRightMargin);
        }

        /// <summary>
        /// Convert entry to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() { return GenerateSetting(); }

        private          bool                             _modified     = false;
        private          Value                            _defaultValue = null;
        private          Value                            _currentValue = null;
        private readonly List<KeyValuePair<Value, Value>> _validRange   = new List<KeyValuePair<Value, Value>>();
        private readonly List<Value>                      _validValue   = new List<Value>();

    #endregion
    }

#endregion

#region ConfigEntry enums

    /// <summary>
    /// Options for one configuration entry.
    /// </summary>
    [Flags]
    public enum ConfigEntryFlags : ulong
    {
        None = 0,

        /// <summary>
        /// This is a custom setting from user, not registered.
        /// </summary>
        Custom = 1,

        /// <summary>
        /// Don't write to file if setting is equal to default value.
        /// </summary>
        Hidden = 2,

        /// <summary>
        /// Don't write to file ever. This means setting is reset every time.
        /// </summary>
        NoSave = 4,

        /// <summary>
        /// Prefer to write out this value in hexadecimal notation when possible.
        /// </summary>
        PreferHex = 8,

        /// <summary>
        /// Skip writing a new line before this entry if the comments are empty.
        /// </summary>
        NoNewLineBefore = 0x10,

        /// <summary>
        /// Make the comment block very small.
        /// </summary>
        VeryShortComment = 0x20
    }

#endregion

#region ConfigFile class

    /// <summary>
    /// Handles reading and writing configuration settings from file.
    /// </summary>
    public sealed class ConfigFile
    {
    #region Constructors

        /// <summary>
        /// Create a new configuration file.
        /// </summary>
        /// <param name="keyword">Keyword of configuration. This should usually be the name of your plugin's DLL without the extension.</param>
        public ConfigFile(string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException("keyword");
            keyword = keyword.Trim();
            if (keyword.Length == 0)
                throw new ArgumentOutOfRangeException("keyword");

            Keyword = keyword;
            if (Main.Config != null)
            {
                var value = Main.Config.GetValue(Main._Config_Plugin_Path);
                if (value != null)
                    Path = value.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigFile"/> class.
        /// </summary>
        internal ConfigFile()
        {
            Keyword = Main.FrameworkName;
            Path    = Main.FrameworkPath;
        }

    #endregion

    #region ConfigFile members

        /// <summary>
        /// Keyword of file.
        /// </summary>
        public readonly string Keyword;

        /// <summary>
        /// Get or set prefix of file manually.
        /// </summary>
        internal string Prefix { get; set; } = "";

        /// <summary>
        /// Get or set suffix of file manually.
        /// </summary>
        internal string Suffix { get; set; } = "config";

        /// <summary>
        /// Get or set path of file manually.
        /// </summary>
        internal string Path { get; set; } = "";

        /// <summary>
        /// Get or set whether configuration file should be saved automatically when it's modified during runtime.
        /// Default is true.
        /// </summary>
        public bool AutoSave
        {
            get => (Flags & ConfigFileFlags.ManualSave) == ConfigFileFlags.None;
            set
            {
                if (value)
                    Flags &= ~ConfigFileFlags.ManualSave;
                else
                    Flags |= ConfigFileFlags.ManualSave;
            }
        }

        /// <summary>
        /// Get or set whether configuration file should be created automatically if it doesn't exist.
        /// Default is true.
        /// </summary>
        public bool AutoCreate
        {
            get => (Flags & ConfigFileFlags.ManualCreate) == ConfigFileFlags.None;
            set
            {
                if (value)
                    Flags &= ~ConfigFileFlags.ManualCreate;
                else
                    Flags |= ConfigFileFlags.ManualCreate;
            }
        }

        /// <summary>
        /// Loads configuration from file into the specified object. Returns true if file was loaded and false if file was not loaded (but may have been created now if the argument for it was set).
        /// </summary>
        /// <typeparam name="T">Type of object to load into.</typeparam>
        /// <param name="obj">The object to load into.</param>
        /// <param name="keyword">The keyword of the configuration file.</param>
        /// <param name="createIfMissing">if set to <c>true</c> then create configuration file if missing or failed to load.</param>
        /// <exception cref="System.ArgumentNullException">obj</exception>
        public static bool LoadFrom<T>(T obj, string keyword, bool createIfMissing) where T : class
        {
            if (ReferenceEquals(obj, null))
                throw new ArgumentNullException("obj");

            var type   = obj.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Public     | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var props  = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var cf = new ConfigFile(keyword);

            var valid = new List<Tuple<ConfigEntry, System.Reflection.FieldInfo, System.Reflection.PropertyInfo>>();

            foreach (var f in fields)
            {
                var attrs = f.GetCustomAttributes(typeof(ConfigValueAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;

                var a = attrs[0] as ConfigValueAttribute;
                if (a == null)
                    continue;

                var val     = ConfigValueAttribute.ToValue(f.FieldType, f.GetValue(obj));
                var setting = cf.AddSetting(a.Keyword, val, a.Name, a.Description, a.Flags);
                valid.Add(new Tuple<ConfigEntry, System.Reflection.FieldInfo, System.Reflection.PropertyInfo>(setting, f, null));
            }

            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes(typeof(ConfigValueAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;

                var a = attrs[0] as ConfigValueAttribute;
                if (a == null)
                    continue;

                if (p.GetMethod == null || p.SetMethod == null)
                    continue;

                var val     = ConfigValueAttribute.ToValue(p.PropertyType, p.GetValue(obj));
                var setting = cf.AddSetting(a.Keyword, val, a.Name, a.Description, a.Flags);
                valid.Add(new Tuple<ConfigEntry, System.Reflection.FieldInfo, System.Reflection.PropertyInfo>(setting, null, p));
            }

            if (valid.Count == 0)
                throw new ArgumentException("Didn't find any valid fields or properties with the ConfigValueAttribute attribute!", "T");

            if (!cf.Load())
            {
                if (createIfMissing)
                    cf.Save();
                return false;
            }

            foreach (var t in valid)
            {
                var val = t.Item1.CurrentValue;
                if (val == null)
                    continue;

                if (t.Item2 != null)
                    try
                    {
                        var v = ConfigValueAttribute.FromValue(t.Item2.FieldType, val);
                        t.Item2.SetValue(obj, v);
                    }
                    catch (InvalidCastException) { }
                else if (t.Item3 != null)
                    try
                    {
                        var v = ConfigValueAttribute.FromValue(t.Item3.PropertyType, val);
                        t.Item3.SetValue(obj, v);
                    }
                    catch (InvalidCastException) { }
                else
                    throw new InvalidOperationException();
            }

            return true;
        }

        /// <summary>
        /// (Re)load configuration from file. Settings are not reset if we didn't load.
        /// </summary>
        public bool Load()
        {
            // Check if directory exists.
            if (!CheckFolderExists(false))
                return false;

            // Get file path.
            var filePath = GenerateFilePath();

            // Get file info.
            var file = new FileInfo(filePath);

            // File does not exist.
            if (!file.Exists)
                return false;

            // Try to read.
            using (var reader = new StreamReader(file.FullName))
            {
                // All currently registered entries.
                var all = Entries.Values.ToList();

                // All custom new entries found in file.
                var custom = new List<ConfigEntry>();

                // Temporary list of all processed entries.
                var temp = new List<string>();

                // Parse whole file.
                string l;
                while ((l = reader.ReadLine()) != null)
                    ParseLine(l, all, custom, temp);

                // Register new custom entries.
                if (custom.Count != 0)
                    foreach (var e in custom)
                        _addCustomSetting(e);
            }

            // Set loaded from file.
            IsLoadedFromFile = true;
            return true;
        }

        /// <summary>
        /// Mark all settings as modified. This will rewrite all comment blocks and settings to file.
        /// </summary>
        public void MarkAllAsModified()
        {
            // Set all entries as modified.
            foreach (var x in Entries.Values)
                x.IsModified = true;

            // Set file as modified.
            IsModifiedSinceSave = true;
        }

        /// <summary>
        /// Save all current settings to file. If file does not exist it will be created.
        /// </summary>
        public void Save()
        {
            // Check if directory exists.
            if (!CheckFolderExists(true))
                return;

            // Get file path.
            var filePath = GenerateFilePath();

            // Create file writer.
            using (var writer = new StreamWriter(filePath, false))
            {
                // Gather all newest settings to previous lines if we had any.
                if (Lines != null)
                {
                    Lines.Modify(Entries.Values);
                    Lines.WriteToFile(writer);
                }
                else
                {
                    // Let all entries write to file.
                    foreach (var entry in Entries.Values)
                        entry.WriteToFile(writer);
                }
            }

            // Set not modified to all entries.
            foreach (var entry in Entries.Values)
                entry.IsModified = false;

            // Not modified file anymore.
            IsModifiedSinceSave = false;
        }

        /// <summary>
        /// Reset all settings to default.
        /// </summary>
        public void Reset()
        {
            // Reset all to default.
            foreach (var e in Entries.Values)
            {
                e.CurrentValue = e.DefaultValue;
                e.IsModified   = false;
            }

            // Not loaded from file anymore.
            IsLoadedFromFile = false;
        }

        /// <summary>
        /// Check if settings were loaded from file, otherwise we are using default settings.
        /// </summary>
        public bool IsLoadedFromFile
        {
            get => _loadedFromFile;
            private set => _loadedFromFile = value;
        }

        /// <summary>
        /// Check if configuration has been modified since last saving.
        /// </summary>
        public bool IsModifiedSinceSave
        {
            get => _modified;
            private set => _modified = value;
        }

        /// <summary>
        /// Add setting to file. Entry is returned so you may add restricting values to it.
        /// </summary>
        /// <param name="keyword">Keyword of setting. This is used to fetch and set value.</param>
        /// <param name="value">Default value of setting.</param>
        /// <param name="name">Name of setting. Optional.</param>
        /// <param name="description">Description of setting. Optional.</param>
        /// <param name="flags">Options.</param>
        /// <returns></returns>
        public ConfigEntry AddSetting(string keyword, Value value, string name = null, string description = null, ConfigEntryFlags flags = ConfigEntryFlags.None)
        {
            var entry = new ConfigEntry(keyword, value, name, description, flags);
            Entries[entry.Keyword] = entry;
            return entry;
        }

        /// <summary>
        /// Get value of a setting by keyword.
        /// </summary>
        /// <param name="keyword">Keyword of a setting.</param>
        /// <returns></returns>
        public Value GetValue(string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException("keyword");

            return Entries.ContainsKey(keyword) ? Entries[keyword].CurrentValue : null;
        }

        /// <summary>
        /// Get value of a setting by keyword.
        /// </summary>
        /// <param name="keyword">Keyword of a setting.</param>
        /// <returns></returns>
        public Value GetDefaultValue(string keyword)
        {
            if (keyword == null)
                throw new ArgumentNullException("keyword");

            return Entries.ContainsKey(keyword) ? Entries[keyword].DefaultValue : null;
        }

        /// <summary>
        /// Try to set value of a setting. If we could not set then we return false.
        /// </summary>
        /// <param name="keyword">Keyword of a setting.</param>
        /// <param name="value">Value to set.</param>
        /// <returns></returns>
        public bool SetValue(string keyword, string value)
        {
            if (keyword == null)
                throw new ArgumentNullException("keyword");
            if (value == null)
                throw new ArgumentNullException("value");

            var entry = GetEntry(keyword);
            if (entry != null)
                return entry.TrySetValue(value);
            return false;
        }

    #endregion

    #region Internal members

        /// <summary>
        /// Get entry by keyword.
        /// </summary>
        /// <param name="keyword">Keyword of entry.</param>
        /// <returns></returns>
        internal ConfigEntry GetEntry(string keyword) { return Entries.ContainsKey(keyword) ? Entries[keyword] : null; }

        /// <summary>
        /// Add a new custom user setting.
        /// </summary>
        /// <param name="e">Entry.</param>
        internal void _addCustomSetting(ConfigEntry e) { Entries[e.Keyword] = e; }

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
            strFile.Append(".txt");

            var fullPath = strFile.ToString();
            if (!string.IsNullOrEmpty(Path))
                fullPath = System.IO.Path.Combine(Path, fullPath);

            return fullPath;
        }

        /// <summary>
        /// Check if folder to file exists.
        /// </summary>
        /// <param name="allowCreate">Allow create if it doesn't exist.</param>
        /// <returns></returns>
        private bool CheckFolderExists(bool allowCreate)
        {
            // Get path.
            var pathName = Path;

            // Executable folder, this always exists.
            if (string.IsNullOrEmpty(pathName))
                return true;

            // Get info.
            var dir = new DirectoryInfo(pathName);

            // Already exists.
            if (dir.Exists)
                return true;

            // Want to create.
            if (allowCreate)
            {
                dir.Create();
                return true;
            }

            // Didn't exist.
            return false;
        }

        /// <summary>
        /// Parse one line of configuration file.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <param name="all">All entries.</param>
        /// <param name="custom">Custom entries.</param>
        /// <param name="loaded">Already loaded setting entries.</param>
        private void ParseLine(string line, List<ConfigEntry> all, List<ConfigEntry> custom, List<string> loaded)
        {
            // Create new lines.
            if (Lines == null)
                Lines = new ConfigLines();

            // Parse one line.
            Lines.ParseLine(line, all, custom, loaded, this);
        }

        private          bool                            _loadedFromFile = false;
        private          bool                            _modified       = false;
        private readonly Dictionary<string, ConfigEntry> Entries         = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);
        private          ConfigFileFlags                 Flags           = ConfigFileFlags.None;
        private          ConfigLines                     Lines           = null;

    #endregion
    }

#endregion

#region ConfigFile enums

    /// <summary>
    /// Options for configuration file.
    /// </summary>
    [Flags]
    internal enum ConfigFileFlags : uint
    {
        None = 0,

        /// <summary>
        /// Don't automatically save file.
        /// </summary>
        ManualSave = 1,

        /// <summary>
        /// Don't automatically create file.
        /// </summary>
        ManualCreate = 2
    }

#endregion

#region ConfigLine class

    /// <summary>
    /// One line of configuration file. This is used internally to preserve manual changes to configuration file.
    /// </summary>
    internal sealed class ConfigLine
    {
    #region Constructors

        /// <summary>
        /// Create new line.
        /// </summary>
        internal ConfigLine() { }

    #endregion

    #region ConfigLine members

        /// <summary>
        /// Text of line.
        /// </summary>
        internal string Text = "";

        /// <summary>
        /// Type of line.
        /// </summary>
        internal ConfigLineTypes Type = ConfigLineTypes.Ignored;

        /// <summary>
        /// If this is a setting then the keyword of what is set will be here.
        /// </summary>
        internal string Keyword = "";

    #endregion
    }

#endregion

#region ConfigLine enums

    /// <summary>
    /// All possible configuration file line types.
    /// </summary>
    internal enum ConfigLineTypes : int
    {
        /// <summary>
        /// Ignored line, but not comment. This can be empty line, spaces or other such things.
        /// </summary>
        Ignored = 0,

        /// <summary>
        /// Commented line.
        /// </summary>
        Comment,

        /// <summary>
        /// One configuration setting.
        /// </summary>
        Setting
    }

#endregion

#region ConfigLines class

    /// <summary>
    /// This class helps preserve manual changes to configuration files.
    /// </summary>
    internal sealed class ConfigLines
    {
    #region Constructors

        /// <summary>
        /// Create new lines collection.
        /// </summary>
        internal ConfigLines() { }

    #endregion

    #region ConfigLines members

        /// <summary>
        /// Parse one line from file.
        /// </summary>
        /// <param name="line">Line as is.</param>
        /// <param name="all">All registered entries.</param>
        /// <param name="custom">Any new custom entries will be added here and later registered.</param>
        /// <param name="processed">Already processed setting keywords.</param>
        /// <param name="file">File that is processing.</param>
        internal void ParseLine(string line, List<ConfigEntry> all, List<ConfigEntry> custom, List<string> processed, ConfigFile file)
        {
            // This shouldn't happen.
            if (line == null)
                throw new ArgumentNullException("line");

            // Trim line.
            var text = line.Trim();

            // Empty line.
            if (text.Length == 0)
            {
                AddEmptyLine(line);
                return;
            }

            // Comment character.
            if (text[0] == '#')
            {
                // Add new comment as is (line instead of text).
                AddNewComment(line);
                return;
            }

            // Try to parse keyword.
            string keyword = null;
            for (var i = 0; i < text.Length; i++)
                if (char.IsWhiteSpace(text[i]))
                {
                    keyword = text.Substring(0, i);
                    text    = text.Substring(i).Trim();
                    break;
                }

            // Invalid syntax.
            if (keyword == null || text.Length == 0 || text[0] != '=')
            {
                MakeError(new string[] {"Invalid syntax for line! Setting should be in format \"Keyword = Value\"", line});
                return;
            }

            // Remove assign character.
            text = text.Substring(1).Trim();

            // Check if no save.
            {
                var en = file.GetEntry(keyword);
                if (en != null && (en.Flags & ConfigEntryFlags.NoSave) != ConfigEntryFlags.None)
                {
                    MakeError(new string[] {"This setting is not allowed to be modified from configuration file!", line});
                    return;
                }
            }

            // Already processed this. Go back and comment it out.
            if (processed.Contains(keyword, StringComparer.OrdinalIgnoreCase))
            {
                // Find previous setting of same name.
                var settingIndex = FindSetting(keyword);

                // Make previous as error.
                MakeError(new string[] {"Found duplicate entry for this setting! Using last assigned below."}, settingIndex);

                // Remove from custom list, we will create a new one since it might have a different value type.
                custom.RemoveAll(q => q.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase));
            }

            // Add to processed list.
            processed.Add(keyword);

            // Try to find registered entry first.
            var entry = all.FirstOrDefault(q => q.Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase));

            // Not registered, we must create a custom entry.
            if (entry == null)
            {
                Value dv = null;
                if (text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"')
                {
                    text = text.Substring(1, text.Length - 2);
                    dv   = new Value(text);
                }
                else
                {
                    var vl = new Value(text);
                    {
                        long rl = 0;
                        if (vl.TryToInt64(out rl))
                            dv = new Value(rl);
                    }
                    if (dv == null)
                    {
                        var rl = 0.0;
                        if (vl.TryToDouble(out rl))
                            dv = new Value(rl);
                    }

                    if (dv == null)
                        dv = new Value(text);
                }

                entry = new ConfigEntry(keyword, dv, null, null, ConfigEntryFlags.Custom);
                custom.Add(entry);
                return;
            }

            // Remove quotes.
            if (entry.DefaultValue != null && (entry.DefaultValue.CurrentType == TypeCode.String || entry.DefaultValue.CurrentType == TypeCode.Char))
            {
                // Remove quotes but don't trim!
                if (text.Length >= 2 && text[0] == '"' && text[text.Length       - 1] == '"')
                    text = text.Substring(1, text.Length                         - 2);
                else if (text.Length >= 2 && text[0] == '\'' && text[text.Length - 1] == '\'')
                    text = text.Substring(1, text.Length                         - 2);
            }

            // Set new value.
            if (!entry.TrySetValue(text))
                AddNewSetting(entry.GenerateSetting(), entry.Keyword);
            else
                AddNewSetting(line, keyword);
        }

        /// <summary>
        /// Modify lines of configuration files.
        /// </summary>
        /// <param name="modifiedEntries">Collection of modified entries.</param>
        /// <param name="force">Force modify even if nothing changed.</param>
        internal void Modify(IEnumerable<ConfigEntry> modifiedEntries, bool force = false)
        {
            // Get all for reference.
            var all = modifiedEntries.ToList();

            // Modify each.
            foreach (var e in modifiedEntries)
                Modify(e, all, force);
        }

        /// <summary>
        /// Write all lines to file.
        /// </summary>
        /// <param name="writer">File stream.</param>
        internal void WriteToFile(StreamWriter writer)
        {
            foreach (var line in Lines)
                writer.WriteLine(line.Text);
        }

    #endregion

    #region Internal members

        /// <summary>
        /// Comment block top line.
        /// </summary>
        internal const string CommentBlockTop =
            "########################################################################################################################";

        /// <summary>
        /// Comment block bottom line.
        /// </summary>
        internal const string CommentBlockBottom =
            "########################################################################################################################";

        /// <summary>
        /// Comment block left (start of line).
        /// </summary>
        internal const string CommentBlockLeft = "#";

        /// <summary>
        /// Comment block right (end of line).
        /// </summary>
        internal const string CommentBlockRight = "#";

        /// <summary>
        /// How many lines to add between top of comment block and first line of text.
        /// </summary>
        internal const int CommentBlockTopMargin = 0;

        /// <summary>
        /// How many lines to add between bottom of comment block and last line of text.
        /// </summary>
        internal const int CommentBlockBottomMargin = 0;

        /// <summary>
        /// How many spaces to add between left side of comment block and first letter.
        /// </summary>
        internal const int CommentBlockLeftMargin = 1;

        /// <summary>
        /// How many spaces to add between right side of comment block and last letter.
        /// </summary>
        internal const int CommentBlockRightMargin = 1;

        /// <summary>
        /// Modify one entry.
        /// </summary>
        /// <param name="entry">Entry to modify.</param>
        /// <param name="all">All, for reference.</param>
        /// <param name="force">Force modify.</param>
        private void Modify(ConfigEntry entry, List<ConfigEntry> all, bool force)
        {
            // This entry was not modified.
            if (!entry.IsModified && !force)
                return;

            // Setting already exists.
            if (FindSetting(entry.Keyword) >= 0)
            {
                // Try to rewrite comment block if we can.
                TryModifyCommentBlock(entry, all);

                // Try to rewrite setting.
                TryModifySetting(entry, all);
            }
            else // Add new setting.
            {
                // Add setting.
                AddNewSetting(entry, all);
            }
        }

        /// <summary>
        /// Try to modify comment block.
        /// </summary>
        /// <param name="entry">Entry to replace comment block on.</param>
        /// <param name="all">All, for reference.</param>
        private void TryModifyCommentBlock(ConfigEntry entry, List<ConfigEntry> all)
        {
            // Find setting first.
            var settingIndex = FindSetting(entry.Keyword);

            // Setting isn't present so we can't modify the comment block.
            if (settingIndex < 0)
                return;

            // Get previous index.
            var previousSettingIndex = FindPreviousSetting(settingIndex);

            // Increase it by one, we only want to check next. It's ok if not found because that will become the first line.
            previousSettingIndex++;

            // Find end of comment block.
            var commentBlockEnd = FindEndOfComment(previousSettingIndex, settingIndex);

            // Not valid block.
            if (commentBlockEnd < 0)
                return;

            // Find start of comment block.
            var commentBlockStart = FindStartOfComment(previousSettingIndex, commentBlockEnd);

            // Didn't find, not valid.
            if (commentBlockStart < 0)
                return;

            // Found ambiguous comment block.
            if (HasMoreBlock(previousSettingIndex, commentBlockStart))
                return;

            // Remove current comment block.
            Lines.RemoveRange(commentBlockStart, commentBlockEnd - commentBlockStart + 1);

            // Generate new comment block.
            var block = entry.GenerateCommentBlock();

            // Add new comment block.
            AddNewCommentBlock(block, commentBlockStart);
        }

        /// <summary>
        /// Try to modify setting.
        /// </summary>
        /// <param name="entry">Entry to modify.</param>
        /// <param name="all">All, for reference.</param>
        private void TryModifySetting(ConfigEntry entry, List<ConfigEntry> all)
        {
            var settingIndex = FindSetting(entry.Keyword);
            if (settingIndex < 0)
                throw new InvalidOperationException();

            Lines[settingIndex].Text = entry.GenerateSetting();
        }

        /// <summary>
        /// Add new setting to lines.
        /// </summary>
        /// <param name="entry">Entry to add.</param>
        /// <param name="all">All, for reference.</param>
        private void AddNewSetting(ConfigEntry entry, List<ConfigEntry> all)
        {
            // Find proper index to add to.
            var indexToAdd = -1;
            var indexInAll = all.IndexOf(entry);
            if (indexInAll < 0)
                throw new InvalidOperationException();

            // See if we have previous setting added.
            for (var i = indexInAll - 1; i >= 0; i--)
            {
                // Get previous index.
                var previousIndex = FindSetting(all[i].Keyword);
                if (previousIndex >= 0)
                {
                    indexToAdd = previousIndex + 1;
                    break;
                }
            }

            // Didn't find.
            if (indexToAdd == -1)
            {
                // See if we have next setting added.
                for (var i = indexInAll + 1; i < all.Count; i++)
                {
                    // Get next index.
                    var nextIndex = FindSetting(all[i].Keyword);
                    if (nextIndex >= 0)
                    {
                        var previousNext = FindPreviousSetting(nextIndex);
                        if (previousNext >= 0)
                            indexToAdd = previousNext + 1;
                        else
                            indexToAdd = 0;
                        break;
                    }
                }

                // Didn't find any suitable, add to start of file.
                if (indexToAdd == -1)
                    indexToAdd = 0;
            }

            // Add empty line before block and increase index.
            AddEmptyLine(null, indexToAdd++);

            // Add block first.
            var block = entry.GenerateCommentBlock();
            AddNewCommentBlock(block, indexToAdd);

            // Increase index because of comment block.
            indexToAdd += block.Count;

            // Add empty line only when necessary.
            if (Lines[indexToAdd - 1].Type != ConfigLineTypes.Ignored)
                AddEmptyLine(null, indexToAdd++);

            // Write setting.
            AddNewSetting(entry.GenerateSetting(), entry.Keyword, indexToAdd);

            // Add empty line after setting.
            AddEmptyLine(null, ++indexToAdd);
        }

        /// <summary>
        /// Add new setting to lines.
        /// </summary>
        /// <param name="line">Line as is.</param>
        /// <param name="keyword">Keyword of setting for line entry.</param>
        /// <param name="index">Index where to add. If negative then add to end.</param>
        private void AddNewSetting(string line, string keyword, int index = -1)
        {
            // Invalid line.
            if (line == null)
                throw new ArgumentNullException("line");
            if (keyword == null)
                throw new ArgumentNullException("keyword");

            // Create new line.
            var cline = new ConfigLine();
            cline.Text    = line;
            cline.Type    = ConfigLineTypes.Setting;
            cline.Keyword = keyword;

            // Append.
            if (index < 0) { Lines.Add(cline); }
            else
            {
                // Must be valid index.
                if (index > Lines.Count)
                    throw new ArgumentOutOfRangeException("index");

                // Insert to lines.
                Lines.Insert(index, cline);
            }
        }

        /// <summary>
        /// Insert a new comment block to lines.
        /// </summary>
        /// <param name="block">Block lines.</param>
        /// <param name="startIndex">Starting index. If negative then add to end.</param>
        private void AddNewCommentBlock(List<string> block, int startIndex = -1)
        {
            // Add each line.
            for (var i = 0; i < block.Count; i++)
                AddNewComment(block[i], startIndex < 0 ? startIndex : startIndex + i);
        }

        /// <summary>
        /// Add a new comment line.
        /// </summary>
        /// <param name="line">Line to add.</param>
        /// <param name="startIndex">Start index. If negative then add to end.</param>
        private void AddNewComment(string line, int startIndex = -1)
        {
            // Create new line.
            var cline = new ConfigLine();
            cline.Text    = line;
            cline.Type    = ConfigLineTypes.Comment;
            cline.Keyword = "";

            // Append.
            if (startIndex < 0) { Lines.Add(cline); }
            else
            {
                // Must be valid index.
                if (startIndex > Lines.Count)
                    throw new ArgumentOutOfRangeException("startIndex");

                // Insert to lines.
                Lines.Insert(startIndex, cline);
            }
        }

        /// <summary>
        /// Add empty line to file.
        /// </summary>
        /// <param name="line">Custom line. If null then create empty line.</param>
        /// <param name="startIndex">Start index. If negative then add to end.</param>
        private void AddEmptyLine(string line = null, int startIndex = -1)
        {
            // Create new line.
            var cline = new ConfigLine();
            cline.Text    = line ?? string.Empty;
            cline.Type    = ConfigLineTypes.Ignored;
            cline.Keyword = "";

            // Append.
            if (startIndex < 0) { Lines.Add(cline); }
            else
            {
                // Must be valid index.
                if (startIndex > Lines.Count)
                    throw new ArgumentOutOfRangeException("startIndex");

                // Insert to lines.
                Lines.Insert(startIndex, cline);
            }
        }

        /// <summary>
        /// This is used to check if comment block is unambiguous.
        /// </summary>
        /// <param name="start">Start index.</param>
        /// <param name="end">End index (excluded).</param>
        /// <returns></returns>
        private bool HasMoreBlock(int start, int end)
        {
            // Check all lines.
            for (var i = start; i < end; i++)
                // Comment block separator. Means we have ambiguous comment block.
                if (Lines[i].Text == CommentBlockBottom || Lines[i].Text == CommentBlockTop)
                    return true;

            // Everything looks ok.
            return false;
        }

        /// <summary>
        /// Find start of comment block for specific setting. This returns line index where the comment block starts.
        /// If not found then negative value is returned.
        /// </summary>
        /// <param name="minIndex">Minimum index allowed to search.</param>
        /// <param name="endIndex">End index of comment block.</param>
        /// <returns></returns>
        private int FindStartOfComment(int minIndex, int endIndex)
        {
            // Setting was not found.
            if (endIndex < 0)
                return -1;

            // Go backwards.
            for (var i = endIndex - 1; i >= minIndex; i--)
                // This is the start line.
                if (Lines[i].Text == CommentBlockTop)
                    return i;

            // Not found.
            return -1;
        }

        /// <summary>
        /// Find end of comment block for specific setting. This returns line index where the comment block ends.
        /// The line index is included in the comment block. If not found then negative value is returned.
        /// </summary>
        /// <param name="minIndex">Minium index allowed to search.</param>
        /// <param name="settingIndex">Index of setting.</param>
        /// <returns></returns>
        private int FindEndOfComment(int minIndex, int settingIndex)
        {
            // Setting was not found.
            if (settingIndex < 0)
                return -1;

            // Go backwards.
            for (var i = settingIndex - 1; i >= minIndex; i--)
                // This is the correct line.
                if (Lines[i].Text == CommentBlockBottom)
                    return i;

            // Not found.
            return -1;
        }

        /// <summary>
        /// Find setting line. If not found then negative value is returned.
        /// </summary>
        /// <param name="keyword">Keyword to find for.</param>
        /// <returns></returns>
        private int FindSetting(string keyword)
        {
            // Not a valid keyword.
            if (keyword == null)
                throw new ArgumentNullException("keyword");
            if (keyword.Length == 0)
                throw new ArgumentOutOfRangeException("keyword");

            // Check each line.
            for (var i = 0; i < Lines.Count; i++)
            {
                // Not a setting.
                if (Lines[i].Type != ConfigLineTypes.Setting)
                    continue;

                // Not the right setting.
                if (!Lines[i].Keyword.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Found.
                return i;
            }

            // Not found.
            return -1;
        }

        /// <summary>
        /// Find previous setting index of a keyword.
        /// </summary>
        /// <param name="settingIndex">Index of current setting.</param>
        /// <returns></returns>
        private int FindPreviousSetting(int settingIndex)
        {
            // Go backwards until we find another setting.
            for (var i = settingIndex - 1; i >= 0; i--)
            {
                // Not a setting.
                if (Lines[i].Type != ConfigLineTypes.Setting)
                    continue;

                // Found.
                return i;
            }

            // Not found.
            return -1;
        }

        /// <summary>
        /// All added lines.
        /// </summary>
        private readonly List<ConfigLine> Lines = new List<ConfigLine>();

        /// <summary>
        /// Make an error message.
        /// </summary>
        /// <param name="text">Text to set as error. This will be in comments.</param>
        /// <param name="index">Index of error, if negative then append to end, otherwise make that indexed line as error.
        /// This offsets all other lines.</param>
        private void MakeError(string[] text, int index = -1)
        {
            // Find error line.
            if (index >= 0)
            {
                // Must be a valid line.
                if (index > Lines.Count)
                    throw new ArgumentOutOfRangeException("index");

                // Turn it into a comment.
                Lines[index].Type    = ConfigLineTypes.Comment;
                Lines[index].Text    = "# " + Lines[index].Text;
                Lines[index].Keyword = "";

                // Insert message before the newly commented line.
                foreach (var x in text)
                    AddNewComment(x, index++);

                return;
            }

            // Append new error to end.
            foreach (var x in text)
                AddNewComment(x);
        }

    #endregion
    }

#endregion

    /// <summary>
    /// Attribute for a configuration value.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ConfigValueAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigValueAttribute"/> class.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="flags">Options for the value.</param>
        public ConfigValueAttribute(string keyword, string name, string description = null, ConfigEntryFlags flags = ConfigEntryFlags.None)
        {
            if (string.IsNullOrEmpty(keyword))
                throw new ArgumentOutOfRangeException("keyword");

            Keyword     = keyword;
            Name        = name;
            Description = description;
            Flags       = flags;
        }

        /// <summary>
        /// The keyword.
        /// </summary>
        public readonly string Keyword;

        /// <summary>
        /// The name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The description.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// The flags of entry.
        /// </summary>
        public readonly ConfigEntryFlags Flags;

        /// <summary>
        /// Convert from base object to our value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Unhandled type in configuration class ( + type.Name + )!;type</exception>
        internal static Value ToValue(Type type, object value)
        {
            if (type == typeof(bool))
                return new Value((bool) value);
            if (type == typeof(sbyte))
                return new Value((sbyte) value);
            if (type == typeof(byte))
                return new Value((byte) value);
            if (type == typeof(short))
                return new Value((short) value);
            if (type == typeof(ushort))
                return new Value((ushort) value);
            if (type == typeof(int))
                return new Value((int) value);
            if (type == typeof(uint))
                return new Value((uint) value);
            if (type == typeof(long))
                return new Value((long) value);
            if (type == typeof(ulong))
                return new Value((ulong) value);
            if (type == typeof(float))
                return new Value((float) value);
            if (type == typeof(double))
                return new Value((double) value);
            if (type == typeof(decimal))
                return new Value((decimal) value);
            if (type == typeof(DateTime))
                return new Value((DateTime) value);
            if (type == typeof(string))
                return new Value((string) value);

            throw new ArgumentException("Unhandled type in configuration class (" + type.Name + ")!", "type");
        }

        /// <summary>
        /// Convert from our value to underlying object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidCastException">
        /// </exception>
        /// <exception cref="System.ArgumentException">Unhandled type in configuration class ( + type.Name + )!;type</exception>
        internal static object FromValue(Type type, Value value)
        {
            if (type == typeof(bool))
            {
                var v = false;
                if (!value.TryToBoolean(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(sbyte))
            {
                sbyte v = 0;
                if (!value.TryToSByte(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(byte))
            {
                byte v = 0;
                if (!value.TryToByte(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(short))
            {
                short v = 0;
                if (!value.TryToInt16(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(ushort))
            {
                ushort v = 0;
                if (!value.TryToUInt16(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(int))
            {
                var v = 0;
                if (!value.TryToInt32(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(uint))
            {
                uint v = 0;
                if (!value.TryToUInt32(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(long))
            {
                long v = 0;
                if (!value.TryToInt64(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(ulong))
            {
                ulong v = 0;
                if (!value.TryToUInt64(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(float))
            {
                float v = 0;
                if (!value.TryToSingle(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(double))
            {
                double v = 0;
                if (!value.TryToDouble(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(decimal))
            {
                decimal v = 0;
                if (!value.TryToDecimal(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(DateTime))
            {
                var v = default(DateTime);
                if (!value.TryToDateTime(out v))
                    throw new InvalidCastException("Unable to cast to underlying type in configuration value (" + type.Name + ")!");
                return v;
            }

            if (type == typeof(string))
            {
                var v = value.ToString();
                return v;
            }

            throw new ArgumentException("Unhandled type in configuration class (" + type.Name + ")!", "type");
        }
    }
}