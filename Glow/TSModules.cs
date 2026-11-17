using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glow
{
    // TS UI THREAD HELPER
    // ======================================================================================================
    public static class TS_UIThreadHelper
    {
        private static void InvokeIfRequired(Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;
            if (control.InvokeRequired)
            {
                control.BeginInvoke((MethodInvoker)(() => { if (!control.IsDisposed) action(); }));
            }
            else { action(); }
        }
        //
        public static void ExecuteSafe(this Control control, Action action)
        {
            InvokeIfRequired(control, action);
        }
        public static void SetTextSafe(this Control control, string text)
        {
            InvokeIfRequired(control, () => control.Text = text);
        }
        public static void SetEnabledSafe(this Control control, bool enabled)
        {
            InvokeIfRequired(control, () => control.Enabled = enabled);
        }
        public static void SetVisibleSafe(this Control control, bool visible)
        {
            InvokeIfRequired(control, () => control.Visible = visible);
        }
        public static void AddItemSafe(this ComboBox comboBox, object item)
        {
            InvokeIfRequired(comboBox, () => comboBox.Items.Add(item));
        }
        //
        public static void SetEnabledSafe(this ToolStripItem item, bool enabled)
        {
            if (item == null) return;
            if (item.Owner != null) InvokeIfRequired(item.Owner, () => item.Enabled = enabled);
            else item.Enabled = enabled;
        }
        public static void SetVisibleSafe(this ToolStripItem item, bool visible)
        {
            if (item == null) return;
            if (item.Owner != null) InvokeIfRequired(item.Owner, () => item.Visible = visible);
            else item.Visible = visible;
        }
        public static void SetTextSafe(this ToolStripItem item, string text)
        {
            if (item == null) return;
            if (item.Owner != null) InvokeIfRequired(item.Owner, () => item.Text = text);
            else item.Text = text;
        }
    }
    internal class TSModules
    {
        // STARTUP LOCATION
        // ======================================================================================================
        private static readonly string StartupPath = Application.StartupPath;
        // LINK SYSTEM
        // ======================================================================================================
        public class TS_LinkSystem
        {
            public const string
            // Main Control Links
            github_link_lv = "https://raw.githubusercontent.com/turkaysoft/glow/main/Glow/SoftwareVersion.txt",
            github_link_lr = "https://github.com/turkaysoft/glow/releases/latest",
            // Social Links
            website_link = "https://turkaysoft.com",
            github_link = "https://github.com/turkaysoft",
            // Other Links
            ts_donate = "https://buymeacoffee.com/turkaysoft";
        }
        // VERSIONS
        // ======================================================================================================
        public class TS_VersionEngine
        {
            public static string TS_SoftwareVersion(int v_mode)
            {
                string version_mode = "";
                switch (v_mode)
                {
                    case 0:
                        version_mode = string.Format("{0} - v{1}", Application.ProductName, TS_VersionParser.ParseUINormalize(Application.ProductVersion));
                        break;
                    case 1:
                        version_mode = string.Format("v{0}", TS_VersionParser.ParseUINormalize(Application.ProductVersion));
                        break;
                }
                return version_mode;
            }
        }
        // VERSION PARSER
        // ======================================================================================================
        public static class TS_VersionParser
        {
            public static string ParseUINormalize(string version)
            {
                Version v = Normalize(version);
                if (v.Revision > 0)
                    return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                if (v.Build > 0)
                    return $"{v.Major}.{v.Minor}.{v.Build}";
                return $"{v.Major}.{v.Minor}";
            }
            private static Version Normalize(string version)
            {
                if (string.IsNullOrWhiteSpace(version))
                    return new Version(0, 0, 0, 0);
                version = version.Trim();
                var parts = version.Split('.');
                while (parts.Length < 4)
                {
                    version += ".0";
                    parts = version.Split('.');
                }
                if (!Version.TryParse(version, out var v))
                    v = new Version(0, 0, 0, 0);
                return v;
            }
        }
        // TS MESSAGEBOX ENGINE
        // ======================================================================================================
        public static class TS_MessageBoxEngine
        {
            private static readonly Dictionary<int, KeyValuePair<MessageBoxButtons, MessageBoxIcon>> TSMessageBoxConfig = new Dictionary<int, KeyValuePair<MessageBoxButtons, MessageBoxIcon>>(){
                { 1, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OK, MessageBoxIcon.Information) },               // Ok and Info
                { 2, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OK, MessageBoxIcon.Warning) },                   // Ok and Warning
                { 3, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OK, MessageBoxIcon.Error) },                     // Ok and Error
                { 4, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Question) },               // Yes/No and Quest
                { 5, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Information) },            // Yes/No and Info
                { 6, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Warning) },                // Yes/No and Warning
                { 7, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Error) },                  // Yes/No and Error
                { 8, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) },            // Retry/Cancel and Error
                { 9, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) },         // Yes/No/Cancel and Quest
                { 10, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information) },     // Yes/No/Cancel and Info
                { 11, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) },         // Yes/No/Cancel and Warning
                // OK / Cancel combinations
                { 12, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OKCancel, MessageBoxIcon.Question) },           // OK/Cancel and Question
                { 13, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OKCancel, MessageBoxIcon.Information) },        // OK/Cancel and Info
                { 14, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) },            // OK/Cancel and Warning
                { 15, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OKCancel, MessageBoxIcon.Error) },              // OK/Cancel and Error
                // Retry / Cancel additional variants
                { 16, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.RetryCancel, MessageBoxIcon.Information) },     // Retry/Cancel and Info
                { 17, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) },         // Retry/Cancel and Warning
                { 18, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.RetryCancel, MessageBoxIcon.Question) },        // Retry/Cancel and Question
                // Abort / Retry / Ignore combinations
                { 19, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) },   // Abort/Retry/Ignore and Question
                { 20, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Information) },// Abort/Retry/Ignore and Info
                { 21, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning) },    // Abort/Retry/Ignore and Warning
                { 22, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error) }       // Abort/Retry/Ignore and Error
            };
            public static DialogResult TS_MessageBox(Form m_form, int m_mode, string m_message, string m_title = "")
            {
                if (m_form != null && m_form.InvokeRequired)
                {
                    return (DialogResult)m_form.Invoke(new Func<DialogResult>(() => TS_MessageBox(m_form, m_mode, m_message, m_title)));
                }
                if (m_form != null)
                {
                    BringFormToFront(m_form);
                }
                string m_box_title = string.IsNullOrEmpty(m_title) ? Application.ProductName : m_title;
                MessageBoxButtons m_button = MessageBoxButtons.OK;
                MessageBoxIcon m_icon = MessageBoxIcon.Information;
                if (TSMessageBoxConfig.ContainsKey(m_mode))
                {
                    var m_serialize = TSMessageBoxConfig[m_mode];
                    m_button = m_serialize.Key;
                    m_icon = m_serialize.Value;
                }
                return MessageBox.Show(m_form, m_message, m_box_title, m_button, m_icon);
            }
            private static void BringFormToFront(Form m_form)
            {
                if (m_form.WindowState == FormWindowState.Minimized)
                {
                    m_form.WindowState = FormWindowState.Normal;
                }
                m_form.BringToFront();
                m_form.Activate();
            }
        }
        // TS LOGGER - V2.2
        // ======================================================================================================
        public static class TSLogger
        {
            private static readonly object _lock = new object();
            private static bool _fileEnabled;
            private static bool _consoleEnabled;
            private static string _currentLogFile;
            private static readonly string _logDir;
            private static StreamWriter _writer;
            private static bool _isClosed = false;
            private static bool _isUnlocked = false;
            private static bool _hasLogContent = false;
            private const int MaxLogFileCount = 64;
            public static string LogDirectory
            {
                get
                {
                    lock (_lock)
                        return _logDir;
                }
            }
            public static bool IsClosed
            {
                get
                {
                    lock (_lock)
                        return _isClosed;
                }
            }
            public static string CurrentLogFile
            {
                get
                {
                    lock (_lock)
                        return _currentLogFile;
                }
            }
            static TSLogger()
            {
                _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "g_logs");
                try
                {
                    AppDomain.CurrentDomain.ProcessExit += (_, __) => CloseWriter_NoThrow();
                    AppDomain.CurrentDomain.DomainUnload += (_, __) => CloseWriter_NoThrow();
                }
                catch { }
            }
            public static void Enable(bool fileEnabled, bool consoleEnabled)
            {
                lock (_lock)
                {
                    _isClosed = false;
                    _isUnlocked = false;
                    _fileEnabled = fileEnabled;
                    _consoleEnabled = consoleEnabled;
                    CloseWriter_NoThrow();
                    if (!_fileEnabled)
                    {
                        _currentLogFile = null;
                        return;
                    }
                    InitializeInternal_NoThrow(false);
                }
            }
            public static void UnlockFile()
            {
                lock (_lock)
                {
                    if (string.IsNullOrEmpty(_currentLogFile))
                        return;
                    CloseWriter_NoThrow();
                    _isUnlocked = true;
                    if (_consoleEnabled)
                    {
                        string msg = $"[DEBUG] TSLogger: File unlocked - {_currentLogFile}";
                        Console.WriteLine(msg);
                        Debug.WriteLine(msg);
                    }
                }
            }
            public static void Close()
            {
                lock (_lock)
                {
                    _isClosed = true;
                    _isUnlocked = false;
                    _fileEnabled = false;
                    _consoleEnabled = false;
                    CloseWriter_NoThrow();
                    _currentLogFile = null;
                }
            }
            public static void Log(string message)
            {
                lock (_lock)
                {
                    if (_isClosed || (!_fileEnabled && !_consoleEnabled))
                        return;
                    Write_NoLock(message);
                }
            }
            public static void Log(Exception ex)
            {
                if (ex == null)
                    return;
                bool isClosed;
                lock (_lock)
                    isClosed = _isClosed;
                if (isClosed)
                    return;
                Log(ex.ToString());
            }
            private static void InitializeInternal_NoThrow(bool keepUnlocked = true)
            {
                if (_isClosed) return;
                try
                {
                    if (!Directory.Exists(_logDir))
                    {
                        Directory.CreateDirectory(_logDir);
                    }
                    TrimOldLogFiles_NoThrow();
                    string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    string baseName = $"{Application.ProductName}_{stamp}";
                    string path;
                    for (int i = 0; i < 1000; i++)
                    {
                        string suffix = (i == 0) ? "" : "_" + i.ToString();
                        path = Path.Combine(_logDir, baseName + suffix + ".log");
                        FileStream fs = null;
                        try
                        {
                            fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            _currentLogFile = path;
                            _writer = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                            {
                                AutoFlush = true
                            };
                            _hasLogContent = false;
                            if (!keepUnlocked)
                            {
                                _isUnlocked = false;
                            }
                            return;
                        }
                        catch (IOException)
                        {
                            fs?.Dispose();
                            continue;
                        }
                    }
                    _writer = null;
                    _currentLogFile = null;
                    _fileEnabled = false;
                    if (_consoleEnabled)
                    {
                        string errorMsg = "[DEBUG] " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss") + " - TSLogger: Failed to create log file (attempted 1000 names). File logging disabled.";
                        Console.WriteLine(errorMsg);
                        Debug.WriteLine(errorMsg);
                    }
                }
                catch
                {
                    _writer = null;
                    _currentLogFile = null;
                    _fileEnabled = false;
                    if (_consoleEnabled)
                    {
                        string exMsg = "[DEBUG] " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss") + " - TSLogger: Exception during log initialization. File logging disabled.";
                        Console.WriteLine(exMsg);
                        Debug.WriteLine(exMsg);
                    }
                }
            }
            private static void EnsureWriterOpen()
            {
                if (_writer != null)
                    return;
                if (_isUnlocked && !string.IsNullOrEmpty(_currentLogFile))
                {
                    if (File.Exists(_currentLogFile))
                    {
                        try
                        {
                            var fs = new FileStream(_currentLogFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                            _hasLogContent = fs.Length > 0;
                            _writer = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                            {
                                AutoFlush = true
                            };
                            return;
                        }
                        catch { }
                    }
                    else
                    {
                        if (_consoleEnabled)
                        {
                            string deleteMsg = $"[DEBUG] TSLogger: Log file was deleted, creating new one.";
                            Console.WriteLine(deleteMsg);
                            Debug.WriteLine(deleteMsg);
                        }
                    }
                }
                InitializeInternal_NoThrow(true);
            }
            private static void CloseWriter_NoThrow()
            {
                try
                {
                    if (_writer != null)
                    {
                        _writer.Flush();
                        _writer.Dispose();
                    }
                }
                catch (ObjectDisposedException) { }
                catch { }
                _writer = null;
            }
            private static void TrimOldLogFiles_NoThrow()
            {
                try
                {
                    FileInfo[] oldFiles = new DirectoryInfo(_logDir).GetFiles("*.log").OrderBy(file => file.CreationTimeUtc).ThenBy(file => file.LastWriteTimeUtc).ToArray();
                    int filesToDelete = oldFiles.Length - MaxLogFileCount + 1;
                    for (int i = 0; i < filesToDelete; i++)
                    {
                        try
                        {
                            oldFiles[i].Delete();
                        }
                        catch { }
                    }
                }
                catch { }
            }
            private static void Write_NoLock(string text)
            {
                if (_isClosed) return;
                if (text == null)
                    text = "";
                string[] lines = SplitLines(text);
                if (lines.Length == 0)
                    return;
                string prefix = $"{DateTime.Now:dd.MM.yyyy - HH:mm:ss} - ";
                if (_consoleEnabled)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string formattedLine = "[DEBUG] " + prefix + lines[i];
                        Console.WriteLine(formattedLine);
                        Debug.WriteLine(formattedLine);
                    }
                }
                if (_fileEnabled)
                {
                    EnsureWriterOpen();
                    if (_writer == null)
                    {
                        if (_consoleEnabled)
                        {
                            string writerFail = "[DEBUG] TSLogger: Failed to open writer.";
                            Console.WriteLine(writerFail);
                            Debug.WriteLine(writerFail);
                        }
                        return;
                    }
                    try
                    {
                        WriteLines_NoTrailingNewline(prefix, lines);
                        if (_isUnlocked)
                        {
                            _writer.Flush();
                            _writer.Dispose();
                            _writer = null;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        _writer = null;
                        EnsureWriterOpen();
                        if (_writer != null)
                        {
                            try
                            {
                                WriteLines_NoTrailingNewline(prefix, lines);
                                if (_isUnlocked)
                                {
                                    _writer.Flush();
                                    _writer.Dispose();
                                    _writer = null;
                                }
                            }
                            catch
                            {
                                _writer = null;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        _writer = null;
                    }
                }
            }
            private static void WriteLines_NoTrailingNewline(string prefix, string[] lines)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (_hasLogContent || i > 0)
                        _writer.WriteLine();
                    _writer.Write(prefix + lines[i]);
                }
                _hasLogContent = true;
            }
            private static string[] SplitLines(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return Array.Empty<string>();
                string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
                string[] parts = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
                int last = parts.Length - 1;
                while (last >= 0 && parts[last].Length == 0)
                    last--;
                if (last < 0)
                    return Array.Empty<string>();
                if (last < parts.Length - 1)
                    Array.Resize(ref parts, last + 1);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Length > 0)
                        return parts;
                }
                return Array.Empty<string>();
            }
        }
        // TS ERROR LOGGER - v2.1
        // ======================================================================================================
        public static class TSErrorLog
        {
            private static readonly object _lock = new object();
            public static void LogException(Exception ex, string context = null)
            {
                if (ex == null)
                    return;
                bool isClosed;
                lock (_lock)
                {
                    isClosed = TSLogger.IsClosed;
                }
                if (isClosed)
                    return;
                string formattedLog = FormatException(ex, context);
                TSLogger.Log(formattedLog);
            }
            private static string FormatException(Exception ex, string context, int depth = 0)
            {
                if (ex == null)
                    return "";
                const int maxDepth = 5;
                const int indentSize = 2;
                if (depth > maxDepth)
                    return $"{new string(' ', depth * indentSize)}[Max depth reached]\n";
                string indent = new string(' ', depth * indentSize);
                string header = new string('-', 50);
                string time = $"{DateTime.Now:dd.MM.yyyy - HH:mm:ss}";
                string type = ex.GetType().FullName;
                string msg = ex.Message ?? "";
                string source = ex.Source ?? "";
                string target = ex.TargetSite?.ToString() ?? "";
                int tid = Thread.CurrentThread.ManagedThreadId;
                string firstFrameInfo = ExtractFirstFrameInfo(ex);
                var sb = new StringBuilder();
                sb.AppendLine(indent + header);
                sb.AppendLine(indent + "EXCEPTION");
                sb.AppendLine(indent + $"Time       : {time}");
                if (!string.IsNullOrWhiteSpace(context))
                    sb.AppendLine(indent + $"Context    : {context}");
                sb.AppendLine(indent + $"Type       : {type}");
                sb.AppendLine(indent + $"Message    : {msg}");
                if (!string.IsNullOrEmpty(source))
                    sb.AppendLine(indent + $"Source     : {source}");
                if (!string.IsNullOrEmpty(target))
                    sb.AppendLine(indent + $"TargetSite : {target}");
                sb.AppendLine(indent + $"ThreadId   : {tid}");
                if (!string.IsNullOrEmpty(firstFrameInfo))
                    sb.Append(indent + firstFrameInfo);
                sb.AppendLine(indent + "StackTrace :");
                sb.AppendLine(indent + (ex.StackTrace ?? ""));
                if (ex is AggregateException aggEx && aggEx.InnerExceptions.Count > 0)
                {
                    sb.AppendLine(indent + $"AggregateException ({aggEx.InnerExceptions.Count} inner exceptions):");
                    for (int i = 0; i < aggEx.InnerExceptions.Count; i++)
                    {
                        sb.AppendLine(indent + $"[{i}]:");
                        sb.Append(FormatException(aggEx.InnerExceptions[i], null, depth + 1));
                    }
                }
                else if (ex.InnerException != null)
                {
                    sb.AppendLine(indent + "InnerException:");
                    sb.Append(FormatException(ex.InnerException, null, depth + 1));
                }
                sb.Append(indent + header);
                return sb.ToString();
            }
            private static string ExtractFirstFrameInfo(Exception ex)
            {
                try
                {
                    var st = new StackTrace(ex, true);
                    var frame = st.FrameCount > 0 ? st.GetFrame(0) : null;
                    if (frame != null)
                    {
                        string file = frame.GetFileName();
                        int line = frame.GetFileLineNumber();
                        if (!string.IsNullOrEmpty(file) && line > 0)
                        {
                            return $"Location   : {file}:{line}\n";
                        }
                    }
                }
                catch { }
                return "";
            }
        }
        // TS SOFTWARE COPYRIGHT DATE
        // ======================================================================================================
        public class TS_SoftwareCopyrightDate
        {
            public static string ts_scd_preloader = string.Format("\u00a9 2019-{0}, {1}.", DateTime.Now.Year, Application.CompanyName);
        }
        // SETTINGS MODULE PATHS
        // ======================================================================================================
        public static readonly string ts_sf = StartupPath + @"\" + Application.ProductName + "Settings.ini";
        public static readonly string ts_settings_container = Path.GetFileNameWithoutExtension(ts_sf);
        // SETTINGS MODULE CLASS
        // ======================================================================================================
        public class TSSettingsModule
        {
            private readonly string _iniFilePath;
            private readonly object _fileLock = new object();
            public TSSettingsModule(string filePath)
            {
                _iniFilePath = filePath;
            }
            // READ SETTINGS
            public string TSReadSettings(string sectionName, string keyName)
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_iniFilePath)) return string.Empty;
                    string[] lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8);
                    bool isInSection = string.IsNullOrEmpty(sectionName);
                    foreach (string rawLine in lines)
                    {
                        string line = rawLine.Trim();
                        if (line.Length == 0 || line.StartsWith(";"))
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            isInSection = line.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);
                            continue;
                        }
                        if (isInSection)
                        {
                            int equalsIndex = line.IndexOf('=');
                            if (equalsIndex > 0)
                            {
                                string currentKey = line.Substring(0, equalsIndex).Trim();
                                if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                                {
                                    return line.Substring(equalsIndex + 1).Trim();
                                }
                            }
                        }
                    }
                    return string.Empty;
                }
            }
            // WRITE/UPDATE SETTINGS
            public void TSWriteSettings(string sectionName, string keyName, string value)
            {
                lock (_fileLock)
                {
                    List<string> lines = File.Exists(_iniFilePath) ? File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList() : new List<string>();
                    bool sectionFound = string.IsNullOrEmpty(sectionName);
                    bool keyUpdated = false;
                    int insertIndex = lines.Count;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string trimmedLine = lines[i].Trim();
                        if (trimmedLine.Length == 0 || trimmedLine.StartsWith(";"))
                            continue;
                        if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                        {
                            if (sectionFound && !keyUpdated)
                            {
                                insertIndex = i;
                                break;
                            }
                            if (!string.IsNullOrEmpty(sectionName))
                                sectionFound = trimmedLine.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);
                            continue;
                        }
                        if (sectionFound)
                        {
                            int equalsIndex = trimmedLine.IndexOf('=');
                            if (equalsIndex > 0)
                            {
                                string currentKey = trimmedLine.Substring(0, equalsIndex).Trim();
                                if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                                {
                                    lines[i] = keyName + "=" + value;
                                    keyUpdated = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!sectionFound && !string.IsNullOrEmpty(sectionName))
                    {
                        if (lines.Count > 0) lines.Add("");
                        lines.Add("[" + sectionName + "]");
                        lines.Add(keyName + "=" + value);
                    }
                    else if (!keyUpdated)
                    {
                        insertIndex = (insertIndex == lines.Count) ? lines.Count : insertIndex;
                        lines.Insert(insertIndex, keyName + "=" + value);
                    }
                    try
                    {
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                    }
                    catch (IOException) { }
                }
            }
            // DELETE SETTINSG
            public void TSDeleteSetting(string sectionName, string keyName)
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_iniFilePath)) return;
                    List<string> lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    bool isInSection = string.IsNullOrEmpty(sectionName);
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string line = lines[i].Trim();
                        if (line.Length == 0 || line.StartsWith(";"))
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            if (!string.IsNullOrEmpty(sectionName))
                                isInSection = line.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);

                            continue;
                        }
                        if (isInSection)
                        {
                            int eqIndex = line.IndexOf('=');
                            if (eqIndex > 0)
                            {
                                string currentKey = line.Substring(0, eqIndex).Trim();
                                if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                                {
                                    lines.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                    try
                    {
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                    }
                    catch (IOException) { }
                }
            }
            // RENAME KEY
            public bool TSRenameKey(string sectionName, string oldKey, string newKey)
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_iniFilePath)) return false;
                    List<string> lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    bool isInSection = string.IsNullOrEmpty(sectionName);
                    foreach (var raw in lines)
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith(";"))
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            if (!string.IsNullOrEmpty(sectionName))
                                isInSection = line.Equals($"[{sectionName}]", StringComparison.OrdinalIgnoreCase);
                            else
                                isInSection = true;
                            continue;
                        }
                        if (isInSection)
                        {
                            int eq = line.IndexOf('=');
                            if (eq > 0)
                            {
                                var key = line.Substring(0, eq).Trim();
                                if (key.Equals(newKey, StringComparison.OrdinalIgnoreCase))
                                    return false;
                            }
                        }
                    }
                    isInSection = string.IsNullOrEmpty(sectionName);
                    for (int i = 0; i < lines.Count; i++)
                    {
                        string trimmed = lines[i].Trim();
                        if (trimmed.Length == 0 || trimmed.StartsWith(";"))
                            continue;
                        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                        {
                            if (!string.IsNullOrEmpty(sectionName))
                                isInSection = trimmed.Equals($"[{sectionName}]", StringComparison.OrdinalIgnoreCase);
                            else
                                isInSection = true;
                            continue;
                        }
                        if (isInSection)
                        {
                            int eqIndex = trimmed.IndexOf('=');
                            if (eqIndex > 0)
                            {
                                string currentKey = trimmed.Substring(0, eqIndex).Trim();
                                if (currentKey.Equals(oldKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    string value = trimmed.Substring(eqIndex + 1).Trim();
                                    lines[i] = $"{newKey}={value}";
                                    try
                                    {
                                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                                        return true;
                                    }
                                    catch (IOException)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            // RENAME SECTION
            public bool TSRenameSection(string oldSectionName, string newSectionName)
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_iniFilePath)) return false;
                    var lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    foreach (var raw in lines)
                    {
                        var t = raw.Trim();
                        if (t.StartsWith("[") && t.EndsWith("]"))
                        {
                            var sec = t.Substring(1, t.Length - 2).Trim();
                            if (sec.Equals(newSectionName, StringComparison.OrdinalIgnoreCase))
                                return false;
                        }
                    }
                    bool oldFound = false;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var trimmed = lines[i].Trim();
                        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                        {
                            var sec = trimmed.Substring(1, trimmed.Length - 2).Trim();
                            if (sec.Equals(oldSectionName, StringComparison.OrdinalIgnoreCase))
                            {
                                lines[i] = $"[{newSectionName}]";
                                oldFound = true;
                                break;
                            }
                        }
                    }
                    if (!oldFound) return false;
                    try
                    {
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                        return true;
                    }
                    catch (IOException)
                    {
                        return false;
                    }
                }
            }
            // ORDER / NORMALIZE KEYS IN A SECTION
            public bool TSOrderSectionKeys(string sectionName, IEnumerable<string> orderedKeys)
            {
                if (orderedKeys == null) return false;
                lock (_fileLock)
                {
                    if (!File.Exists(_iniFilePath)) return false;
                    var lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    int sectionHeaderIndex = -1;
                    int sectionEndIndex = lines.Count;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var t = lines[i].Trim();
                        if (t.StartsWith("[") && t.EndsWith("]"))
                        {
                            var sec = t.Substring(1, t.Length - 2).Trim();
                            if (sec.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                            {
                                sectionHeaderIndex = i;
                                for (int j = i + 1; j < lines.Count; j++)
                                {
                                    var tj = lines[j].Trim();
                                    if (tj.StartsWith("[") && tj.EndsWith("]"))
                                    {
                                        sectionEndIndex = j;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (sectionHeaderIndex < 0) return false;
                    var currentOrder = new List<string>();
                    var seenOrderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var kv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = sectionHeaderIndex + 1; i < sectionEndIndex; i++)
                    {
                        var raw = lines[i];
                        var t = raw.Trim();
                        if (t.Length == 0 || t.StartsWith(";")) continue;
                        int eq = t.IndexOf('=');
                        if (eq > 0)
                        {
                            var key = t.Substring(0, eq).Trim();
                            var val = t.Substring(eq + 1).Trim();
                            if (seenOrderKeys.Add(key))
                                currentOrder.Add(key);
                            kv[key] = val;
                        }
                    }
                    var orderedList = orderedKeys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()).ToList();
                    var orderedSet = new HashSet<string>(orderedList, StringComparer.OrdinalIgnoreCase);
                    var desiredOrder = new List<string>();
                    foreach (var key in orderedList)
                    {
                        if (kv.ContainsKey(key))
                            desiredOrder.Add(key);
                    }
                    foreach (var key in currentOrder)
                    {
                        if (!orderedSet.Contains(key))
                            desiredOrder.Add(key);
                    }
                    if (currentOrder.SequenceEqual(desiredOrder, StringComparer.OrdinalIgnoreCase))
                        return true;
                    var rebuilt = new List<string> { lines[sectionHeaderIndex].Trim() };
                    foreach (var key in orderedList)
                    {
                        if (kv.TryGetValue(key, out var val))
                            rebuilt.Add($"{key}={val}");
                    }
                    foreach (var key in currentOrder)
                    {
                        if (!orderedSet.Contains(key) && kv.TryGetValue(key, out var val))
                            rebuilt.Add($"{key}={val}");
                    }
                    lines.RemoveRange(sectionHeaderIndex, sectionEndIndex - sectionHeaderIndex);
                    lines.InsertRange(sectionHeaderIndex, rebuilt);
                    try
                    {
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                        return true;
                    }
                    catch (IOException)
                    {
                        return false;
                    }
                }
            }
        }
        // READ LANG PATHS
        // ======================================================================================================
        public static readonly string ts_lf = Path.Combine(StartupPath, "g_langs");     // Main Path
        public static readonly string ts_lang_ar = ts_lf + @"\Arabic.ini";              // Arabic       | ar
        public static readonly string ts_lang_zh = ts_lf + @"\Chinese.ini";             // Chinese      | zh
        public static readonly string ts_lang_zh_tw = ts_lf + @"\Chinese_TW.ini";       // Chinese TW   | zh-tw
        public static readonly string ts_lang_en = ts_lf + @"\English.ini";             // English      | en
        public static readonly string ts_lang_nl = ts_lf + @"\Dutch.ini";               // Nederlands   | nl
        public static readonly string ts_lang_fr = ts_lf + @"\French.ini";              // French       | fr
        public static readonly string ts_lang_de = ts_lf + @"\German.ini";              // German       | de
        public static readonly string ts_lang_hi = ts_lf + @"\Hindi.ini";               // Hindi        | hi
        public static readonly string ts_lang_it = ts_lf + @"\Italian.ini";             // Italian      | it
        public static readonly string ts_lang_ja = ts_lf + @"\Japanese.ini";            // Japanese     | ja
        public static readonly string ts_lang_ko = ts_lf + @"\Korean.ini";              // Korean       | ko
        public static readonly string ts_lang_pl = ts_lf + @"\Polish.ini";              // Polish       | pl
        public static readonly string ts_lang_pt = ts_lf + @"\Portuguese.ini";          // Portuguese   | pt
        public static readonly string ts_lang_ru = ts_lf + @"\Russian.ini";             // Russian      | ru
        public static readonly string ts_lang_sl = ts_lf + @"\Slovenian.ini";           // Slovenian    | sl
        public static readonly string ts_lang_es = ts_lf + @"\Spanish.ini";             // Spanish      | es
        public static readonly string ts_lang_tr = ts_lf + @"\Turkish.ini";             // Turkish      | tr
        // LANGUAGE MANAGE FUNCTIONS
        // ======================================================================================================
        public static readonly Dictionary<string, string> AllLanguageFiles = new Dictionary<string, string> {
            { "ar", ts_lang_ar },
            { "zh", ts_lang_zh },
            { "zh-tw", ts_lang_zh_tw },
            { "en", ts_lang_en },
            { "nl", ts_lang_nl },
            { "fr", ts_lang_fr },
            { "de", ts_lang_de },
            { "hi", ts_lang_hi },
            { "it", ts_lang_it },
            { "ja", ts_lang_ja },
            { "ko", ts_lang_ko },
            { "pl", ts_lang_pl },
            { "pt", ts_lang_pt },
            { "ru", ts_lang_ru },
            { "sl", ts_lang_sl },
            { "es", ts_lang_es },
            { "tr", ts_lang_tr },
        };
        public static string TSPreloaderSetDefaultLanguage(string ui_lang)
        {
            bool anyLanguageFileExists = AllLanguageFiles.Values.Any(File.Exists);
            if (!anyLanguageFileExists) return "en";
            if (!string.IsNullOrEmpty(ui_lang))
            {
                if (AllLanguageFiles.ContainsKey(ui_lang) && File.Exists(AllLanguageFiles[ui_lang]))
                {
                    return ui_lang;
                }
                if (ui_lang.Length >= 2)
                {
                    string twoLetter = ui_lang.Substring(0, 2);
                    if (AllLanguageFiles.ContainsKey(twoLetter) && File.Exists(AllLanguageFiles[twoLetter]))
                    {
                        return twoLetter;
                    }
                }
            }
            return "en";
        }
        public static List<string> AvailableLanguages = AllLanguageFiles.Values.Where(filePath => File.Exists(filePath)).ToList();
        // READ LANG CLASS
        // ======================================================================================================
        public class TSGetLangs
        {
            private readonly string _iniFilePath;
            private readonly string _fallbackIniFilePath = ts_lang_en;
            private readonly object _cacheLock = new object();
            private string[] _cachedLines = null;
            private DateTime _lastFileWriteTime = DateTime.MinValue;
            private string[] _cachedFallbackLines = null;
            private DateTime _lastFallbackWriteTime = DateTime.MinValue;
            public TSGetLangs(string iniFilePath)
            {
                _iniFilePath = iniFilePath;
            }
            public string TSReadLangs(string sectionName, string keyName)
            {
                string value = FindLangsValue(GetIniLinesCached(_iniFilePath, ref _cachedLines, ref _lastFileWriteTime), sectionName, keyName);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
                value = FindLangsValue(GetIniLinesCached(_fallbackIniFilePath, ref _cachedFallbackLines, ref _lastFallbackWriteTime), sectionName, keyName);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return "N/A Langs";
            }
            private string FindLangsValue(string[] iniLines, string sectionName, string keyName)
            {
                bool isInSection = string.IsNullOrEmpty(sectionName);
                foreach (string rawLine in iniLines)
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith(";"))
                        continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        isInSection = line.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (!isInSection)
                        continue;
                    int eqIndex = line.IndexOf('=');
                    if (eqIndex <= 0)
                        continue;
                    string currentKey = line.Substring(0, eqIndex).Trim();
                    if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                        return line.Substring(eqIndex + 1).Trim();
                }
                return string.Empty;
            }
            private string[] GetIniLinesCached(string path, ref string[] cache, ref DateTime lastWriteTimeUtc)
            {
                lock (_cacheLock)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        {
                            return Array.Empty<string>();
                        }
                        DateTime currentWriteTime = File.GetLastWriteTimeUtc(path);
                        if (cache == null || currentWriteTime != lastWriteTimeUtc)
                        {
                            cache = File.ReadAllLines(path, Encoding.UTF8);
                            lastWriteTimeUtc = currentWriteTime;
                        }
                        return cache;
                    }
                    catch (IOException)
                    {
                        return Array.Empty<string>();
                    }
                }
            }
        }
        // TS COLOR MATH (shared helpers)
        // ======================================================================================================
        internal static class TSColorMath
        {
            public static double ColorDistance(Color a, Color b)
            {
                int dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
                return Math.Sqrt(dr * dr + dg * dg + db * db);
            }
            // WCAG relative luminance, 0 (black) .. 1 (white).
            public static double RelativeLuminance(Color c)
            {
                double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
                r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
                g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
                b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);
                return 0.2126 * r + 0.7152 * g + 0.0722 * b;
            }
        }
        // TS THEME ENGINE
        // ======================================================================================================
        public class TS_ThemeEngine
        {
            // LIGHT THEME COLORS
            // ====================================
            public static readonly Dictionary<string, string> LightTheme = new Dictionary<string, string>{
                // BG & PANELS
                { "TSBT_BGColor", "#ECF2F8" },
                { "TSBT_BGColor2", "#FFFFFF" },
                // ACCENT COLORS
                { "TSBT_AccentColor", "#365F92" },
                { "AccentColorHover", "#3F6DA5" },
                { "AccentPurple", "#7655B1" },
                { "AccentRed", "#CF1800" },
                { "AccentGreen", "#1C7A19" },
                { "AccentOrange", "#9B5500" },
                // FOREGROUND / TEXT
                { "TSBT_LabelColor1", "#16191d" },
                { "TSBT_LabelColor2", "#646464" },
                // BORDERS & GRIDS
                { "SelectBoxBorderColor", "#d1d7df" },
                { "CheckBoxUnCheckBorderColor", "#394149" }
            };
            // DARK THEME COLORS
            // ====================================
            public static readonly Dictionary<string, string> DarkTheme = new Dictionary<string, string>{
                // BG & PANELS
                { "TSBT_BGColor", "#0d0f12" },
                { "TSBT_BGColor2", "#16191d" },
                // ACCENT COLORS
                { "TSBT_AccentColor", "#5899E9" },
                { "AccentColorHover", "#5DA5FD" },
                { "AccentPurple", "#A476F3" },
                { "AccentRed", "#FF4D4D" },
                { "AccentGreen", "#26BB21" },
                { "AccentOrange", "#F58806" },
                // FOREGROUND / TEXT
                { "TSBT_LabelColor1", "#F5F5F5" },
                { "TSBT_LabelColor2", "#c3cbd6" },
                // BORDERS & GRIDS
                { "SelectBoxBorderColor", "#293036" },
                { "CheckBoxUnCheckBorderColor", "#394149" }
            };
            // HEX TO ARGB
            // ====================================
            public static Color HexToARGB(string hex)
            {
                if (string.IsNullOrWhiteSpace(hex))
                {
                    Debug.WriteLine(new ArgumentException("The hex code cannot be empty or null.", nameof(hex)));
                }
                //
                string s = hex.Trim().TrimStart('#');
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) s = s.Substring(2);
                //
                if (s.Length == 3) s = $"FF{s[0]}{s[0]}{s[1]}{s[1]}{s[2]}{s[2]}";
                else if (s.Length == 4) s = $"{s[0]}{s[0]}{s[1]}{s[1]}{s[2]}{s[2]}{s[3]}{s[3]}";
                else if (s.Length == 6) s = "FF" + s;
                else if (s.Length != 8) Debug.WriteLine(new FormatException($"Invalid Hex format: '{hex}'"));
                //
                return Color.FromArgb((int)Convert.ToUInt32(s, 16));
            }
            // System accent override, active only in system theme mode.
            // Covers TSBT_AccentColor + AccentColorHover; text colors stay fixed.
            // ====================================
            private static readonly object _accentLock = new object();
            private static bool _useSystemAccent = false;
            private static Color _sysAccentDark = Color.Empty;
            private static Color _sysAccentDarkHover = Color.Empty;
            private static Color _sysAccentLight = Color.Empty;
            private static Color _sysAccentLightHover = Color.Empty;
            public static bool UseSystemAccent
            {
                get { lock (_accentLock) { return _useSystemAccent; } }
            }
            private static int _lastAccentArgb = int.MinValue;
            public static int LastSystemAccentArgb
            {
                get { lock (_accentLock) { return _lastAccentArgb; } }
            }
            private static bool _lastResolveFallback = false;
            public static bool LastResolveWasFallback
            {
                get { lock (_accentLock) { return _lastResolveFallback; } }
            }
            public static void SetSystemAccentEnabled(bool enabled)
            {
                if (enabled) RefreshSystemAccent();
                else ClearSystemAccent();
            }
            public static void RefreshSystemAccent()
            {
                try
                {
                    Color baseAccent = TSAccentHelper.GetSystemAccentBase();
                    bool exact = TSAccentHelper.ResolveAccents(baseAccent, out Color dark, out Color darkHover, out Color light, out Color lightHover);
                    lock (_accentLock)
                    {
                        _sysAccentDark = dark;
                        _sysAccentDarkHover = darkHover;
                        _sysAccentLight = light;
                        _sysAccentLightHover = lightHover;
                        _useSystemAccent = true;
                        _lastAccentArgb = baseAccent.ToArgb();
                        _lastResolveFallback = !exact;
                    }
                }
                catch
                {
                    lock (_accentLock) { _useSystemAccent = false; }
                }
            }
            public static void ClearSystemAccent()
            {
                lock (_accentLock) { _useSystemAccent = false; }
            }
            // THEME SWITCHER
            // ====================================
            public static Color ColorMode(int theme, string key)
            {
                if (key == "TSBT_AccentColor" || key == "AccentColorHover")
                {
                    lock (_accentLock)
                    {
                        if (_useSystemAccent)
                        {
                            if (theme == 0 && _sysAccentDark.A != 0)
                                return key == "TSBT_AccentColor" ? _sysAccentDark : _sysAccentDarkHover;
                            if (theme == 1 && _sysAccentLight.A != 0)
                                return key == "TSBT_AccentColor" ? _sysAccentLight : _sysAccentLightHover;
                        }
                    }
                }
                if (theme == 0)
                {
                    return DarkTheme.ContainsKey(key) ? HexToARGB(DarkTheme[key]) : Color.Black;
                }
                else if (theme == 1)
                {
                    return LightTheme.ContainsKey(key) ? HexToARGB(LightTheme[key]) : Color.White;
                }
                return Color.White;
            }
            // SYSTEM ACCENT HELPER (uxtheme immersive + DWM fallback)
            // Base + exact Light/Dark shades come from Windows; only a tiny
            // RGB-mix fallback remains for old systems. No registry, no HSL.
            // ====================================
            public static class TSAccentHelper
            {
                [DllImport("dwmapi.dll")]
                private static extern int DwmGetColorizationColor(out uint pcrColorization, out bool pfOpaqueBlend);
                [DllImport("uxtheme.dll", EntryPoint = "#95")]
                private static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
                [DllImport("uxtheme.dll", EntryPoint = "#96")]
                private static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
                [DllImport("uxtheme.dll", EntryPoint = "#98")]
                private static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);
                private const double HoverWhiteMix = 0.12;
                private const double DarkFloorLuminance = 0.15;
                private const double LightCeilLuminance = 0.30;
                private static readonly object _immersiveLock = new object();
                private static readonly Dictionary<string, uint> _immersiveTypeCache = new Dictionary<string, uint>(StringComparer.Ordinal);
                // Type ID is stable per name -> cache it. Color value is always
                // queried fresh, otherwise accent change with same ColorSet returns stale color.
                private static uint GetImmersiveType(string name)
                {
                    lock (_immersiveLock)
                    {
                        if (_immersiveTypeCache.TryGetValue(name, out uint hit)) return hit;
                    }
                    IntPtr pName = IntPtr.Zero;
                    try
                    {
                        pName = Marshal.StringToHGlobalUni(name);
                        uint type = GetImmersiveColorTypeFromName(pName);
                        if (type != uint.MaxValue)
                        {
                            lock (_immersiveLock) { _immersiveTypeCache[name] = type; }
                        }
                        return type;
                    }
                    catch { return uint.MaxValue; }
                    finally { if (pName != IntPtr.Zero) Marshal.FreeHGlobal(pName); }
                }
                // Single immersive query, leak-free. Returns Empty on failure;
                // valid black (255,0,0,0) is preserved.
                private static Color GetImmersiveColor(string name)
                {
                    try
                    {
                        uint type = GetImmersiveType(name);
                        if (type == uint.MaxValue) return Color.Empty;
                        int set = GetImmersiveUserColorSetPreference(false, false);
                        uint raw = GetImmersiveColorFromColorSetEx((uint)set, type, false, 0);
                        if (raw == 0) return Color.Empty;
                        return Color.FromArgb(255, (byte)(raw & 0xFF), (byte)((raw >> 8) & 0xFF), (byte)((raw >> 16) & 0xFF));
                    }
                    catch { return Color.Empty; }
                }
                // BASE ACCENT FROM SYSTEM
                // ====================================
                public static Color GetSystemAccentBase()
                {
                    Color c = GetImmersiveColor("ImmersiveSystemAccent");
                    if (c.A != 0) return c;
                    c = GetImmersiveColor("ImmersiveStartSelectionBackground");
                    if (c.A != 0) return c;
                    try
                    {
                        if (DwmGetColorizationColor(out uint raw, out _) == 0 && raw != 0)
                        {
                            Color d = Color.FromArgb((int)raw);
                            return Color.FromArgb(255, d.R, d.G, d.B);
                        }
                    }
                    catch { }
                    return Color.FromArgb(0, 120, 212);
                }
                private static Color MixWithWhite(Color c, double amount)
                {
                    if (amount < 0) amount = 0;
                    if (amount > 1) amount = 1;
                    return Color.FromArgb(255,
                        (int)Math.Round(c.R * (1.0 - amount) + 255.0 * amount),
                        (int)Math.Round(c.G * (1.0 - amount) + 255.0 * amount),
                        (int)Math.Round(c.B * (1.0 - amount) + 255.0 * amount));
                }
                private static Color MixWithBlack(Color c, double amount)
                {
                    if (amount < 0) amount = 0;
                    if (amount > 1) amount = 1;
                    return Color.FromArgb(255,
                        (int)Math.Round(c.R * (1.0 - amount)),
                        (int)Math.Round(c.G * (1.0 - amount)),
                        (int)Math.Round(c.B * (1.0 - amount)));
                }
                private static Color Opaque(Color c)
                {
                    return Color.FromArgb(255, c.R, c.G, c.B);
                }
                // Resolves dark / darkHover / light / lightHover.
                // Returns true when exact Windows shades were used (false = fallback).
                public static bool ResolveAccents(Color baseColor, out Color dark, out Color darkHover, out Color light, out Color lightHover)
                {
                    Color normal = GetImmersiveColor("ImmersiveSystemAccent");
                    Color light1 = GetImmersiveColor("ImmersiveSystemAccentLight1");
                    Color light2 = GetImmersiveColor("ImmersiveSystemAccentLight2");
                    Color light3 = GetImmersiveColor("ImmersiveSystemAccentLight3");
                    Color dark1 = GetImmersiveColor("ImmersiveSystemAccentDark1");
                    if (normal.A != 0 && light2.A != 0)
                    {
                        dark = light2;
                        if (TSColorMath.RelativeLuminance(dark) < DarkFloorLuminance && light3.A != 0)
                            dark = light3;
                        darkHover = MixWithWhite(dark, HoverWhiteMix);
                        light = normal;
                        if (TSColorMath.RelativeLuminance(light) > LightCeilLuminance && dark1.A != 0)
                        {
                            light = dark1;
                            lightHover = normal;
                        }
                        else
                        {
                            lightHover = light1.A != 0 ? light1 : MixWithWhite(light, HoverWhiteMix);
                        }
                        return true;
                    }
                    // Fallback (old Windows / API failed): simple RGB mix, no HSL.
                    Color b = baseColor.A != 0 ? Opaque(baseColor) : (normal.A != 0 ? normal : Color.FromArgb(0, 120, 212));
                    dark = MixWithWhite(b, 0.35);
                    light = TSColorMath.RelativeLuminance(b) > 0.65 ? MixWithBlack(b, 0.25) : b;
                    darkHover = MixWithWhite(dark, HoverWhiteMix);
                    lightHover = MixWithWhite(light, HoverWhiteMix);
                    return false;
                }
            }
        }
        // THEME MODE HELPER
        // ======================================================================================================
        public static class TSThemeModeHelper
        {
            private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
            [DllImport("user32.dll")]
            private static extern bool GetComboBoxInfo(IntPtr hWnd, ref COMBOBOXINFO pcbi);
            [StructLayout(LayoutKind.Sequential)]
            private struct COMBOBOXINFO
            {
                public int cbSize;
                public RECT rcItem;
                public RECT rcButton;
                public int buttonState;
                public IntPtr hwndCombo;
                public IntPtr hwndEdit;
                public IntPtr hwndList;
            }
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
            private static bool _isDarkModeEnabled = false;
            public static bool IsDarkModeEnabled => _isDarkModeEnabled;
            public static void SetThemeMode(bool enableTMode)
            {
                _isDarkModeEnabled = enableTMode;
            }
            public static void InitializeThemeForForm(Form targetForm)
            {
                if (targetForm == null || targetForm.IsDisposed)
                    return;
                ApplyThemeModeToForm(targetForm, _isDarkModeEnabled);
            }
            private static void ApplyThemeModeToForm(Form targetForm, bool enableRequire)
            {
                if (targetForm == null || targetForm.IsDisposed)
                    return;
                int useDark = enableRequire ? 1 : 0;
                DwmSetWindowAttribute(targetForm.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                string themeName = enableRequire ? "DarkMode_Explorer" : "Explorer";
                ApplyScrollTheme(targetForm, themeName);
            }
            private static void ApplyScrollTheme(Control parentControl, string targetTheme)
            {
                if (parentControl == null || parentControl.IsDisposed)
                    return;
                if (parentControl is ComboBox comboBox)
                {
                    ApplyThemeToComboBoxDropDown(comboBox);
                }
                if (parentControl is DataGridView || parentControl is ComboBox || parentControl is ListBox || parentControl is ListView || parentControl is TreeView || parentControl is RichTextBox || parentControl is Panel || parentControl is Form || (parentControl is TextBox tb && tb.Multiline))
                {
                    if (parentControl.Tag as string != targetTheme)
                    {
                        SetWindowTheme(parentControl.Handle, targetTheme, null);
                        if (parentControl is DataGridView dgv)
                        {
                            foreach (Control c in dgv.Controls)
                            {
                                if (c is ScrollBar)
                                    SetWindowTheme(c.Handle, targetTheme, null);
                            }
                        }
                        parentControl.Tag = targetTheme;
                    }
                }
                if (parentControl is Form || parentControl is TabControl || parentControl is TabPage || parentControl is Panel || parentControl is GroupBox || parentControl is UserControl)
                {
                    if (parentControl.HasChildren)
                    {
                        foreach (Control childControl in parentControl.Controls)
                        {
                            ApplyScrollTheme(childControl, targetTheme);
                        }
                    }
                }
            }
            public static void ApplyThemeToComboBoxDropDown(ComboBox comboBox)
            {
                if (comboBox == null || comboBox.IsDisposed || comboBox.Disposing || !comboBox.IsHandleCreated)
                    return;
                try
                {
                    bool dark = _isDarkModeEnabled;
                    string theme = dark ? "DarkMode_Explorer" : "Explorer";
                    try { SetWindowTheme(comboBox.Handle, theme, null); } catch { }
                    try
                    {
                        COMBOBOXINFO cbi = new COMBOBOXINFO
                        {
                            cbSize = Marshal.SizeOf(typeof(COMBOBOXINFO))
                        };
                        if (GetComboBoxInfo(comboBox.Handle, ref cbi) && cbi.hwndList != IntPtr.Zero)
                        {
                            SetWindowTheme(cbi.hwndList, theme, null);
                            int useDark = dark ? 1 : 0;
                            DwmSetWindowAttribute(cbi.hwndList, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                        }
                    }
                    catch { }
                }
                catch { }
            }
            public static int GetSystemTheme(int theme_mode)
            {
                if (theme_mode == 2)
                {
                    using (var getSystemThemeKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    {
                        theme_mode = (int)(getSystemThemeKey?.GetValue("SystemUsesLightTheme") ?? 1);
                    }
                }
                return theme_mode;
            }
        }
        // TS ICON ACCENT TINT ENGINE (ported from Walpa WalpaImageModule.TS_CI_Engine)
        // Tints only native-accent icons; white/black/green/red pass through untouched.
        // ======================================================================================================
        public static class TSIconTintEngine
        {
            // Tinted copy of renderedImage, or null. Caller owns the copy; render cache untouched.
            public static Image TintForAccent(Image renderedImage, Image sourceImage)
            {
                try
                {
                    if (!TS_ThemeEngine.UseSystemAccent) return null;
                    if (renderedImage == null || sourceImage == null) return null;
                    if (!IsAccentIcon(sourceImage)) return null;
                    Color accent = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                    if (accent.A == 0) return null;
                    return TS_CI_Engine(renderedImage, accent);
                }
                catch { return null; }
            }
            // Walpa TS_CI_Engine port: recolors opaque pixels, preserves alpha.
            public static unsafe Bitmap TS_CI_Engine(Image sourceImage, Color targetColor)
            {
                if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));
                int width = sourceImage.Width;
                int height = sourceImage.Height;
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(sourceImage, 0, 0, width, height);
                }
                byte targetB = targetColor.B;
                byte targetG = targetColor.G;
                byte targetR = targetColor.R;
                const int bytesPerPixel = 4;
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                try
                {
                    int stride = bmpData.Stride;
                    byte* scan0 = (byte*)bmpData.Scan0.ToPointer();
                    Parallel.For(0, height, y =>
                    {
                        byte* pixel = scan0 + (y * stride);
                        for (int x = 0; x < width; x++)
                        {
                            if (pixel[3] > 0)
                            {
                                pixel[0] = targetB;
                                pixel[1] = targetG;
                                pixel[2] = targetR;
                            }
                            pixel += bytesPerPixel;
                        }
                    });
                }
                finally { bmp.UnlockBits(bmpData); }
                return bmp;
            }
            // True when the icon's dominant opaque color is a native theme accent.
            public static unsafe bool IsAccentIcon(Image sourceImage)
            {
                if (sourceImage == null) return false;
                Bitmap bmp = sourceImage as Bitmap;
                bool ownCopy = false;
                try
                {
                    try
                    {
                        if (bmp == null) { bmp = new Bitmap(sourceImage); ownCopy = true; }
                    }
                    catch { return false; }
                    int width = bmp.Width, height = bmp.Height;
                    if (width <= 0 || height <= 0) return false;
                    BitmapData bmpData = null;
                    try { bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); }
                    catch { return false; }
                    try
                    {
                        long n = 0, sr = 0, sg = 0, sb = 0;
                        int stride = bmpData.Stride;
                        byte* scan0 = (byte*)bmpData.Scan0.ToPointer();
                        for (int y = 0; y < height; y++)
                        {
                            byte* pixel = scan0 + (y * stride);
                            for (int x = 0; x < width; x++)
                            {
                                if (pixel[3] > 16)
                                {
                                    n++;
                                    sr += pixel[2];
                                    sg += pixel[1];
                                    sb += pixel[0];
                                }
                                pixel += 4;
                            }
                        }
                        if (n < 8) return false;
                        Color avg = Color.FromArgb((int)(sr / n), (int)(sg / n), (int)(sb / n));
                        return IsNativeAccent(avg);
                    }
                    finally { try { bmp.UnlockBits(bmpData); } catch { } }
                }
                catch { return false; }
                finally { if (ownCopy && bmp != null) { try { bmp.Dispose(); } catch { } } }
            }
            private static Color _nativeDarkAccent = Color.Empty;
            private static Color _nativeLightAccent = Color.Empty;
            private static bool _nativeAccentsLoaded = false;
            private static readonly object _nativeAccentLock = new object();
            private static void EnsureNativeAccents()
            {
                if (_nativeAccentsLoaded) return;
                lock (_nativeAccentLock)
                {
                    if (_nativeAccentsLoaded) return;
                    try
                    {
                        if (TS_ThemeEngine.DarkTheme.TryGetValue("TSBT_AccentColor", out string darkHex))
                            _nativeDarkAccent = TS_ThemeEngine.HexToARGB(darkHex);
                        if (TS_ThemeEngine.LightTheme.TryGetValue("TSBT_AccentColor", out string lightHex))
                            _nativeLightAccent = TS_ThemeEngine.HexToARGB(lightHex);
                    }
                    catch { }
                    _nativeAccentsLoaded = true;
                }
            }
            private static bool IsNativeAccent(Color c)
            {
                EnsureNativeAccents();
                if (_nativeDarkAccent.A != 0 && TSColorMath.ColorDistance(c, _nativeDarkAccent) <= 32.0) return true;
                if (_nativeLightAccent.A != 0 && TSColorMath.ColorDistance(c, _nativeLightAccent) <= 32.0) return true;
                return false;
            }
        }
        // DPI SENSITIVE DYNAMIC IMAGE RENDERER
        // ======================================================================================================
        private static readonly object _lock_icon = new object();
        private static readonly Dictionary<string, Image> _cache_icon = new Dictionary<string, Image>();
        private static readonly Queue<string> _cacheIconOrder = new Queue<string>();
        private const int MaxIconCacheEntries = 256;
        public static void TSImageRenderer(object baseTarget, Image sourceImage, int basePadding, ContentAlignment imageAlign = ContentAlignment.MiddleCenter)
        {
            if (baseTarget == null || sourceImage == null) return;
            const int minImageSize = 16;
            Image ResizeImage(Image targetImg, int targetSize)
            {
                var bmp = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(targetImg, 0, 0, targetSize, targetSize);
                }
                return bmp;
            }
            Image newImage = null;
            Image oldImage = null;
            try
            {
                int size;
                float dpi;
                if (baseTarget is Control ctrl)
                {
                    dpi = TSDpiHelper.EffectiveDpi(ctrl);
                    int padding = (int)Math.Round(basePadding * TSDpiHelper.Scale(dpi));
                    size = ctrl.Height - padding;
                    if (size <= 0) size = minImageSize;
                    string key = $"{RuntimeHelpers.GetHashCode(sourceImage)}_{sourceImage.Width}x{sourceImage.Height}_C_{size}_{dpi}";
                    newImage = GetOrCreate(key, () => ResizeImage(sourceImage, size));
                    try
                    {
                        Image tinted = TSIconTintEngine.TintForAccent(newImage, sourceImage);
                        if (tinted != null) newImage = tinted;
                    }
                    catch { }
                    if (ctrl is Button btn)
                    {
                        oldImage = btn.Image;
                        btn.Image = newImage;
                        btn.ImageAlign = imageAlign;
                    }
                    else if (ctrl is PictureBox pb)
                    {
                        oldImage = pb.Image;
                        pb.Image = newImage;
                        pb.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        newImage.Dispose();
                        newImage = null;
                    }
                }
                else if (baseTarget is ToolStripItem item)
                {
                    int itemDpi = TSDpiHelper.BaseDpi;
                    try
                    {
                        var host = item.GetCurrentParent();
                        if (host != null) itemDpi = TSDpiHelper.EffectiveDpi(host);
                        if (itemDpi <= TSDpiHelper.BaseDpi)
                        {
                            try
                            {
                                var ownerStrip = item.Owner;
                                if (ownerStrip != null)
                                {
                                    int od = TSDpiHelper.EffectiveDpi(ownerStrip);
                                    if (od > itemDpi) itemDpi = od;
                                }
                            }
                            catch { }
                        }
                        if (itemDpi <= TSDpiHelper.BaseDpi)
                        {
                            try
                            {
                                foreach (Form f in Application.OpenForms)
                                {
                                    try { if (f != null && f.IsHandleCreated && f.DeviceDpi > itemDpi) itemDpi = f.DeviceDpi; } catch { }
                                }
                            }
                            catch { }
                        }
                        if (itemDpi <= TSDpiHelper.BaseDpi)
                        {
                            try { using (var sg = Graphics.FromHwnd(IntPtr.Zero)) { int sd = (int)sg.DpiX; if (sd > itemDpi) itemDpi = sd; } } catch { }
                        }
                    }
                    catch { itemDpi = TSDpiHelper.BaseDpi; }
                    dpi = itemDpi;
                    size = (int)Math.Round((TSDpiHelper.MenuBaseIconPx - basePadding) * TSDpiHelper.Scale(dpi));
                    if (size <= 0) size = minImageSize;
                    string key = $"{RuntimeHelpers.GetHashCode(sourceImage)}_{sourceImage.Width}x{sourceImage.Height}_T_{size}_{dpi}";
                    newImage = GetOrCreate(key, () => ResizeImage(sourceImage, size));
                    try
                    {
                        Image tinted = TSIconTintEngine.TintForAccent(newImage, sourceImage);
                        if (tinted != null) newImage = tinted;
                    }
                    catch { }
                    try { if (newImage is Bitmap nbmp) nbmp.SetResolution(dpi, dpi); } catch { }
                    oldImage = item.Image;
                    item.Image = newImage;
                }
                else
                {
                    return;
                }
                if (oldImage != null && !ReferenceEquals(oldImage, sourceImage) && !IsSharedIcon(oldImage))
                {
                    oldImage.Dispose();
                }
            }
            catch
            {
                newImage?.Dispose();
            }
        }
        private static readonly HashSet<Image> _sharedImages = new HashSet<Image>();
        private static bool IsSharedIcon(Image img)
        {
            lock (_lock_icon) { return img != null && _sharedImages.Contains(img); }
        }
        private static Image GetOrCreate(string key, Func<Image> factory)
        {
            lock (_lock_icon)
            {
                if (_cache_icon.TryGetValue(key, out Image cached))
                    return cached;
                var created = factory();
                _cache_icon[key] = created;
                _sharedImages.Add(created);
                _cacheIconOrder.Enqueue(key);
                while (_cache_icon.Count > MaxIconCacheEntries && _cacheIconOrder.Count > 0)
                {
                    string oldest = _cacheIconOrder.Dequeue();
                    if (oldest != null && _cache_icon.TryGetValue(oldest, out Image victim))
                    {
                        _cache_icon.Remove(oldest);
                        _sharedImages.Remove(victim);
                        try { victim.Dispose(); } catch { }
                    }
                }
                return created;
            }
        }
        // DYNAMIC PICTUREBOX RENDERER
        // ======================================================================================================
        public static void SetPictureBoxImage(PictureBox pictureBox, Image newImage)
        {
            if (pictureBox == null) return;
            Image old = pictureBox.Image;
            if (newImage == null)
            {
                pictureBox.Image = null;
                old?.Dispose();
                return;
            }
            var resized = ResizeImageToDeviceDpi(newImage, pictureBox.Width, pictureBox.Height, TSDpiHelper.EffectiveDpi(pictureBox));
            pictureBox.Image = resized;
            if (old != null && !ReferenceEquals(old, newImage))
                old.Dispose();
        }
        public static Image ResizeImageToDeviceDpi(Image img, int maxWidth, int maxHeight, int deviceDpi)
        {
            int baseW = img.Width;
            int baseH = img.Height;
            float scale = TSDpiHelper.Scale(deviceDpi);
            int scaledW = (int)(baseW * scale);
            int scaledH = (int)(baseH * scale);
            double ratio = Math.Min((double)maxWidth / scaledW, (double)maxHeight / scaledH);
            int finalW = Math.Max(1, (int)(scaledW * ratio));
            int finalH = Math.Max(1, (int)(scaledH * ratio));
            var bmp = new Bitmap(finalW, finalH);
            bmp.SetResolution(deviceDpi, deviceDpi);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(img, 0, 0, finalW, finalH);
            }
            return bmp;
        }
        // DVG ICON RESIZER
        // ======================================================================================================
        public static Image ResizeDGIcon(Image img, int size, int deviceDpi)
        {
            if (deviceDpi <= 0) deviceDpi = TSDpiHelper.BaseDpi;
            float scale = TSDpiHelper.Scale(deviceDpi);
            int newSize = Math.Max(1, (int)Math.Round(size * scale));
            Bitmap bmp = new Bitmap(newSize, newSize);
            try { bmp.SetResolution(deviceDpi, deviceDpi); } catch { }
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(img, new Rectangle(0, 0, newSize, newSize));
            }
            return bmp;
        }
        // DYNAMIC SIZE COUNT ALGORITHM
        // ======================================================================================================
        public static string TS_FormatSize(double bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int suffixIndex = 0;
            double value = bytes;
            while (value >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1024;
                suffixIndex++;
            }
            value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            return $"{value:0.##} {suffixes[suffixIndex]}";
        }
        public static double TS_FormatSizeNoType(double bytes)
        {
            while (bytes >= 1024)
            {
                bytes /= 1024;
            }
            return Math.Round(bytes, 2);
        }
        // NORMALIZE PATH
        // ======================================================================================================
        private static readonly HashSet<char> InvalidPathCharsSet = new HashSet<char>(Path.GetInvalidPathChars().Concat(new[] { '*', '?', '"', '<', '>', '|' }).Distinct().ToArray());
        public static string TS_NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            path = path.Replace('/', '\\').Trim();
            bool isUnc = path.StartsWith("\\\\");
            try
            {
                path = Path.GetFullPath(path);
            }
            catch
            {
                path = new string(path.Where(c => !InvalidPathCharsSet.Contains(c)).ToArray());
            }
            var parts = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (isUnc && parts.Length >= 2)
            {
                return $"\\\\{string.Join("\\", parts)}";
            }
            else if (path.StartsWith("\\") && !isUnc)
            {
                return $"\\{string.Join("\\", parts)}";
            }
            else
            {
                return string.Join("\\", parts);
            }
        }
        // DYNAMIC BUTTON WIDTH
        // ======================================================================================================
        public static void TS_AdjustButtonWidth(Button btn)
        {
            if (btn == null || btn.IsDisposed) return;
            float scale = TSDpiHelper.Scale(TSDpiHelper.EffectiveDpi(btn));
            Size text = TextRenderer.MeasureText(btn.Text ?? string.Empty, btn.Font);
            int imgW = btn.Image?.Width ?? 0;
            int padW = btn.Padding.Left + btn.Padding.Right;
            int gap = (imgW > 0 && !string.IsNullOrEmpty(btn.Text)) ? (int)Math.Round(8 * scale) : 0;
            int contentW;
            //
            switch (btn.TextImageRelation)
            {
                case TextImageRelation.ImageBeforeText:
                case TextImageRelation.TextBeforeImage:
                    contentW = imgW + gap + text.Width;
                    break;
                case TextImageRelation.ImageAboveText:
                case TextImageRelation.TextAboveImage:
                case TextImageRelation.Overlay:
                default:
                    contentW = Math.Max(imgW, text.Width);
                    break;
            }
            int chrome = (int)Math.Round(16 * scale);
            btn.Width = contentW + padW + chrome;
        }
        // WINDOWS PROD KEY ALGORITHM
        // ======================================================================================================
        public static class TSWindowsProductKey
        {
            public static string GetWindowsProductKey()
            {
                Version osVer = Environment.OSVersion.Version;
                if (osVer.Major < 6)
                {
                    return "NS_OS";
                }
                RegistryView[] views = new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 };
                bool subKeyFound = false;
                foreach (RegistryView view in views)
                {
                    try
                    {
                        using (RegistryKey localMachineReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                        using (RegistryKey subKey = localMachineReg.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                        {
                            if (subKey == null) continue;
                            subKeyFound = true;
                            object oa3Value = subKey.GetValue("OA3xOriginalProductKey");
                            string oa3Key = (oa3Value as string ?? string.Empty).Trim().ToUpperInvariant();
                            if (!string.IsNullOrEmpty(oa3Key) && IsValidProductKeyFormat(oa3Key))
                            {
                                return oa3Key;
                            }
                            if (!(subKey.GetValue("DigitalProductId") is byte[] digitalProductId) || digitalProductId.Length < 67) continue;
                            string decoded = DecodeWindowsProductKey(digitalProductId);
                            if (!string.IsNullOrEmpty(decoded) && IsValidProductKeyFormat(decoded))
                            {
                                return decoded;
                            }
                        }
                    }
                    catch { continue; }
                }
                if (!subKeyFound) return "NO_SUB_KEY";
                return "NO_KEY";
            }
            private static bool IsValidProductKeyFormat(string key)
            {
                if (string.IsNullOrWhiteSpace(key)) return false;
                return Regex.IsMatch(key.Trim(), @"^[BCDFGHJKMNPQRTVWXY2346789]{5}(-[BCDFGHJKMNPQRTVWXY2346789]{5}){4}$", RegexOptions.IgnoreCase);
            }
            private static string DecodeWindowsProductKey(byte[] d_p_id)
            {
                if (d_p_id == null || d_p_id.Length < 67) return "NO_KEY";
                byte[] d = (byte[])d_p_id.Clone();
                const int key_offset = 52;
                const string digits_list = "BCDFGHJKMPQRTVWXY2346789";
                int isWin8 = (d[66] / 6) & 1;
                d[66] = (byte)((d[66] & 0xF7) | ((isWin8 & 2) * 4));
                string keyLatest = string.Empty;
                int lastIndex = 0;
                for (int i = 24; i >= 0; i--)
                {
                    int currentIndex = 0;
                    for (int j = 14; j >= 0; j--)
                    {
                        currentIndex = d[j + key_offset] + currentIndex * 256;
                        d[j + key_offset] = (byte)(currentIndex / 24);
                        currentIndex %= 24;
                        lastIndex = currentIndex;
                    }
                    keyLatest = digits_list[currentIndex] + keyLatest;
                }
                if (isWin8 == 1)
                {
                    if (lastIndex < 0 || lastIndex > 24) return "NO_KEY";
                    keyLatest = keyLatest.Substring(1, lastIndex) + "N" + keyLatest.Substring(lastIndex + 1);
                }
                for (int i = 5; i < keyLatest.Length; i += 6)
                {
                    keyLatest = keyLatest.Insert(i, "-");
                }
                if (!IsValidProductKeyFormat(keyLatest)) return "NO_KEY";
                return keyLatest;
            }
        }
        // SCREEN API
        // ======================================================================================================
        public const int ENUM_CURRENT_SETTINGS = -1;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);
        // NETWORK NAME REPLACER
        // ======================================================================================================
        public static string Net_replacer(string get_adapter_name) => get_adapter_name.Replace("[", "(").Replace("]", ")");
        // TS NATURAL SORT KEY ALGORITHM
        // ======================================================================================================
        public static string TSNaturalSortKey(string input, int paddingLength = 30)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string text = input.Trim();
            text = Regex.Replace(text, @"\d+", match => match.Value.PadLeft(paddingLength, '0'));
            return text.ToLower(CultureInfo.InvariantCulture);
        }
        // INTERNET CONNECTION STATUS
        // ======================================================================================================
        private static readonly HttpClient _netCheckClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(3) };
        public static async Task<bool> IsNetworkAvailable(CancellationToken ct = default)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;
            try
            {
                Task<bool>[] tasks = new Task<bool>[]{
                    ProbeUrlAsync(_netCheckClient, "https://www.gstatic.com/generate_204", HttpStatusCode.NoContent, ct),
                    ProbeUrlAsync(_netCheckClient, "https://www.cloudflare.com", null, ct),
                    ProbeUrlAsync(_netCheckClient, "https://www.google.com", null, ct)
                };
                var pending = new List<Task<bool>>(tasks);
                while (pending.Count > 0)
                {
                    if (ct.IsCancellationRequested) return false;
                    Task<bool> done = await Task.WhenAny(pending).ConfigureAwait(false);
                    pending.Remove(done);
                    bool ok = false;
                    try { ok = await done.ConfigureAwait(false); } catch { }
                    if (ok) return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private static async Task<bool> ProbeUrlAsync(HttpClient client, string url, HttpStatusCode? expectStatus, CancellationToken ct)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                {
                    if (expectStatus.HasValue) return response.StatusCode == expectStatus.Value;
                    return response.IsSuccessStatusCode;
                }
            }
            catch { return false; }
        }
        // WINDOWS WALLPAPER CHECK
        // ======================================================================================================
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);
        // DWM WINDOW ATTRIBUTE
        // ======================================================================================================
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
    }
}