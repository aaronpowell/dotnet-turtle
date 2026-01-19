namespace DotNetTurtle.Core;

/// <summary>
/// Represents a color for turtle graphics.
/// </summary>
public readonly record struct TurtleColor(byte R, byte G, byte B, byte A = 255)
{
    public static TurtleColor Black => new(0, 0, 0);
    public static TurtleColor White => new(255, 255, 255);
    public static TurtleColor Red => new(255, 0, 0);
    public static TurtleColor Green => new(0, 255, 0);
    public static TurtleColor Blue => new(0, 0, 255);
    public static TurtleColor Yellow => new(255, 255, 0);
    public static TurtleColor Orange => new(255, 165, 0);
    public static TurtleColor Purple => new(128, 0, 128);
    public static TurtleColor Cyan => new(0, 255, 255);
    public static TurtleColor Magenta => new(255, 0, 255);
    public static TurtleColor Pink => new(255, 192, 203);
    public static TurtleColor Brown => new(165, 42, 42);
    public static TurtleColor Gray => new(128, 128, 128);
    public static TurtleColor LightGray => new(211, 211, 211);
    public static TurtleColor DarkGray => new(64, 64, 64);

    public static TurtleColor FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        return hex.Length switch
        {
            6 => new TurtleColor(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16)),
            8 => new TurtleColor(
                Convert.ToByte(hex[..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16),
                Convert.ToByte(hex[6..8], 16)),
            _ => throw new ArgumentException("Invalid hex color format", nameof(hex))
        };
    }
}
