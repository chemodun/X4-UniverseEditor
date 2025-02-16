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
      StatusMessage = string.Empty; // Clear the message
      StatusMessageType = StatusMessageType.Info; // Reset the message type
    }

    // Method to set status message with type
    public void SetStatusMessage(string message, StatusMessageType messageType, bool infinity = false)
    {
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
