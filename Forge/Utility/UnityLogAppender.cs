using log4net.Appender;
using log4net.Core;
using System;

namespace Forge.Unity {
    /// <summary>
    /// This class has been adapted from
    /// www.valion-game.com/295/use-log4net-for-efficient-logging-in-unity3d/
    /// </summary>
    public class UnityLogAppender : MemoryAppender {
        private int maximumEntries;

        public UnityLogAppender() {
            maximumEntries = 600;
        }

        public int MaximumEntries {
            get {
                return this.maximumEntries;
            }
            set {
                this.maximumEntries = Math.Max(0, value);
            }
        }

        protected override void Append(LoggingEvent loggingEvent) {
            try {
                base.Append(loggingEvent);
                if (maximumEntries == 0) {
                    return;
                }

                lock (m_eventsList.SyncRoot) {
                    int elementsToRemove = m_eventsList.Count - maximumEntries;
                    if (elementsToRemove > 0) {
                        m_eventsList.RemoveRange(0, elementsToRemove);
                    }
                }
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError(e);
            }
        }
    }
}