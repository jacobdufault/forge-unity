using Forge.Utilities;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Forge.Unity {
    /// <summary>
    /// This class has been adapted from
    /// www.valion-game.com/295/use-log4net-for-efficient-logging-in-unity3d/
    /// </summary>
    public class UnityLogWindow : MonoBehaviour {
        /// <summary>
        /// Have we already initialized core data structures?
        /// </summary>
        [NonSerialized]
        private static bool _initialized;

        private void Awake() {
            if (_initialized) {
                return;
            }
            _initialized = true;

            // initialize log4net
            ConfigureLog();

            Log<UnityLogWindow>.Info("Logging system started");
            Log<UnityLogWindow>.Info("Operating System: " + SystemInfo.operatingSystem);
            Log<UnityLogWindow>.Info("System Spec: " + SystemInfo.processorType + " - #" + SystemInfo.processorCount + " (" + SystemInfo.systemMemorySize + ")");
            Log<UnityLogWindow>.Info("Device Spec: " + SystemInfo.deviceName + " " + SystemInfo.deviceModel + " [" + SystemInfo.deviceUniqueIdentifier + "]");
            Log<UnityLogWindow>.Info("Graphic Spec: " + SystemInfo.graphicsDeviceID + ":" + SystemInfo.graphicsDeviceName + " - "
             + SystemInfo.graphicsDeviceVendorID + ":" + SystemInfo.graphicsDeviceVendor + " (" + SystemInfo.graphicsMemorySize + "MB, Shader Level " + SystemInfo.graphicsShaderLevel + ")");

            // default window state
            int margins = 40;
            _windowRect = new Rect(margins, margins, Screen.width - 2 * margins, Screen.height - 2 * margins);

            // get our log appender
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            IAppender[] appenders = hierarchy.GetAppenders();
            foreach (IAppender appender in appenders) {
                if (appender is UnityLogAppender) {
                    _logAppender = (UnityLogAppender)appender;
                    break;
                }
            }

            if (_logAppender == null) {
                Debug.LogWarning("There is not UnityLogAppender; there will be no in-game logging support");
            }
        }

        #region Log Setup
        private string ConfigureFrom(Stream originalStream) {
            using (StreamReader reader = new StreamReader(originalStream)) {
                string fileContent = reader.ReadToEnd();
                fileContent = fileContent.Replace("[[date_replacement]]", string.Format("{0:MM.dd.yyyy_HH.mm.ss}", DateTime.Now));

                using (Stream updatedStream = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(updatedStream)) {
                    writer.Write(fileContent);
                    writer.Flush();
                    updatedStream.Seek(0, SeekOrigin.Begin);

                    XmlConfigurator.Configure(updatedStream);
                }

                return fileContent;
            }
        }

        private static void WriteLog(string message) {
            Log<UnityLogWindow>.Info(message);
            Debug.Log(message);
        }

        private static void WriteLogFatal(string message) {
            Log<UnityLogWindow>.Fatal(message);
            Debug.LogError(message);
        }

        private void ConfigureLog() {
            try {
                // try to configure from the file override first
                FileInfo fileInfo = new FileInfo("log_config.xml");
                if (fileInfo.Exists) {
                    using (FileStream stream = fileInfo.OpenRead()) {
                        string loadedSettings = ConfigureFrom(stream);
                        WriteLog("Created log using override settings; settings were " + Environment.NewLine + loadedSettings);
                        return;
                    }
                }

                // file override failed; fall back to the log config in resources
                TextAsset textAsset = (TextAsset)Resources.Load("log_config");
                if (textAsset != null) {
                    using (MemoryStream originalStream = new MemoryStream(textAsset.bytes)) {
                        string loadedSettings = ConfigureFrom(originalStream);
                        WriteLog("Created log from resources settings; settings were " + Environment.NewLine + loadedSettings);
                        return;
                    }
                }
            }
            catch (Exception e) {
                Debug.LogError("Failed to configure logging from file:" + e.Message);
            }

            // file configuration failed; fall back to hard-coded backup; setup the default
            // configuration
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Shutdown();
            hierarchy.ResetConfiguration();
            hierarchy.Root.Level = log4net.Core.Level.All;

            ConfigureAppenders();
            WriteLogFatal("Unable to find or use override or resources log XML files; using hard-coded default format");
        }

        private void ConfigureAppenders() {
            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%date [%thread] %-5level %logger - %message%n";
            layout.ActivateOptions();

            UnityLogAppender logAppender = new UnityLogAppender();
            logAppender.MaximumEntries = 600;
            logAppender.Threshold = log4net.Core.Level.Debug;
            logAppender.Layout = layout;
            logAppender.ActivateOptions();

            if (Application.isWebPlayer) {
                UnityEngine.Debug.Log("Detected Web-Player, only using appended logger");
                BasicConfigurator.Configure(logAppender);
                return;
            }

            ConsoleAppender consoleAppener = new ConsoleAppender();
            consoleAppener.Layout = layout;
            consoleAppener.Threshold = log4net.Core.Level.Debug;
            consoleAppener.Target = ConsoleAppender.ConsoleOut;
            consoleAppener.ActivateOptions();

            PatternLayout fileLayout = new PatternLayout();
            fileLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%n";
            fileLayout.ActivateOptions();

            FileAppender fileAppender = new FileAppender();
            fileAppender.AppendToFile = true;
            fileAppender.Threshold = log4net.Core.Level.Debug;
            fileAppender.File = string.Format("Logs/log_{0:MM-dd-yyyy_HH-mm-ss}.txt", DateTime.Now);
            fileAppender.Layout = fileLayout;
            fileAppender.ImmediateFlush = false;
            fileAppender.ActivateOptions();

            UnityEngine.Debug.Log("log4net is targeting file " + fileAppender.File);
            BasicConfigurator.Configure(fileAppender, logAppender, consoleAppener);
        }
        #endregion

        private void OnApplicationQuit() {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Shutdown();
        }

        private void OnDestroy() {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Shutdown();
        }

        private static ILog _log = LogManager.GetLogger(typeof(UnityLogWindow));
        private static GUIContent gcDrag = new GUIContent("", "drag to resize");

        private UnityLogAppender _logAppender;

        private Rect _windowRect;
        private Vector2 _scrollPosition;

        private bool _visible;

        private bool _isResizing = false;
        private Rect _windowResizeStart = new Rect();
        private Vector2 _minWindowSize = new Vector2(75, 50);

        private void Update() {
            if (Input.GetKeyUp(KeyCode.F12)) {
                _visible = !_visible;
                _log.Debug("Log Window is visible: " + _visible);
            }
        }

        private void OnGUI() {
            if (_visible) {
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Native), _windowRect, DrawLogWindow, "Log Window");
            }
        }

        private string GetLogMessages(string regex) {
            if (_logAppender == null) {
                return "";
            }

            LoggingEvent[] events = _logAppender.GetEvents();

            string logMessages;
            using (StringWriter writer = new StringWriter()) {
                for (int i = 0; i < events.Length; ++i) {
                    LoggingEvent ev = events[i];
                    _logAppender.Layout.Format(writer, ev);
                }

                logMessages = writer.ToString();
            }

            string[] messages = logMessages.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < messages.Length; ++i) {
                if (Regex.IsMatch(messages[i], regex, RegexOptions.IgnoreCase)) {
                    result.Append(messages[i]);
                    result.Append(Environment.NewLine);
                }
            }

            return result.ToString();
        }

        private string _regex = "";

        private void DrawLogWindow(int windowId) {
            _regex = GUILayout.TextField(_regex) ?? "";

            string logMessages = GetLogMessages(_regex);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none);
            GUILayout.TextArea(logMessages);
            GUILayout.EndScrollView();

            _windowRect = ResizeWindow(_windowRect, ref _isResizing, ref _windowResizeStart, _minWindowSize);
            GUI.DragWindow();
        }

        private static Rect ResizeWindow(Rect windowRect, ref bool isResizing, ref Rect resizeStart, Vector2 minWindowSize) {
            Vector2 mouse = GUIUtility.ScreenToGUIPoint(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));

            int margin = 25;
            Rect r = new Rect(windowRect.width - margin, windowRect.height - margin, margin * 2, margin * 2);

            if (Event.current.type == EventType.mouseDown && r.Contains(mouse)) {
                isResizing = true;
                resizeStart = new Rect(mouse.x, mouse.y, windowRect.width, windowRect.height);
                //Event.current.Use(); // the GUI.Button below will eat the event, and this way it
                                       // will show its active state
            }
            else if (Event.current.type == EventType.mouseUp && isResizing) {
                isResizing = false;
            }
            else if (!Input.GetMouseButton(0)) {
                // if the mouse is over some other window we won't get an event, this just kind of
                // circumvents that by checking the button state directly
                isResizing = false;
            }
            else if (isResizing) {
                windowRect.width = Mathf.Max(minWindowSize.x, resizeStart.width + (mouse.x - resizeStart.x));
                windowRect.height = Mathf.Max(minWindowSize.y, resizeStart.height + (mouse.y - resizeStart.y));
                windowRect.xMax = Mathf.Min(Screen.width, windowRect.xMax); // modifying xMax
                                                                            // affects width, not x
                windowRect.yMax = Mathf.Min(Screen.height, windowRect.yMax); // modifying yMax
                                                                             // affects height, not
                                                                             // y
            }

            //GUI.Button(r, gcDrag, styleWindowResize);
            GUI.Button(r, gcDrag);

            return windowRect;
        }
    }
}