namespace DotNetTurtle.Core;

/// <summary>
/// Interface for a turtle graphics canvas that receives drawing commands.
/// </summary>
public interface ITurtleCanvas
{
    /// <summary>
    /// Gets the width of the canvas.
    /// </summary>
    double Width { get; }

    /// <summary>
    /// Gets the height of the canvas.
    /// </summary>
    double Height { get; }

    /// <summary>
    /// Gets or sets the background color of the canvas.
    /// </summary>
    TurtleColor BackgroundColor { get; set; }

    /// <summary>
    /// Adds a draw command to the canvas.
    /// </summary>
    void AddCommand(DrawCommand command);

    /// <summary>
    /// Clears all drawing commands from the canvas.
    /// </summary>
    void Clear();

    /// <summary>
    /// Requests the canvas to redraw.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Registers a turtle with the canvas and returns its ID.
    /// </summary>
    int RegisterTurtle(TurtleState state);

    /// <summary>
    /// Updates a turtle's visual representation.
    /// </summary>
    void UpdateTurtle(int turtleId, TurtleState state);

    /// <summary>
    /// Removes a turtle from the canvas.
    /// </summary>
    void RemoveTurtle(int turtleId);

    /// <summary>
    /// Gets all draw commands.
    /// </summary>
    IReadOnlyList<DrawCommand> Commands { get; }

    /// <summary>
    /// Gets or sets the delay function for animation. 
    /// The canvas implementation provides this to ensure UI thread safety.
    /// </summary>
    Func<int, Task>? DelayFunc { get; set; }
}
