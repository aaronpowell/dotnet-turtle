namespace DotNetTurtle.Core;

/// <summary>
/// Represents the current state of a turtle.
/// </summary>
public record TurtleState
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Heading { get; init; }
    public bool IsPenDown { get; init; } = true;
    public TurtleColor PenColor { get; init; } = TurtleColor.Black;
    public double PenSize { get; init; } = 1.0;
    public bool IsVisible { get; init; } = true;
    public double Speed { get; init; } = 5.0;
}
