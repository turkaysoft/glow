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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glow{
    internal class TSModules{
        // STARTUP LOCATION
        // ======================================================================================================
        private static readonly string StartupPath = Application.StartupPath;
        // LINK SYSTEM
        // ======================================================================================================
        public class TS_LinkSystem{
            public const string
            // Main Control Links
            github_link_lv      = "https://raw.githubusercontent.com/turkaysoft/glow/main/Glow/SoftwareVersion.txt",
            github_link_lr      = "https://github.com/turkaysoft/glow/releases/latest",
            // Social Links
            website_link        = "https://turkaysoft.com",
            github_link         = "https://github.com/turkaysoft",
            // Other Links
            ts_donate           = "https://buymeacoffee.com/turkaysoft";
        }
        // VERSIONS
        // ======================================================================================================
        public class TS_VersionEngine{
            public static string TS_SofwareVersion(int v_mode){
                string version_mode = "";
                switch (v_mode){
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
        public static class TS_VersionParser{
            public static string ParseUINormalize(string version){
                Version v = Normalize(version);
                if (v.Revision > 0)
                    return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                if (v.Build > 0)
                    return $"{v.Major}.{v.Minor}.{v.Build}";
                return $"{v.Major}.{v.Minor}";
            }
            private static Version Normalize(string version){
                if (string.IsNullOrWhiteSpace(version))
                    return new Version(0, 0, 0, 0);
                version = version.Trim();
                var parts = version.Split('.');
                while (parts.Length < 4){
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
        public static class TS_MessageBoxEngine{
            private static readonly Dictionary<int, KeyValuePair<MessageBoxButtons, MessageBoxIcon>> TSMessageBoxConfig = new Dictionary<int, KeyValuePair<MessageBoxButtons, MessageBoxIcon>>(){
                { 1, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OK, MessageBoxIcon.Information) },           // Ok and Info
                { 2, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OK, MessageBoxIcon.Warning) },               // Ok and Warning
                { 3, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.OK, MessageBoxIcon.Error) },                 // Ok and Error
                { 4, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Question) },           // Yes/No and Quest
                { 5, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Information) },        // Yes/No and Info
                { 6, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Warning) },            // Yes/No and Warning
                { 7, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNo, MessageBoxIcon.Error) },              // Yes/No and Error
                { 8, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) },        // Retry/Cancel and Error
                { 9, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) },     // Yes/No/Cancel and Quest
                { 10, new KeyValuePair<MessageBoxButtons, MessageBoxIcon>(MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information) }  // Yes/No/Cancel and Info
            };
            public static DialogResult TS_MessageBox(Form m_form, int m_mode, string m_message, string m_title = ""){
                if (m_form != null && m_form.InvokeRequired){
                    return (DialogResult)m_form.Invoke(new Func<DialogResult>(() => TS_MessageBox(m_form, m_mode, m_message, m_title)));
                }
                if (m_form != null){
                    BringFormToFront(m_form);
                }
                string m_box_title = string.IsNullOrEmpty(m_title) ? Application.ProductName : m_title;
                MessageBoxButtons m_button = MessageBoxButtons.OK;
                MessageBoxIcon m_icon = MessageBoxIcon.Information;
                if (TSMessageBoxConfig.ContainsKey(m_mode)){
                    var m_serialize = TSMessageBoxConfig[m_mode];
                    m_button = m_serialize.Key;
                    m_icon = m_serialize.Value;
                }
                return MessageBox.Show(m_form, m_message, m_box_title, m_button, m_icon);
            }
            private static void BringFormToFront(Form m_form){
                if (m_form.WindowState == FormWindowState.Minimized){
                    m_form.WindowState = FormWindowState.Normal;
                }
                m_form.BringToFront();
                m_form.Activate();
            }
        }
        // TS LOGGER - IMPROVED VERSION
        // ======================================================================================================
        public static class TSLogger{
            private static readonly object _lock = new object();
            private static bool _fileEnabled;
            private static bool _consoleEnabled;
            private static string _currentLogFile;
            private static readonly string _logDir;
            private static StreamWriter _writer;
            public static string LogDirectory => _logDir;
            public static string CurrentLogFile{
                get { lock (_lock) { return _currentLogFile; } }
            }
            static TSLogger(){
                _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "g_logs");
                try{
                    AppDomain.CurrentDomain.ProcessExit += (_, __) => CloseWriter_NoThrow();
                    AppDomain.CurrentDomain.DomainUnload += (_, __) => CloseWriter_NoThrow();
                }catch { }
            }
            public static void Enable(bool fileEnabled, bool consoleEnabled){
                lock (_lock){
                    _fileEnabled = fileEnabled;
                    _consoleEnabled = consoleEnabled;
                    CloseWriter_NoThrow();
                    if (!_fileEnabled){
                        _currentLogFile = null;
                        return;
                    }
                    InitializeInternal_NoThrow();
                }
            }
            public static void Close(){
                lock (_lock){
                    CloseWriter_NoThrow();
                }
            }
            public static void Log(string message){
                lock (_lock){
                    if (!_fileEnabled && !_consoleEnabled)
                        return;

                    Write_NoLock(message);
                }
            }
            public static void Log(Exception ex){
                if (ex == null)
                    return;
                Log(ex.ToString());
            }
            private static void InitializeInternal_NoThrow(){
                try{
                    if (!Directory.Exists(_logDir)){
                        Directory.CreateDirectory(_logDir);
                    }
                    string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    string baseName = $"Glow_{Dns.GetHostName()}_{stamp}";
                    string path;
                    for (int i = 0; i < 1000; i++){
                        string suffix = (i == 0) ? "" : "_" + i.ToString();
                        path = Path.Combine(_logDir, baseName + suffix + ".log");
                        FileStream fs = null;
                        try{
                            fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                            _currentLogFile = path;
                            _writer = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)){
                                AutoFlush = true
                            };
                            return;
                        }catch (IOException){
                            fs?.Dispose();
                            continue;
                        }
                    }
                    _writer = null;
                    _currentLogFile = null;
                    _fileEnabled = false;
                    if (_consoleEnabled){
                        Console.WriteLine("[DEBUG] " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss") + " - TSLogger: Failed to create log file (attempted 1000 names). File logging disabled.");
                    }
                }catch{
                    _writer = null;
                    _currentLogFile = null;
                    _fileEnabled = false;
                    if (_consoleEnabled){
                        Console.WriteLine("[DEBUG] " + DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss") + " - TSLogger: Exception during log initialization. File logging disabled.");
                    }
                }
            }
            private static void CloseWriter_NoThrow(){
                try{
                    if (_writer != null){
                        _writer.Flush();
                        _writer.Dispose();
                    }
                }catch (ObjectDisposedException){ }catch{ }
                _writer = null;
            }
            private static void Write_NoLock(string text){
                if (text == null)
                    text = "";
                string[] lines = SplitLines(text);
                if (lines.Length == 0)
                    return;
                string prefix = $"{DateTime.Now:dd.MM.yyyy - HH:mm:ss} - ";
                if (_consoleEnabled){
                    for (int i = 0; i < lines.Length; i++){
                        Console.WriteLine("[DEBUG] " + prefix + lines[i]);
                    }
                }
                if (_fileEnabled){
                    if (_writer == null){
                        InitializeInternal_NoThrow();
                    }
                    if (_writer != null){
                        try{
                            for (int i = 0; i < lines.Length; i++){
                                _writer.WriteLine(prefix + lines[i]);
                            }
                        }catch (ObjectDisposedException){
                            _writer = null;
                        }catch (IOException){
                            _writer = null;
                        }
                    }
                }
            }
            private static string[] SplitLines(string text){
                if (string.IsNullOrEmpty(text))
                    return Array.Empty<string>();
                string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
                string[] parts = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++){
                    if (parts[i].Length > 0)
                        return parts;
                }
                return Array.Empty<string>();
            }
        }
        // TS ERROR LOGGER - IMPROVED VERSION
        // ======================================================================================================
        public static class TSErrorLog{
            public static void LogException(Exception ex, string context = null){
                if (ex == null)
                    return;
                string formattedLog = FormatException(ex, context);
                TSLogger.Log(formattedLog);
            }
            private static string FormatException(Exception ex, string context, int depth = 0){
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
                if (ex is AggregateException aggEx && aggEx.InnerExceptions.Count > 0){
                    sb.AppendLine(indent + $"AggregateException ({aggEx.InnerExceptions.Count} inner exceptions):");
                    for (int i = 0; i < aggEx.InnerExceptions.Count; i++){
                        sb.AppendLine(indent + $"[{i}]:");
                        sb.Append(FormatException(aggEx.InnerExceptions[i], null, depth + 1));
                    }
                }else if (ex.InnerException != null){
                    sb.AppendLine(indent + "InnerException:");
                    sb.Append(FormatException(ex.InnerException, null, depth + 1));
                }
                sb.AppendLine(indent + header);
                return sb.ToString();
            }
            private static string ExtractFirstFrameInfo(Exception ex){
                try{
                    var st = new StackTrace(ex, true);
                    var frame = st.FrameCount > 0 ? st.GetFrame(0) : null;
                    if (frame != null){
                        string file = frame.GetFileName();
                        int line = frame.GetFileLineNumber();
                        if (!string.IsNullOrEmpty(file) && line > 0){
                            return $"Location   : {file}:{line}\n";
                        }
                    }
                }catch { }
                return "";
            }
        }
        // TS SOFTWARE COPYRIGHT DATE
        // ======================================================================================================
        public class TS_SoftwareCopyrightDate{
            public static string ts_scd_preloader = string.Format("\u00a9 2019-{0}, {1}.", DateTime.Now.Year, Application.CompanyName);
        }
        // SETTINGS MODULE PATHS
        // ======================================================================================================
        public static readonly string ts_sf = StartupPath + @"\" + Application.ProductName + "Settings.ini";
        public static readonly string ts_settings_container = Path.GetFileNameWithoutExtension(ts_sf);
        // SETTINGS MODULE CLASS
        // ======================================================================================================
        public class TSSettingsModule{
            private readonly string _iniFilePath;
            private readonly object _fileLock = new object();
            public TSSettingsModule(string filePath){
                _iniFilePath = filePath;
            }
            // READ SETTINGS
            public string TSReadSettings(string sectionName, string keyName){
                lock (_fileLock){
                    if (!File.Exists(_iniFilePath)) return string.Empty;
                    string[] lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8);
                    bool isInSection = string.IsNullOrEmpty(sectionName);
                    foreach (string rawLine in lines){
                        string line = rawLine.Trim();
                        if (line.Length == 0 || line.StartsWith(";"))
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]")){
                            isInSection = line.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);
                            continue;
                        }
                        if (isInSection){
                            int equalsIndex = line.IndexOf('=');
                            if (equalsIndex > 0){
                                string currentKey = line.Substring(0, equalsIndex).Trim();
                                if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase)){
                                    return line.Substring(equalsIndex + 1).Trim();
                                }
                            }
                        }
                    }
                    return string.Empty;
                }
            }
            // WRITE/UPDATE SETTINGS
            public void TSWriteSettings(string sectionName, string keyName, string value){
                lock (_fileLock){
                    List<string> lines = File.Exists(_iniFilePath) ? File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList() : new List<string>();
                    bool sectionFound = string.IsNullOrEmpty(sectionName);
                    bool keyUpdated = false;
                    int insertIndex = lines.Count;
                    for (int i = 0; i < lines.Count; i++){
                        string trimmedLine = lines[i].Trim();
                        if (trimmedLine.Length == 0 || trimmedLine.StartsWith(";"))
                            continue;
                        if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]")){
                            if (sectionFound && !keyUpdated){
                                insertIndex = i;
                                break;
                            }
                            if (!string.IsNullOrEmpty(sectionName))
                                sectionFound = trimmedLine.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);
                            continue;
                        }
                        if (sectionFound){
                            int equalsIndex = trimmedLine.IndexOf('=');
                            if (equalsIndex > 0){
                                string currentKey = trimmedLine.Substring(0, equalsIndex).Trim();
                                if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase)){
                                    lines[i] = keyName + "=" + value;
                                    keyUpdated = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!sectionFound && !string.IsNullOrEmpty(sectionName)){
                        if (lines.Count > 0) lines.Add("");
                        lines.Add("[" + sectionName + "]");
                        lines.Add(keyName + "=" + value);
                    }else if (!keyUpdated){
                        insertIndex = (insertIndex == lines.Count) ? lines.Count : insertIndex;
                        lines.Insert(insertIndex, keyName + "=" + value);
                    }
                    try{
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                    }catch (IOException){ }
                }
            }
            // DELETE SETTINSG
            public void TSDeleteSetting(string sectionName, string keyName){
                lock (_fileLock){
                    if (!File.Exists(_iniFilePath)) return;
                    List<string> lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    bool isInSection = string.IsNullOrEmpty(sectionName);
                    for (int i = 0; i < lines.Count; i++){
                        string line = lines[i].Trim();
                        if (line.Length == 0 || line.StartsWith(";"))
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]")){
                            if (!string.IsNullOrEmpty(sectionName))
                                isInSection = line.Equals("[" + sectionName + "]", StringComparison.OrdinalIgnoreCase);

                            continue;
                        }
                        if (isInSection){
                            int eqIndex = line.IndexOf('=');
                            if (eqIndex > 0){
                                string currentKey = line.Substring(0, eqIndex).Trim();
                                if (currentKey.Equals(keyName, StringComparison.OrdinalIgnoreCase)){
                                    lines.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                    try{
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                    }catch (IOException){ }
                }
            }
            // RENAME KEY
            public bool TSRenameKey(string sectionName, string oldKey, string newKey){
                lock (_fileLock){
                    if (!File.Exists(_iniFilePath)) return false;
                    List<string> lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    bool isInSection = string.IsNullOrEmpty(sectionName);
                    foreach (var raw in lines){
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith(";"))
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]")){
                            if (!string.IsNullOrEmpty(sectionName))
                                isInSection = line.Equals($"[{sectionName}]", StringComparison.OrdinalIgnoreCase);
                            else
                                isInSection = true;
                            continue;
                        }
                        if (isInSection){
                            int eq = line.IndexOf('=');
                            if (eq > 0){
                                var key = line.Substring(0, eq).Trim();
                                if (key.Equals(newKey, StringComparison.OrdinalIgnoreCase))
                                    return false;
                            }
                        }
                    }
                    isInSection = string.IsNullOrEmpty(sectionName);
                    for (int i = 0; i < lines.Count; i++){
                        string trimmed = lines[i].Trim();
                        if (trimmed.Length == 0 || trimmed.StartsWith(";"))
                            continue;
                        if (trimmed.StartsWith("[") && trimmed.EndsWith("]")){
                            if (!string.IsNullOrEmpty(sectionName))
                                isInSection = trimmed.Equals($"[{sectionName}]", StringComparison.OrdinalIgnoreCase);
                            else
                                isInSection = true;
                            continue;
                        }
                        if (isInSection){
                            int eqIndex = trimmed.IndexOf('=');
                            if (eqIndex > 0){
                                string currentKey = trimmed.Substring(0, eqIndex).Trim();
                                if (currentKey.Equals(oldKey, StringComparison.OrdinalIgnoreCase)){
                                    string value = trimmed.Substring(eqIndex + 1).Trim();
                                    lines[i] = $"{newKey}={value}";
                                    try{
                                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                                        return true;
                                    }catch (IOException){
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
            public bool TSRenameSection(string oldSectionName, string newSectionName){
                lock (_fileLock){
                    if (!File.Exists(_iniFilePath)) return false;
                    var lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    foreach (var raw in lines){
                        var t = raw.Trim();
                        if (t.StartsWith("[") && t.EndsWith("]")){
                            var sec = t.Substring(1, t.Length - 2).Trim();
                            if (sec.Equals(newSectionName, StringComparison.OrdinalIgnoreCase))
                                return false;
                        }
                    }
                    bool oldFound = false;
                    for (int i = 0; i < lines.Count; i++){
                        var trimmed = lines[i].Trim();
                        if (trimmed.StartsWith("[") && trimmed.EndsWith("]")){
                            var sec = trimmed.Substring(1, trimmed.Length - 2).Trim();
                            if (sec.Equals(oldSectionName, StringComparison.OrdinalIgnoreCase)){
                                lines[i] = $"[{newSectionName}]";
                                oldFound = true;
                                break;
                            }
                        }
                    }
                    if (!oldFound) return false;
                    try{
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                        return true;
                    }catch (IOException){
                        return false;
                    }
                }
            }
            // ORDER / NORMALIZE KEYS IN A SECTION
            public bool TSOrderSectionKeys(string sectionName, IEnumerable<string> orderedKeys){
                if (orderedKeys == null) return false;
                lock (_fileLock){
                    if (!File.Exists(_iniFilePath)) return false;
                    var lines = File.ReadAllLines(_iniFilePath, Encoding.UTF8).ToList();
                    int sectionHeaderIndex = -1;
                    int sectionEndIndex = lines.Count;
                    for (int i = 0; i < lines.Count; i++){
                        var t = lines[i].Trim();
                        if (t.StartsWith("[") && t.EndsWith("]")){
                            var sec = t.Substring(1, t.Length - 2).Trim();
                            if (sec.Equals(sectionName, StringComparison.OrdinalIgnoreCase)){
                                sectionHeaderIndex = i;
                                for (int j = i + 1; j < lines.Count; j++){
                                    var tj = lines[j].Trim();
                                    if (tj.StartsWith("[") && tj.EndsWith("]")){
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
                    var kv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = sectionHeaderIndex + 1; i < sectionEndIndex; i++){
                        var raw = lines[i];
                        var t = raw.Trim();
                        if (t.Length == 0 || t.StartsWith(";")) continue;
                        int eq = t.IndexOf('=');
                        if (eq > 0){
                            var key = t.Substring(0, eq).Trim();
                            var val = t.Substring(eq + 1).Trim();
                            if (!currentOrder.Contains(key, StringComparer.OrdinalIgnoreCase))
                                currentOrder.Add(key);
                            kv[key] = val;
                        }
                    }
                    var orderedList = orderedKeys.Where(k => !string.IsNullOrWhiteSpace(k)).Select(k => k.Trim()).ToList();
                    var desiredOrder = new List<string>();
                    foreach (var key in orderedList){
                        if (kv.ContainsKey(key))
                            desiredOrder.Add(key);
                    }
                    foreach (var key in currentOrder){
                        if (!orderedList.Contains(key, StringComparer.OrdinalIgnoreCase))
                            desiredOrder.Add(key);
                    }
                    if (currentOrder.SequenceEqual(desiredOrder, StringComparer.OrdinalIgnoreCase))
                        return true;
                    var rebuilt = new List<string> { lines[sectionHeaderIndex].Trim() };
                    foreach (var key in orderedList){
                        if (kv.TryGetValue(key, out var val))
                            rebuilt.Add($"{key}={val}");
                    }
                    foreach (var key in currentOrder){
                        if (!orderedList.Contains(key, StringComparer.OrdinalIgnoreCase) && kv.TryGetValue(key, out var val))
                            rebuilt.Add($"{key}={val}");
                    }
                    lines.RemoveRange(sectionHeaderIndex, sectionEndIndex - sectionHeaderIndex);
                    lines.InsertRange(sectionHeaderIndex, rebuilt);
                    try{
                        File.WriteAllText(_iniFilePath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
                        return true;
                    }catch (IOException){
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
        public static readonly string ts_lang_es = ts_lf + @"\Spanish.ini";             // Spanish      | es
        public static readonly string ts_lang_tr = ts_lf + @"\Turkish.ini";             // Turkish      | tr
        // LANGUAGE MANAGE FUNCTIONS
        // ======================================================================================================
        public static readonly Dictionary<string, string> AllLanguageFiles = new Dictionary<string, string> {
            { "ar", ts_lang_ar },
            { "zh", ts_lang_zh },
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
            { "es", ts_lang_es },
            { "tr", ts_lang_tr },
        };
        public static string TSPreloaderSetDefaultLanguage(string ui_lang){
            bool anyLanguageFileExists = AllLanguageFiles.Values.Any(File.Exists);
            bool isUiLangValid = !string.IsNullOrEmpty(ui_lang) && AllLanguageFiles.ContainsKey(ui_lang) && File.Exists(AllLanguageFiles[ui_lang]);
            return anyLanguageFileExists && isUiLangValid ? ui_lang : "en";
        }
        public static List<string> AvailableLanguages = AllLanguageFiles.Values.Where(filePath => File.Exists(filePath)).ToList();
        // READ LANG CLASS
        // ======================================================================================================
        public class TSGetLangs{
            private readonly string _iniFilePath;
            private readonly string _fallbackIniFilePath = ts_lang_en;
            private readonly object _cacheLock = new object();
            private string[] _cachedLines = null;
            private DateTime _lastFileWriteTime = DateTime.MinValue;
            private string[] _cachedFallbackLines = null;
            private DateTime _lastFallbackWriteTime = DateTime.MinValue;
            public TSGetLangs(string iniFilePath){
                _iniFilePath = iniFilePath;
            }
            public string TSReadLangs(string sectionName, string keyName){
                string value = FindLangsValue(GetIniLinesCached(_iniFilePath, ref _cachedLines, ref _lastFileWriteTime), sectionName, keyName);
                if (!string.IsNullOrEmpty(value)){
                    return value;
                }
                value = FindLangsValue(GetIniLinesCached(_fallbackIniFilePath, ref _cachedFallbackLines, ref _lastFallbackWriteTime), sectionName, keyName);
                if (!string.IsNullOrEmpty(value)){
                    return value;
                }
                return "N/A Langs";
            }
            private string FindLangsValue(string[] iniLines, string sectionName, string keyName){
                bool isInSection = string.IsNullOrEmpty(sectionName);
                foreach (string rawLine in iniLines){
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith(";"))
                        continue;
                    if (line.StartsWith("[") && line.EndsWith("]")){
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
            private string[] GetIniLinesCached(string path, ref string[] cache, ref DateTime lastWriteTimeUtc){
                lock (_cacheLock){
                    try{
                        if (string.IsNullOrEmpty(path) || !File.Exists(path)){
                            return Array.Empty<string>();
                        }
                        DateTime currentWriteTime = File.GetLastWriteTimeUtc(path);
                        if (cache == null || currentWriteTime != lastWriteTimeUtc){
                            cache = File.ReadAllLines(path, Encoding.UTF8);
                            lastWriteTimeUtc = currentWriteTime;
                        }
                        return cache;
                    }catch (IOException){
                        return Array.Empty<string>();
                    }
                }
            }
        }
        // TS THEME ENGINE
        // ======================================================================================================
        public class TS_ThemeEngine{
            // LIGHT THEME COLORS
            // ====================================
            public static readonly Dictionary<string, Color> LightTheme = new Dictionary<string, Color>{
                // TS PRELOADER
                { "TSBT_BGColor", Color.FromArgb(236, 242, 248) },
                { "TSBT_BGColor2", Color.White },
                { "TSBT_AccentColor", Color.FromArgb(54, 95, 146) },
                { "TSBT_LabelColor1", Color.FromArgb(51, 51, 51) },
                { "TSBT_LabelColor2", Color.FromArgb(100, 100, 100) },
                { "TSBT_CloseBG", Color.FromArgb(25, 255, 255, 255) },
                { "TSBT_CloseBGHover", Color.FromArgb(50, 255, 255, 255) },
                // HEADER MENU COLOR MODE
                { "HeaderBGColorMain", Color.White },
                { "HeaderFEColorMain", Color.FromArgb(51, 51, 51) },
                { "HeaderFEColor", Color.FromArgb(51, 51, 51) },
                { "HeaderBGColor", Color.FromArgb(236, 242, 248) },
                // ACTIVE PAGE COLOR
                { "BtnActiveColor", Color.White },
                { "BtnDeActiveColor", Color.FromArgb(236, 242, 248) },
                // UI COLOR
                { "LeftMenuBGAndBorderColor", Color.FromArgb(236, 242, 248) },
                { "LeftMenuButtonHoverAndMouseDownColor", Color.White },
                { "LeftMenuButtonAlphaColor", Color.FromArgb(50, 255, 255, 255) },
                { "LeftMenuButtonFEColor", Color.FromArgb(51, 51, 51) },
                { "LeftMenuButtonFEColor2", Color.FromArgb(27, 30, 34) },
                { "PageContainerBGAndPageContentTotalColors", Color.White },
                { "ContentPanelBGColor", Color.FromArgb(236, 242, 248) },
                { "ContentLabelLeft", Color.FromArgb(51, 51, 51) },
                { "AccentColor", Color.FromArgb(54, 95, 146) },
                { "AccentColorHover", Color.FromArgb(63, 109, 165) },
                //
                { "SelectBoxBGColor", Color.White },
                { "SelectBoxBGColor2", Color.FromArgb(236, 242, 248) },
                { "SelectBoxFEColor", Color.FromArgb(51, 51, 51) },
                { "SelectBoxBorderColor", Color.FromArgb(226, 226, 226) },
                { "CheckBoxUnCheckBorderColor", Color.FromArgb(98, 98, 98) },
                //
                { "TextBoxBGColor", Color.White },
                { "TextBoxFEColor", Color.FromArgb(51, 51, 51) },
                { "DataGridBGColor", Color.FromArgb(236, 242, 248) },
                { "DataGridFEColor", Color.FromArgb(51, 51, 51) },
                { "DataGridColor", Color.FromArgb(226, 226, 226) },
                { "DataGridAlternatingColor", Color.White },
                { "OSDAndServicesPageBG", Color.FromArgb(54, 95, 146) },
                { "OSDAndServicesPageFE", Color.White },
                { "DynamicThemeActiveBtnBG", Color.White },
                // ACCENT COLOR
                { "AccentBlue", Color.FromArgb(54, 95, 146) },
                { "AccentPurple", Color.FromArgb(118, 85, 177) },
                { "AccentRed", Color.FromArgb(207, 24, 0) },
                { "AccentGreen", Color.FromArgb(28, 122, 25) },
            };
            // DARK THEME COLORS
            // ====================================
            public static readonly Dictionary<string, Color> DarkTheme = new Dictionary<string, Color>{
                // TS PRELOADER
                { "TSBT_BGColor", Color.FromArgb(27, 30, 34) },
                { "TSBT_BGColor2", Color.FromArgb(34, 38, 44) },
                { "TSBT_AccentColor", Color.FromArgb(88, 153, 233) },
                { "TSBT_LabelColor1", Color.WhiteSmoke },
                { "TSBT_LabelColor2", Color.FromArgb(176, 184, 196) },
                { "TSBT_CloseBG", Color.FromArgb(75, 34, 38, 44) },
                { "TSBT_CloseBGHover", Color.FromArgb(75, 27,30, 34) },
                // HEADER MENU COLOR MODE
                { "HeaderBGColorMain", Color.FromArgb(34, 38, 44) },
                { "HeaderFEColorMain", Color.FromArgb(222, 222, 222) },
                { "HeaderFEColor", Color.WhiteSmoke },
                { "HeaderBGColor", Color.FromArgb(27, 30, 34) },
                 // ACTIVE PAGE COLOR
                { "BtnActiveColor", Color.FromArgb(34, 38, 44) },
                { "BtnDeActiveColor", Color.FromArgb(27, 30, 34) },
                // UI COLOR
                { "LeftMenuBGAndBorderColor", Color.FromArgb(27, 30, 34) },
                { "LeftMenuButtonHoverAndMouseDownColor", Color.FromArgb(34, 38, 44) },
                { "LeftMenuButtonAlphaColor", Color.FromArgb(50, 34, 38, 44) },
                { "LeftMenuButtonFEColor", Color.WhiteSmoke },
                { "LeftMenuButtonFEColor2", Color.White },
                { "PageContainerBGAndPageContentTotalColors", Color.FromArgb(34, 38, 44) },
                { "ContentPanelBGColor", Color.FromArgb(27, 30, 34) },
                { "ContentLabelLeft", Color.WhiteSmoke },
                { "AccentColor", Color.FromArgb(88, 153, 233) },
                { "AccentColorHover", Color.FromArgb(93, 165, 253) },
                //
                { "SelectBoxBGColor", Color.FromArgb(34, 38, 44) },
                { "SelectBoxBGColor2", Color.FromArgb(27, 30, 34) },
                { "SelectBoxFEColor", Color.WhiteSmoke },
                { "SelectBoxBorderColor", Color.FromArgb(42, 47, 53) },
                { "CheckBoxUnCheckBorderColor", Color.FromArgb(170, 170, 170) },
                //
                { "TextBoxBGColor", Color.FromArgb(34, 38, 44) },
                { "TextBoxFEColor", Color.WhiteSmoke },
                { "DataGridBGColor", Color.FromArgb(27, 30, 34) },
                { "DataGridFEColor", Color.WhiteSmoke },
                { "DataGridColor", Color.FromArgb(42, 47, 53) },
                { "DataGridAlternatingColor", Color.FromArgb(34, 38, 44) },
                { "OSDAndServicesPageBG", Color.FromArgb(88, 153, 233) },
                { "OSDAndServicesPageFE", Color.FromArgb(34, 38, 44) },
                { "DynamicThemeActiveBtnBG", Color.FromArgb(34, 38, 44) },
                // ACCENT COLOR
                { "AccentBlue", Color.FromArgb(88, 153, 233) },
                { "AccentPurple", Color.FromArgb(164, 118, 243) },
                { "AccentRed", Color.FromArgb(255, 77, 77) },
                { "AccentGreen", Color.FromArgb(38, 187, 33) },
            };
            // THEME SWITCHER
            // ====================================
            public static Color ColorMode(int theme, string key){
                if (theme == 0){
                    return DarkTheme.ContainsKey(key) ? DarkTheme[key] : Color.Black;
                }else if (theme == 1){
                    return LightTheme.ContainsKey(key) ? LightTheme[key] : Color.White;
                }
                return Color.White;
            }
        }
        // THEME MODE HELPER
        // ======================================================================================================
        public static class TSThemeModeHelper{
            private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
            [DllImport("dwmapi.dll", PreserveSig = true)]
            private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
            private static bool _isDarkModeEnabled = false;
            public static bool IsDarkModeEnabled => _isDarkModeEnabled;
            public static void SetThemeMode(bool enableTMode){
                _isDarkModeEnabled = enableTMode;
            }
            public static void InitializeThemeForForm(Form targetForm){
                if (targetForm == null || targetForm.IsDisposed)
                    return;
                ApplyThemeModeToForm(targetForm, _isDarkModeEnabled);
            }
            private static void ApplyThemeModeToForm(Form targetForm, bool enableRequire){
                if (targetForm == null || targetForm.IsDisposed)
                    return;
                int useDark = enableRequire ? 1 : 0;
                DwmSetWindowAttribute(targetForm.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                string themeName = enableRequire ? "DarkMode_Explorer" : "Explorer";
                ApplyScrollTheme(targetForm, themeName);
            }
            private static void ApplyScrollTheme(Control parentControl, string targetTheme){
                if (parentControl == null || parentControl.IsDisposed)
                    return;
                if (parentControl is DataGridView || parentControl is ListBox || parentControl is ListView || parentControl is TreeView || parentControl is RichTextBox || parentControl is Panel || parentControl is Form || (parentControl is TextBox tb && tb.Multiline)){
                    if (parentControl.Tag as string != targetTheme){
                        SetWindowTheme(parentControl.Handle, targetTheme, null);
                        if (parentControl is DataGridView dgv){
                            foreach (Control c in dgv.Controls){
                                if (c is ScrollBar)
                                    SetWindowTheme(c.Handle, targetTheme, null);
                            }
                        }
                        parentControl.Tag = targetTheme;
                    }
                }
                if (parentControl is Form || parentControl is TabControl || parentControl is TabPage || parentControl is Panel || parentControl is GroupBox || parentControl is UserControl){
                    if (parentControl.HasChildren){
                        foreach (Control childControl in parentControl.Controls){
                            ApplyScrollTheme(childControl, targetTheme);
                        }
                    }
                }
            }
            public static int GetSystemTheme(int theme_mode){
                if (theme_mode == 2){
                    using (var getSystemThemeKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")){
                        theme_mode = (int)(getSystemThemeKey?.GetValue("SystemUsesLightTheme") ?? 1);
                    }
                }
                return theme_mode;
            }
        }
        // DPI SENSITIVE DYNAMIC IMAGE RENDERER
        // ======================================================================================================
        private static readonly object _lock_icon = new object();
        private static readonly Dictionary<string, Image> _cache_icon = new Dictionary<string, Image>();
        public static void TSImageRenderer(object baseTarget, Image sourceImage, int basePadding, ContentAlignment imageAlign = ContentAlignment.MiddleCenter){
            if (baseTarget == null || sourceImage == null) return;
            const int minImageSize = 16;
            Image ResizeImage(Image targetImg, int targetSize){
                var bmp = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp)){
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
            try{
                int size;
                float dpi;
                if (baseTarget is Control ctrl){
                    dpi = ctrl.DeviceDpi > 0 ? ctrl.DeviceDpi : 96f;
                    int padding = (int)Math.Round(basePadding * (dpi / 96f));
                    size = ctrl.Height - padding;
                    if (size <= 0) size = minImageSize;
                    string key = $"{sourceImage.GetHashCode()}_C_{size}_{dpi}";
                    newImage = GetOrCreate(key, () => ResizeImage(sourceImage, size));
                    if (ctrl is Button btn){
                        oldImage = btn.Image;
                        btn.Image = newImage;
                        btn.ImageAlign = imageAlign;
                    }else if (ctrl is PictureBox pb){
                        oldImage = pb.Image;
                        pb.Image = newImage;
                        pb.SizeMode = PictureBoxSizeMode.Zoom;
                    }else{
                        newImage.Dispose();
                        newImage = null;
                    }
                }else if (baseTarget is ToolStripItem item){
                    dpi = item.GetCurrentParent()?.DeviceDpi ?? 96f;
                    int padding = (int)Math.Round(basePadding * (dpi / 96f));
                    size = item.Height - padding;
                    if (size <= 0) size = minImageSize;
                    string key = $"{sourceImage.GetHashCode()}_T_{size}_{dpi}";
                    newImage = GetOrCreate(key, () => ResizeImage(sourceImage, size));
                    oldImage = item.Image;
                    item.Image = newImage;
                }else{
                    return;
                }
                if (oldImage != null && !ReferenceEquals(oldImage, sourceImage)){
                    oldImage.Dispose();
                }
            }catch{
                newImage?.Dispose();
            }
        }
        private static Image GetOrCreate(string key, Func<Image> factory){
            lock (_lock_icon){
                if (_cache_icon.TryGetValue(key, out Image cached))
                    return cached;
                var created = factory();
                _cache_icon[key] = created;
                return created;
            }
        }
        // SET DYNAMIC SIZE ALGORITHM
        // ======================================================================================================
        public static void SetPictureBoxImage(PictureBox pictureBox, Image newImage){
            if (pictureBox == null) return;
            Image old = pictureBox.Image;
            if (newImage == null){
                pictureBox.Image = null;
                old?.Dispose();
                return;
            }
            var resized = ResizeImageToDeviceDpi(newImage, pictureBox.Width, pictureBox.Height, pictureBox.DeviceDpi);
            pictureBox.Image = resized;
            if (old != null && !ReferenceEquals(old, newImage))
                old.Dispose();
        }
        public static Image ResizeImageToDeviceDpi(Image img, int maxWidth, int maxHeight, int deviceDpi){
            int baseW = img.Width;
            int baseH = img.Height;
            float scale = deviceDpi / 96f;
            int scaledW = (int)(baseW * scale);
            int scaledH = (int)(baseH * scale);
            double ratio = Math.Min((double)maxWidth / scaledW, (double)maxHeight / scaledH);
            int finalW = Math.Max(1, (int)(scaledW * ratio));
            int finalH = Math.Max(1, (int)(scaledH * ratio));
            var bmp = new Bitmap(finalW, finalH);
            bmp.SetResolution(deviceDpi, deviceDpi);
            using (var g = Graphics.FromImage(bmp)){
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(img, 0, 0, finalW, finalH);
            }
            return bmp;
        }
        public static Image ResizeDGIcon(Image img, int size, int deviceDpi){
            float scale = deviceDpi / 96f;
            int newSize = (int)(size * scale);
            Bitmap bmp = new Bitmap(newSize, newSize);
            using (Graphics g = Graphics.FromImage(bmp)){
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
        public static string TS_FormatSize(double bytes){
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int suffixIndex = 0;
            double value = bytes;
            while (value >= 1024 && suffixIndex < suffixes.Length - 1){
                value /= 1024;
                suffixIndex++;
            }
            value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            return $"{value:0.##} {suffixes[suffixIndex]}";
        }
        public static double TS_FormatSizeNoType(double bytes){
            while (bytes >= 1024){
                bytes /= 1024;
            }
            return Math.Round(bytes, 2);
        }
        // DYNAMIC BUTTON WIDTH
        // ======================================================================================================
        public static void TS_AdjustButtonWidth(Button btn){
            if (btn == null || btn.IsDisposed) return;
            float scale = btn.DeviceDpi / 96f;
            Size text = TextRenderer.MeasureText(btn.Text ?? string.Empty, btn.Font);
            int imgW = btn.Image?.Width ?? 0;
            int padW = btn.Padding.Left + btn.Padding.Right;
            int gap = (imgW > 0 && !string.IsNullOrEmpty(btn.Text)) ? (int)Math.Round(8 * scale) : 0;
            int contentW;
            //
            switch (btn.TextImageRelation){
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
        public static class TSWindowsProductKey{
            public static string GetWindowsProductKey(){
                Version osVer = Environment.OSVersion.Version;
                if (osVer.Major < 6 || (osVer.Major == 6 && osVer.Minor < 2)){
                    return "NS_OS";
                }
                using (RegistryKey localMachineReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (RegistryKey subKey = localMachineReg.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")){
                    if (subKey == null) return "NO_SUB_KEY";
                    object regKeyValue = subKey.GetValue("DigitalProductId");
                    if (regKeyValue == null) return "NO_KEY";
                    byte[] digitalProductId = (byte[])regKeyValue;
                    return DecodeWindowsProductKey(digitalProductId);
                }
            }
            private static string DecodeWindowsProductKey(byte[] d_p_id){
                const int key_offset = 52;
                const string digits_list = "BCDFGHJKMPQRTVWXY2346789";
                byte win10Mode = (byte)((d_p_id[66] / 6) & 1);
                d_p_id[66] = (byte)((d_p_id[66] & 0xF7) | (win10Mode & 2) * 4);
                string keyLatest = string.Empty;
                int lastIndex = 0;
                for (int i = 24; i >= 0; i--){
                    int currentIndex = 0;
                    for (int j = 14; j >= 0; j--){
                        currentIndex = d_p_id[j + key_offset] + currentIndex * 256;
                        d_p_id[j + key_offset] = (byte)(currentIndex / 24);
                        currentIndex %= 24;
                        lastIndex = currentIndex;
                    }
                    keyLatest = digits_list[currentIndex] + keyLatest;
                }
                keyLatest = keyLatest.Substring(1, lastIndex) + "N" + keyLatest.Substring(lastIndex + 1);
                for (int i = 5; i < keyLatest.Length; i += 6){
                    keyLatest = keyLatest.Insert(i, "-");
                }
                return keyLatest;
            }
        }
        // SCREEN API
        // ======================================================================================================
        public const int ENUM_CURRENT_SETTINGS = -1;
        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE{
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
        public struct DISPLAY_DEVICE{
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
        public static string TSNaturalSortKey(string input, int paddingLength = 30){
            if (string.IsNullOrEmpty(input)) return "";
            string text = input.Trim();
            text = Regex.Replace(text, @"\d+", match => match.Value.PadLeft(paddingLength, '0'));
            return text.ToLower(CultureInfo.InvariantCulture);
        }
        // INTERNET CONNECTION STATUS
        // ======================================================================================================
        public static async Task<bool> IsNetworkAvailable(){
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;
            string[] urls ={
                "https://www.gstatic.com/generate_204",
                "https://www.cloudflare.com",
                "https://www.google.com"
            };
            try{
                using (HttpClient client = new HttpClient()){
                    client.Timeout = TimeSpan.FromSeconds(3);
                    foreach (var url in urls){
                        try{
                            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)){
                                if (response.IsSuccessStatusCode)
                                    return true;
                            }
                        }catch{ }
                    }
                }
            }catch{
                return false;
            }
            return false;
        }
        // WINDOWS WALLPAPER CHECK
        // ======================================================================================================
        [DllImport("user32.dll", SetLastError = true, PreserveSig = true)]
        public static extern bool SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);
        // DPI AWARE V2
        // ======================================================================================================
        [DllImport("user32.dll", PreserveSig = true)]
        public static extern bool SetProcessDpiAwarenessContext(IntPtr dpiFlag);
        public static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);
        // ENABLE EDGE WHEN BORDER IS CLOSED FOR WINDOWS 11
        // ======================================================================================================
        [DllImport("dwmapi.dll", SetLastError = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public enum DWM_WINDOW_CORNER_PREFERENCE{ Default = 0, DoNotRound = 1, Round = 2, RoundSmall = 3 }
    }
}