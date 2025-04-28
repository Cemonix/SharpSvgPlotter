using System;

namespace SharpSvgPlotter.Components;

public class RangeException : Exception
{
    public string ClassName { get; } = string.Empty;
    
    public RangeException(string message) : base(message) {}

    public RangeException(string message, Exception innerException) : base(message, innerException) {}

    public RangeException(string message, string className) : base(message)
    {
        ClassName = className;
    }
}