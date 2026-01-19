# DotNetTurtle

A .NET implementation of turtle graphics, inspired by Python's [turtle module](https://docs.python.org/3/library/turtle.html) and the classic [Logo programming language](https://en.wikipedia.org/wiki/Logo_(programming_language)).

Built with [Avalonia](https://avaloniaui.net/) for cross-platform support (Windows, macOS, Linux).

## Features

- 🐢 Classic turtle graphics with animated movement
- 🎨 Full color support with preset colors and custom RGB/hex values
- ✏️ Pen control (up/down, color, size)
- 🔄 Multiple turtles on the same canvas
- ⚡ Async/await API for smooth animations
- 🖥️ Cross-platform (Windows, macOS, Linux)

## Quick Start

```csharp
using DotNetTurtle.Avalonia;
using DotNetTurtle.Core;

using var window = TurtleWindow.Create();
var turtle = window.CreateTurtle();

// Draw a square
turtle.Speed(6).PenColor(TurtleColor.Blue);
for (int i = 0; i < 4; i++)
{
    await turtle.Forward(100);
    await turtle.Right(90);
}

window.WaitForClose();
```

## Multiple Turtles

```csharp
using var window = TurtleWindow.Create();

var red = window.CreateTurtle();
var blue = window.CreateTurtle();

red.PenColor(TurtleColor.Red);
blue.PenColor(TurtleColor.Blue);

// Move both turtles simultaneously
await Task.WhenAll(
    red.Forward(100),
    blue.Forward(100)
);

window.WaitForClose();
```

## API Reference

### Movement
- `Forward(distance)` / `Fd(distance)` - Move forward
- `Backward(distance)` / `Bk(distance)` - Move backward
- `Right(angle)` / `Rt(angle)` - Turn right
- `Left(angle)` / `Lt(angle)` - Turn left
- `GoTo(x, y)` - Move to absolute position
- `Home()` - Return to center, facing right
- `Circle(radius, extent?, steps?)` - Draw a circle or arc

### Pen Control
- `PenUp()` / `Pu()` - Stop drawing
- `PenDown()` / `Pd()` - Start drawing
- `PenColor(color)` - Set pen color
- `PenSize(size)` - Set line thickness

### Appearance
- `ShowTurtle()` / `St()` - Show the turtle
- `HideTurtle()` / `Ht()` - Hide the turtle
- `Speed(speed)` - Set animation speed (0=instant, 1=slowest, 10=fastest)

### Colors
Use preset colors like `TurtleColor.Red`, `TurtleColor.Blue`, etc., or create custom colors:
```csharp
turtle.PenColor(new TurtleColor(255, 128, 0));  // RGB
turtle.PenColor(TurtleColor.FromHex("#FF8000")); // Hex
```

## Requirements

- .NET 10.0 or later

## Building

```bash
dotnet build
```

## Running the Sample

```bash
dotnet run --project samples/DotNetTurtle.Sample
```

## License

MIT
