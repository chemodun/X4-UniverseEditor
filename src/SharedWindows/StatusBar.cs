using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SharedWindows
{
  public class StatusBarMessage : INotifyPropertyChanged
  {
    private string _statusMessage = "Ready";
    private string? _pendingMessage;
    private StatusMessageType _pendingType;
    private bool _pendingInfinity;
    public string StatusMessage
    {
      get => _statusMessage;
      set
      {
        if (_statusMessage != value)
        {
          _statusMessage = value;
          OnPropertyChanged(nameof(StatusMessage));
          StartStatusMessageTimer();
        }
      }
    }

    private StatusMessageType _statusMessageType = StatusMessageType.Info;
    public StatusMessageType StatusMessageType
    {
      get => _statusMessageType;
      set
      {
        if (_statusMessageType != value)
        {
          _statusMessageType = value;
          OnPropertyChanged(nameof(StatusMessageType));
        }
      }
    }
    private DispatcherTimer? _statusMessageTimer;
    private readonly TimeSpan _statusMessageDisplayDuration = TimeSpan.FromSeconds(5); // Adjust as needed

    private bool Infinity { get; set; }

    private bool IsActive => (Infinity || (_statusMessageTimer?.IsEnabled ?? false));

    private void StartStatusMessageTimer()
    {
      // Stop existing timer if any
      _statusMessageTimer?.Stop();

      if (string.IsNullOrEmpty(_statusMessage) || Infinity)
        return;

      // Initialize and start the timer
      _statusMessageTimer = new DispatcherTimer { Interval = _statusMessageDisplayDuration };
      _statusMessageTimer.Tick += StatusMessageTimer_Tick;
      _statusMessageTimer.Start();
    }

    private void StatusMessageTimer_Tick(object? sender, EventArgs e)
    {
      _statusMessageTimer?.Stop();
      _statusMessageTimer = null;

      // If there is a pending suppressed message, show it now instead of clearing
      if (!string.IsNullOrEmpty(_pendingMessage))
      {
        var msg = _pendingMessage;
        var type = _pendingType;
        var inf = _pendingInfinity;
        _pendingMessage = null;
        _pendingInfinity = false;

        // Display the pending message (this will start/reset timer appropriately)
        SetStatusMessage(msg!, type, inf);
        return;
      }

      StatusMessage = string.Empty; // Clear the message
      StatusMessageType = StatusMessageType.Info; // Reset the message type
    }

    // Method to set status message with type
    public void SetStatusMessage(string message, StatusMessageType messageType, bool infinity = false)
    {
      // Suppress lower-priority messages while a higher-priority message is active (timer not finished)
      // Allow equal or higher priority to replace immediately.
      if (IsActive && messageType < _statusMessageType)
      {
        // If current message is infinite, do not enqueue lower-priority message; simply ignore
        if (Infinity)
        {
          return;
        }

        // Store as pending to show after current timer elapses
        _pendingMessage = message;
        _pendingType = messageType;
        _pendingInfinity = infinity;
        return;
      }

      Infinity = infinity;
      StatusMessageType = messageType;
      StatusMessage = message;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public enum StatusMessageType
  {
    Info,
    Warning,
    Error,
  }
}
