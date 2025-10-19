using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


    public class BCLogger : MonoBehaviour
    {
        [SerializeField] Text _logText;
        [SerializeField] int _visibleMessageCount = 40;


        private int _lastMessageCount;

        public static bool s_Debug;

        private static int s_VisibleMessageCount;

        private static Text s_LogText;

        private static List<string> s_LogList = new List<string>();

        private static StringBuilder s_StringBuilder = new StringBuilder();


        private void Awake()
        {
            s_LogText = _logText;
            s_VisibleMessageCount = _visibleMessageCount;
            Log("Log console initialized.", false);
            s_Debug = Debug.isDebugBuild;
        }

        private void Update()
        {
            if (!s_LogText || !_logText.enabled) return;

            lock (s_LogList)
            {
                if (_lastMessageCount != s_LogList.Count)
                {
                    s_StringBuilder.Clear();
                    var startIndex = Mathf.Max(s_LogList.Count - s_VisibleMessageCount, 0);
                    for (int i = startIndex; i < s_LogList.Count; ++i)
                    {
                        s_StringBuilder.Append($"{i:000}> {s_LogList[i]}\n");
                    }

                    s_LogText.text = s_StringBuilder.ToString();
                }

                _lastMessageCount = s_LogList.Count;
            }
        }

        public static void Log(string message, bool error = false)
        {
            if (!s_Debug) return;

            if (!error)
                Debug.Log("### " + message);
            else
                Debug.LogError("### " + message);

            if (!s_LogText || !s_LogText.enabled) return;

            lock (s_LogList)
            {
                if (s_LogList == null)
                    s_LogList = new List<string>();

                s_LogList.Add(message);
            }
        }
    }
