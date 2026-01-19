using DotNetTurtle.Avalonia;
using DotNetTurtle.Core;

// Create a turtle window
using var window = TurtleWindow.Create(title: "DotNetTurtle - Multiple Turtles");

// Create three turtles with different colors
var red = window.CreateTurtle();
var green = window.CreateTurtle();
var blue = window.CreateTurtle();

// Set up the turtles
red.Speed(7).PenColor(TurtleColor.Red).PenSize(3);
green.Speed(7).PenColor(TurtleColor.Green).PenSize(3);
blue.Speed(7).PenColor(TurtleColor.Blue).PenSize(3);

// Position them facing different directions
await red.Left(90);    // Face up
await green.Right(30); // Face right-ish
await blue.Right(150); // Face left-ish

// Draw simultaneously - each turtle draws a spiral
for (int i = 0; i < 50; i++)
{
    // Move all three turtles together
    await Task.WhenAll(
        red.Forward(i * 3),
        green.Forward(i * 3),
        blue.Forward(i * 3)
    );
    
    await Task.WhenAll(
        red.Right(61),
        green.Right(61),
        blue.Right(61)
    );
}

// Hide the turtles when done
red.HideTurtle();
green.HideTurtle();
blue.HideTurtle();

// Keep window open until user closes it
window.WaitForClose();