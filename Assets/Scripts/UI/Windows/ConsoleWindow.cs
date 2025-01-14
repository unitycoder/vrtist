﻿/* MIT License
 *
 * Copyright (c) 2021 Ubisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRtist
{
    public class LogItem
    {
        public LogType type;
        public string text;
        public string stackTrace;
    }

    public sealed class FixedSizeQueue<T> : Queue<T>
    {
        public int Capacity { get; }

        public FixedSizeQueue(int capacity)
        {
            Capacity = capacity;
        }

        public new T Enqueue(T item)
        {
            base.Enqueue(item);
            if (base.Count > Capacity)
            {
                return base.Dequeue();
            }
            return default;
        }
    }

    public class ConsoleWindow : MonoBehaviour
    {
        [Tooltip("Max number of logs"), Range(10, 500)] public int capacity = 100;

        private Transform mainPanel = null;
        private UIVerticalSlider scrollbar = null;

        private UILabel[] labels = new UILabel[8];
        private bool dirty = false;
        private bool scrollDirty = false;
        private int scrollIndex = 0;

        private bool showInfos = true;
        private bool showWarnings = true;
        private bool showErrors = true;

        FixedSizeQueue<LogItem> logs;
        List<LogItem> filteredLogs = new List<LogItem>();

        void Start()
        {
            mainPanel = transform.Find("MainPanel");
            if (mainPanel != null)
            {
                scrollbar = mainPanel.Find("Scrollbar")?.GetComponent<UIVerticalSlider>();

                for (int i = 0; i < labels.Length; ++i)
                {
                    labels[i] = mainPanel.Find($"Line_{i}").GetComponent<UILabel>();
                }
            }

            logs = new FixedSizeQueue<LogItem>(capacity);
            Application.logMessageReceived += HandleLog;
        }

        private void Update()
        {
            if (dirty)
            {
                FilterLogs();
            }

            if (dirty || scrollDirty)
            {
                int index = 0;      // for lines
                int logsIndex = 0;  // for log items
                foreach (LogItem item in filteredLogs)
                {
                    // Apply scroll
                    if (logsIndex < scrollIndex)
                    {
                        ++logsIndex;
                        continue;
                    }

                    // Set lines
                    labels[index].Text = item.text.Substring(0, Math.Min(item.text.Length, 65));  // TMP truncate is bugged
                    if (labels[index].Text.Length < item.text.Length) { labels[index].Text += "..."; }

                    switch (item.type)
                    {
                        case LogType.Log: labels[index].Image = UIUtils.LoadIcon("info"); break;
                        case LogType.Warning: labels[index].Image = UIUtils.LoadIcon("warning"); break;
                        case LogType.Error:
                        case LogType.Exception:
                        case LogType.Assert:
                            labels[index].Image = UIUtils.LoadIcon("error");
                            break;
                    }

                    ++index;
                    if (index == labels.Length) { break; }
                }
                dirty = false;
                scrollDirty = false;
            }
        }

        private void HandleLog(string text, string stackTrace, LogType type)
        {
            logs.Enqueue(new LogItem { type = type, text = text, stackTrace = stackTrace });
            dirty = true;
        }

        public void OnScroll(int value)
        {
            scrollIndex = value;
            scrollDirty = true;
        }

        private void FilterLogs()
        {
            filteredLogs.Clear();
            foreach (LogItem item in logs)
            {
                if (!showInfos && item.type == LogType.Log) { continue; }
                if (!showWarnings && item.type == LogType.Warning) { continue; }
                if (!showErrors && (item.type == LogType.Error || item.type == LogType.Exception || item.type == LogType.Assert)) { continue; }

                filteredLogs.Add(item);
            }

            scrollbar.maxValue = Math.Max(filteredLogs.Count - 8, 0.1f);  // there are 8 lines of logs
            scrollbar.Value = scrollbar.maxValue;
            scrollIndex = (int) scrollbar.Value;
        }

        private void ClearLabels()
        {
            foreach (UILabel label in labels)
            {
                label.Text = "";
                label.Image = UIUtils.LoadIcon("empty");
            }
        }

        public void Clear()
        {
            logs.Clear();
            filteredLogs.Clear();
            scrollIndex = 0;
            scrollbar.maxValue = 0.1f;
            ClearLabels();
        }

        public void ShowErrors(bool value)
        {
            showErrors = value;
            ClearLabels();
            dirty = true;
        }

        public void ShowWarnings(bool value)
        {
            showWarnings = value;
            ClearLabels();
            dirty = true;
        }

        public void ShowInfos(bool value)
        {
            showInfos = value;
            ClearLabels();
            dirty = true;
        }
    }
}
