using log4net;
using log4net.Appender;
using log4net.Core;
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
        private static ILog _log = LogManager.GetLogger(typeof(UnityLogWindow));
        private static GUIContent gcDrag = new GUIContent("", "drag to resize");

        private UnityLogAppender _logAppender;

        private Rect _windowRect;
        private Vector2 _scrollPosition;

        private bool _visible;

        private bool _isResizing = false;
        private Rect _windowResizeStart = new Rect();
        private Vector2 _minWindowSize = new Vector2(75, 50);

        public void Awake() {
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
        }

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