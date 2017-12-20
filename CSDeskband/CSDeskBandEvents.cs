﻿using System;
using System.Drawing;

namespace CSDeskband
{
    public class VisibilityChangedEventArgs : EventArgs
    {
        public bool IsVisible { get; set; }
    }

    public class TaskbarOrientationChangedEventArgs : EventArgs
    {
        public TaskbarOrientation Orientation { get; set; }
    }

    public enum TaskbarOrientation
    {
        Vertical,
        Horizontal,
    }
}