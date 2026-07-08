using System.Collections.Generic;
using UnityEngine;

namespace PsychoMenuCU.Utilities
{
    public static class NotificationUtils
    {
        private class NotificationData
        {
            public string Text;
            public float EndTime;

            public NotificationData(string text, float duration)
            {
                Text = text;
                EndTime = Time.time + duration;
            }
        }

        private static readonly List<NotificationData> notifications = new List<NotificationData>();

        private static GUIStyle boxStyle;
        private static GUIStyle textStyle;

        public static void Show(string text, float duration = 4f)
        {
            notifications.Add(new NotificationData(text, duration));
        }

        public static void Draw()
        {
            if (notifications.Count == 0)
                return;

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);

                textStyle = new GUIStyle(GUI.skin.label);
                textStyle.alignment = TextAnchor.UpperLeft;
                textStyle.fontSize = 13;
                textStyle.wordWrap = true;
                textStyle.normal.textColor = Color.white;
            }

            const float width = 300f;
            const float height = 55f;
            const float margin = 15f;

            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                NotificationData n = notifications[i];

                if (Time.time >= n.EndTime)
                {
                    notifications.RemoveAt(i);
                    continue;
                }

                float remaining = n.EndTime - Time.time;
                float alpha = Mathf.Clamp01(remaining);

                Rect rect = new Rect(
                    Screen.width - width - margin,
                    Screen.height - height - margin - ((notifications.Count - 1 - i) * (height + 10)),
                    width,
                    height
                );

                Color old = GUI.color;

                GUI.color = new Color(0f, 0f, 0f, alpha * 0.8f);
                GUI.Box(rect, GUIContent.none, boxStyle);

                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(
                    new Rect(rect.x + 8, rect.y + 8, rect.width - 16, 18),
                    n.Text,
                    textStyle
                );

                if (GUI.Button(
                    new Rect(rect.x + 8, rect.y + 28, rect.width - 16, 24),
                    "OK"))
                {
                    notifications.RemoveAt(i);
                    GUI.color = old;
                    continue;
                }

                GUI.color = old;
            }
        }
    }
}