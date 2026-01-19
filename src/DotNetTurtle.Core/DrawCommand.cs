namespace DotNetTurtle.Core;

/// <summary>
/// Represents a drawing command that can be rendered.
/// </summary>
public abstract record DrawCommand;

/// <summary>
/// Command to draw a line from one point to another.
/// </summary>
public record LineCommand(double X1, double Y1, double X2, double Y2, TurtleColor Color, double Thickness) : DrawCommand;

/// <summary>
/// Command to draw a dot at a specific position.
/// </summary>
public record DotCommand(double X, double Y, double Size, TurtleColor Color) : DrawCommand;

/// <summary>
/// Command to draw a circle (or arc) at a specific position.
/// </summary>
public record CircleCommand(double CenterX, double CenterY, double Radius, TurtleColor Color, double Thickness, double StartAngle = 0, double SweepAngle = 360) : DrawCommand;

/// <summary>
/// Command to draw filled circle.
/// </summary>
public record FilledCircleCommand(double CenterX, double CenterY, double Radius, TurtleColor FillColor) : DrawCommand;

/// <summary>
/// Command to write text at a specific position.
/// </summary>
public record TextCommand(double X, double Y, string Text, string FontFamily, double FontSize, TurtleColor Color, TextAlignment Alignment) : DrawCommand;

/// <summary>
/// Text alignment options.
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// Command to begin filling a shape.
/// </summary>
public record BeginFillCommand(TurtleColor FillColor) : DrawCommand;

/// <summary>
/// Command to end filling a shape with all the points collected.
/// </summary>
public record EndFillCommand(IReadOnlyList<(double X, double Y)> Points, TurtleColor FillColor) : DrawCommand;
