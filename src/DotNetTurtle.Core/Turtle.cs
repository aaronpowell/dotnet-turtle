namespace DotNetTurtle.Core;

/// <summary>
/// A turtle that can draw on a canvas, similar to Python's turtle module.
/// All movement methods are async to support animation.
/// </summary>
public class Turtle
{
    private readonly ITurtleCanvas _canvas;
    private readonly int _id;
    private TurtleState _state;
    private TurtleColor _fillColor = TurtleColor.Black;
    private bool _isFilling;
    private readonly List<(double X, double Y)> _fillPoints = [];
    private bool _animate = true;

    public Turtle(ITurtleCanvas canvas)
    {
        _canvas = canvas;
        _state = new TurtleState
        {
            X = canvas.Width / 2,
            Y = canvas.Height / 2,
            Heading = 0,
            Speed = 6
        };
        _id = canvas.RegisterTurtle(_state);
    }

    /// <summary>
    /// Gets the unique ID of this turtle.
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// Gets the current state of the turtle.
    /// </summary>
    public TurtleState State => _state;

    /// <summary>
    /// Gets the canvas the turtle is drawing on.
    /// </summary>
    public ITurtleCanvas Canvas => _canvas;

    /// <summary>
    /// Gets or sets whether animation is enabled.
    /// </summary>
    public bool Animate
    {
        get => _animate;
        set => _animate = value;
    }

    private int GetDelayMs()
    {
        if (!_animate || _state.Speed == 0) return 0;
        // Speed 1 = slowest (100ms), Speed 10 = fastest (5ms)
        return (int)(105 - _state.Speed * 10);
    }

    private async Task AnimateAsync()
    {
        var delay = GetDelayMs();
        if (delay > 0 && _canvas.DelayFunc != null)
        {
            await _canvas.DelayFunc(delay);
        }
    }

    #region Movement

    /// <summary>
    /// Move the turtle forward by the specified distance.
    /// </summary>
    public async Task<Turtle> Forward(double distance)
    {
        var steps = _animate && _state.Speed > 0 ? Math.Max(1, (int)(Math.Abs(distance) / 5)) : 1;
        var stepDistance = distance / steps;
        var radians = _state.Heading * Math.PI / 180;
        var dx = stepDistance * Math.Cos(radians);
        var dy = stepDistance * Math.Sin(radians);

        for (int i = 0; i < steps; i++)
        {
            var newX = _state.X + dx;
            var newY = _state.Y + dy;

            if (_state.IsPenDown)
            {
                _canvas.AddCommand(new LineCommand(_state.X, _state.Y, newX, newY, _state.PenColor, _state.PenSize));
            }

            if (_isFilling && i == steps - 1)
            {
                _fillPoints.Add((newX, newY));
            }

            _state = _state with { X = newX, Y = newY };
            UpdateCanvas();
            await AnimateAsync();
        }

        return this;
    }

    /// <summary>
    /// Alias for Forward.
    /// </summary>
    public Task<Turtle> Fd(double distance) => Forward(distance);

    /// <summary>
    /// Move the turtle backward by the specified distance.
    /// </summary>
    public Task<Turtle> Backward(double distance) => Forward(-distance);

    /// <summary>
    /// Alias for Backward.
    /// </summary>
    public Task<Turtle> Bk(double distance) => Backward(distance);

    /// <summary>
    /// Alias for Backward.
    /// </summary>
    public Task<Turtle> Back(double distance) => Backward(distance);

    /// <summary>
    /// Turn the turtle right by the specified angle in degrees.
    /// </summary>
    public async Task<Turtle> Right(double angle)
    {
        var steps = _animate && _state.Speed > 0 ? Math.Max(1, (int)(Math.Abs(angle) / 10)) : 1;
        var stepAngle = angle / steps;

        for (int i = 0; i < steps; i++)
        {
            _state = _state with { Heading = (_state.Heading + stepAngle) % 360 };
            UpdateCanvas();
            await AnimateAsync();
        }

        return this;
    }

    /// <summary>
    /// Alias for Right.
    /// </summary>
    public Task<Turtle> Rt(double angle) => Right(angle);

    /// <summary>
    /// Turn the turtle left by the specified angle in degrees.
    /// </summary>
    public Task<Turtle> Left(double angle) => Right(-angle);

    /// <summary>
    /// Alias for Left.
    /// </summary>
    public Task<Turtle> Lt(double angle) => Left(angle);

    /// <summary>
    /// Move the turtle to an absolute position.
    /// </summary>
    public async Task<Turtle> GoTo(double x, double y)
    {
        var canvasX = _canvas.Width / 2 + x;
        var canvasY = _canvas.Height / 2 - y;

        var totalDistance = Math.Sqrt(Math.Pow(canvasX - _state.X, 2) + Math.Pow(canvasY - _state.Y, 2));
        var steps = _animate && _state.Speed > 0 ? Math.Max(1, (int)(totalDistance / 5)) : 1;

        var startX = _state.X;
        var startY = _state.Y;

        for (int i = 1; i <= steps; i++)
        {
            var t = (double)i / steps;
            var newX = startX + (canvasX - startX) * t;
            var newY = startY + (canvasY - startY) * t;

            if (_state.IsPenDown)
            {
                _canvas.AddCommand(new LineCommand(_state.X, _state.Y, newX, newY, _state.PenColor, _state.PenSize));
            }

            if (_isFilling && i == steps)
            {
                _fillPoints.Add((newX, newY));
            }

            _state = _state with { X = newX, Y = newY };
            UpdateCanvas();
            await AnimateAsync();
        }

        return this;
    }

    /// <summary>
    /// Alias for GoTo.
    /// </summary>
    public Task<Turtle> SetPos(double x, double y) => GoTo(x, y);

    /// <summary>
    /// Alias for GoTo.
    /// </summary>
    public Task<Turtle> SetPosition(double x, double y) => GoTo(x, y);

    /// <summary>
    /// Set the turtle's x coordinate.
    /// </summary>
    public Task<Turtle> SetX(double x) => GoTo(x, YCor());

    /// <summary>
    /// Set the turtle's y coordinate.
    /// </summary>
    public Task<Turtle> SetY(double y) => GoTo(XCor(), y);

    /// <summary>
    /// Set the turtle's heading in degrees.
    /// </summary>
    public async Task<Turtle> SetHeading(double angle)
    {
        var diff = (angle - _state.Heading + 540) % 360 - 180;
        await Right(diff);
        return this;
    }

    /// <summary>
    /// Alias for SetHeading.
    /// </summary>
    public Task<Turtle> Seth(double angle) => SetHeading(angle);

    /// <summary>
    /// Move turtle to the origin and set heading to 0.
    /// </summary>
    public async Task<Turtle> Home()
    {
        await GoTo(0, 0);
        await SetHeading(0);
        return this;
    }

    /// <summary>
    /// Draw a circle with given radius.
    /// </summary>
    public async Task<Turtle> Circle(double radius, double extent = 360, int? steps = null)
    {
        var stepsToUse = steps ?? (int)Math.Max(Math.Abs(extent) / 5, 1);
        var stepAngle = extent / stepsToUse;
        var stepLength = 2 * Math.PI * Math.Abs(radius) * Math.Abs(extent) / 360 / stepsToUse;

        for (int i = 0; i < stepsToUse; i++)
        {
            await Forward(stepLength);
            if (radius > 0)
                await Left(stepAngle);
            else
                await Right(Math.Abs(stepAngle));
        }

        return this;
    }

    /// <summary>
    /// Draw a dot with given diameter.
    /// </summary>
    public Turtle Dot(double? size = null, TurtleColor? color = null)
    {
        var dotSize = size ?? Math.Max(_state.PenSize + 4, _state.PenSize * 2);
        var dotColor = color ?? _state.PenColor;
        _canvas.AddCommand(new DotCommand(_state.X, _state.Y, dotSize, dotColor));
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Stamp a copy of the turtle shape onto the canvas.
    /// </summary>
    public Turtle Stamp()
    {
        var size = 10.0;
        var radians = _state.Heading * Math.PI / 180;
        
        var tipX = _state.X + size * Math.Cos(radians);
        var tipY = _state.Y + size * Math.Sin(radians);
        
        var leftX = _state.X + size * 0.5 * Math.Cos(radians + 2.5);
        var leftY = _state.Y + size * 0.5 * Math.Sin(radians + 2.5);
        
        var rightX = _state.X + size * 0.5 * Math.Cos(radians - 2.5);
        var rightY = _state.Y + size * 0.5 * Math.Sin(radians - 2.5);

        _canvas.AddCommand(new LineCommand(tipX, tipY, leftX, leftY, _state.PenColor, _state.PenSize));
        _canvas.AddCommand(new LineCommand(leftX, leftY, rightX, rightY, _state.PenColor, _state.PenSize));
        _canvas.AddCommand(new LineCommand(rightX, rightY, tipX, tipY, _state.PenColor, _state.PenSize));
        
        UpdateCanvas();
        return this;
    }

    #endregion

    #region Position and Heading

    /// <summary>
    /// Gets the turtle's x coordinate (relative to center).
    /// </summary>
    public double XCor() => _state.X - _canvas.Width / 2;

    /// <summary>
    /// Gets the turtle's y coordinate (relative to center).
    /// </summary>
    public double YCor() => _canvas.Height / 2 - _state.Y;

    /// <summary>
    /// Gets the turtle's heading in degrees.
    /// </summary>
    public double Heading() => _state.Heading;

    /// <summary>
    /// Gets the turtle's position as a tuple.
    /// </summary>
    public (double X, double Y) Position() => (XCor(), YCor());

    /// <summary>
    /// Alias for Position.
    /// </summary>
    public (double X, double Y) Pos() => Position();

    /// <summary>
    /// Calculate the distance to a point.
    /// </summary>
    public double Distance(double x, double y)
    {
        var dx = x - XCor();
        var dy = y - YCor();
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Calculate the angle towards a point.
    /// </summary>
    public double Towards(double x, double y)
    {
        var dx = x - XCor();
        var dy = y - YCor();
        return Math.Atan2(-dy, dx) * 180 / Math.PI;
    }

    #endregion

    #region Pen Control

    /// <summary>
    /// Pull the pen down - drawing when moving.
    /// </summary>
    public Turtle PenDown()
    {
        _state = _state with { IsPenDown = true };
        return this;
    }

    /// <summary>
    /// Alias for PenDown.
    /// </summary>
    public Turtle Pd() => PenDown();

    /// <summary>
    /// Alias for PenDown.
    /// </summary>
    public Turtle Down() => PenDown();

    /// <summary>
    /// Pull the pen up - no drawing when moving.
    /// </summary>
    public Turtle PenUp()
    {
        _state = _state with { IsPenDown = false };
        return this;
    }

    /// <summary>
    /// Alias for PenUp.
    /// </summary>
    public Turtle Pu() => PenUp();

    /// <summary>
    /// Alias for PenUp.
    /// </summary>
    public Turtle Up() => PenUp();

    /// <summary>
    /// Returns true if pen is down.
    /// </summary>
    public bool IsDown() => _state.IsPenDown;

    /// <summary>
    /// Set the pen color.
    /// </summary>
    public Turtle PenColor(TurtleColor color)
    {
        _state = _state with { PenColor = color };
        return this;
    }

    /// <summary>
    /// Set the pen color using RGB values (0-255).
    /// </summary>
    public Turtle PenColor(byte r, byte g, byte b) => PenColor(new TurtleColor(r, g, b));

    /// <summary>
    /// Set the pen color using a hex string.
    /// </summary>
    public Turtle PenColor(string hex) => PenColor(TurtleColor.FromHex(hex));

    /// <summary>
    /// Get the current pen color.
    /// </summary>
    public TurtleColor GetPenColor() => _state.PenColor;

    /// <summary>
    /// Set the fill color.
    /// </summary>
    public Turtle FillColor(TurtleColor color)
    {
        _fillColor = color;
        return this;
    }

    /// <summary>
    /// Set the fill color using RGB values (0-255).
    /// </summary>
    public Turtle FillColor(byte r, byte g, byte b) => FillColor(new TurtleColor(r, g, b));

    /// <summary>
    /// Get the current fill color.
    /// </summary>
    public TurtleColor GetFillColor() => _fillColor;

    /// <summary>
    /// Set both pen and fill color.
    /// </summary>
    public Turtle Color(TurtleColor color)
    {
        PenColor(color);
        FillColor(color);
        return this;
    }

    /// <summary>
    /// Set the pen size (line thickness).
    /// </summary>
    public Turtle PenSize(double size)
    {
        _state = _state with { PenSize = size };
        return this;
    }

    /// <summary>
    /// Alias for PenSize.
    /// </summary>
    public Turtle Width(double size) => PenSize(size);

    /// <summary>
    /// Get the current pen size.
    /// </summary>
    public double GetPenSize() => _state.PenSize;

    #endregion

    #region Filling

    /// <summary>
    /// Begin filling a shape.
    /// </summary>
    public Turtle BeginFill()
    {
        _isFilling = true;
        _fillPoints.Clear();
        _fillPoints.Add((_state.X, _state.Y));
        return this;
    }

    /// <summary>
    /// End filling a shape.
    /// </summary>
    public Turtle EndFill()
    {
        if (_isFilling && _fillPoints.Count >= 3)
        {
            _canvas.AddCommand(new EndFillCommand(_fillPoints.ToList(), _fillColor));
        }
        _isFilling = false;
        _fillPoints.Clear();
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Returns true if currently filling.
    /// </summary>
    public bool Filling() => _isFilling;

    #endregion

    #region Visibility

    /// <summary>
    /// Make the turtle visible.
    /// </summary>
    public Turtle ShowTurtle()
    {
        _state = _state with { IsVisible = true };
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Alias for ShowTurtle.
    /// </summary>
    public Turtle St() => ShowTurtle();

    /// <summary>
    /// Make the turtle invisible.
    /// </summary>
    public Turtle HideTurtle()
    {
        _state = _state with { IsVisible = false };
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Alias for HideTurtle.
    /// </summary>
    public Turtle Ht() => HideTurtle();

    /// <summary>
    /// Returns true if turtle is visible.
    /// </summary>
    public bool IsVisible() => _state.IsVisible;

    #endregion

    #region Speed

    /// <summary>
    /// Set the turtle's speed (0 = instant, 1 = slowest, 10 = fastest).
    /// </summary>
    public Turtle Speed(double speed)
    {
        _state = _state with { Speed = Math.Clamp(speed, 0, 10) };
        return this;
    }

    /// <summary>
    /// Get the current speed.
    /// </summary>
    public double GetSpeed() => _state.Speed;

    #endregion

    #region Text

    /// <summary>
    /// Write text at the current turtle position.
    /// </summary>
    public Turtle Write(string text, bool move = false, TextAlignment align = TextAlignment.Left, string font = "Arial", double size = 12)
    {
        _canvas.AddCommand(new TextCommand(_state.X, _state.Y, text, font, size, _state.PenColor, align));
        UpdateCanvas();
        return this;
    }

    #endregion

    #region Canvas Operations

    /// <summary>
    /// Clear the canvas and reset the turtle.
    /// </summary>
    public Turtle Reset()
    {
        _canvas.Clear();
        _state = new TurtleState
        {
            X = _canvas.Width / 2,
            Y = _canvas.Height / 2,
            Heading = 0,
            Speed = _state.Speed
        };
        _isFilling = false;
        _fillPoints.Clear();
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Clear the canvas but don't move the turtle.
    /// </summary>
    public Turtle Clear()
    {
        _canvas.Clear();
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Set the background color.
    /// </summary>
    public Turtle BgColor(TurtleColor color)
    {
        _canvas.BackgroundColor = color;
        UpdateCanvas();
        return this;
    }

    /// <summary>
    /// Set the background color using RGB values.
    /// </summary>
    public Turtle BgColor(byte r, byte g, byte b) => BgColor(new TurtleColor(r, g, b));

    #endregion

    private void UpdateCanvas()
    {
        _canvas.UpdateTurtle(_id, _state);
        _canvas.Invalidate();
    }
}
