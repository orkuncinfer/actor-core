using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexCore.DebugOverlay
{
    public class DebugOverlay
{
    private static DebugOverlay _instance;
    public static DebugOverlay Instance => _instance ?? (_instance = new DebugOverlay());

    private class DebugMessage
    {
        public string Text;
        public Color Color;
        public float TimeToLive;

        public DebugMessage(string text, Color color, float duration)
        {
            Text = text;
            Color = color;
            TimeToLive = duration;
        }
    }

    private readonly List<DebugMessage> _messages = new List<DebugMessage>();
    private readonly float _maxLogTime = 5f; // Duration before messages disappear
    private readonly int _maxMessages = 25; // Max messages on screen

    private DebugOverlay()
    {
        Application.logMessageReceived += HandleUnityLog;
    }

    ~DebugOverlay()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    public void Log(string message, float duration = 5f)
    {
        AddMessage(message, Color.white, duration);
    }

    public void LogWarning(string message, float duration = 5f)
    {
        AddMessage(message, Color.yellow, duration);
    }

    public void LogError(string message, float duration = 5f)
    {
        AddMessage(message, Color.red, duration);
    }

    private void AddMessage(string message, Color color, float duration)
    {
        if (_messages.Count >= _maxMessages)
            _messages.RemoveAt(0); // Remove the oldest message if at capacity

        _messages.Add(new DebugMessage(message, color, duration));
    }

    private void HandleUnityLog(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                LogError(condition);
                break;
            case LogType.Warning:
                LogWarning(condition);
                break;
            default:
                Log(condition);
                break;
        }
    }

    public void Update(float deltaTime)
    {
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            _messages[i].TimeToLive -= deltaTime;
            if (_messages[i].TimeToLive <= 0)
                _messages.RemoveAt(i);
        }
    }

    public void Draw()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            normal = { textColor = Color.white }
        };

        float y = Screen.height - 25; // Start from bottom
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            style.normal.textColor = _messages[i].Color;
            GUI.Label(new Rect(10, y, 500, 30), _messages[i].Text, style);
            y -= 25;
        }
    }
}
}

