using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DotNetTurtle.Core;

namespace DotNetTurtle.Avalonia;

/// <summary>
/// An Avalonia control that renders turtle graphics.
/// </summary>
public class TurtleCanvas : Control, ITurtleCanvas
{
    private readonly List<DrawCommand> _commands = [];
    private readonly Dictionary<int, TurtleState> _turtles = [];
    private int _nextTurtleId = 0;
    private TurtleColor _backgroundColor = TurtleColor.White;

    public TurtleCanvas()
    {
        // Provide a delay function that works with Avalonia's UI thread
        DelayFunc = async (ms) =>
        {
            await Task.Delay(ms);
            await Dispatcher.UIThread.InvokeAsync(() => InvalidateVisual());
        };
    }

    public new double Width => Bounds.Width;
    public new double Height => Bounds.Height;

    public TurtleColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            InvalidateVisual();
        }
    }

    public Func<int, Task>? DelayFunc { get; set; }

    public IReadOnlyList<DrawCommand> Commands => _commands.AsReadOnly();

    public void AddCommand(DrawCommand command)
    {
        _commands.Add(command);
    }

    public void Clear()
    {
        _commands.Clear();
        InvalidateVisual();
    }

    public void Invalidate()
    {
        InvalidateVisual();
    }

    public int RegisterTurtle(TurtleState state)
    {
        var id = _nextTurtleId++;
        _turtles[id] = state;
        return id;
    }

    public void UpdateTurtle(int turtleId, TurtleState state)
    {
        _turtles[turtleId] = state;
    }

    public void RemoveTurtle(int turtleId)
    {
        _turtles.Remove(turtleId);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Draw background
        var bgBrush = new SolidColorBrush(ToAvaloniaColor(_backgroundColor));
        context.DrawRectangle(bgBrush, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Draw all commands
        foreach (var command in _commands)
        {
            RenderCommand(context, command);
        }

        // Draw all visible turtles
        foreach (var turtle in _turtles.Values)
        {
            if (turtle.IsVisible)
            {
                DrawTurtle(context, turtle);
            }
        }
    }

    private void RenderCommand(DrawingContext context, DrawCommand command)
    {
        switch (command)
        {
            case LineCommand line:
                var linePen = new Pen(new SolidColorBrush(ToAvaloniaColor(line.Color)), line.Thickness);
                context.DrawLine(linePen, new Point(line.X1, line.Y1), new Point(line.X2, line.Y2));
                break;

            case DotCommand dot:
                var dotBrush = new SolidColorBrush(ToAvaloniaColor(dot.Color));
                context.DrawEllipse(dotBrush, null, new Point(dot.X, dot.Y), dot.Size / 2, dot.Size / 2);
                break;

            case CircleCommand circle:
                var circlePen = new Pen(new SolidColorBrush(ToAvaloniaColor(circle.Color)), circle.Thickness);
                context.DrawEllipse(null, circlePen, new Point(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius);
                break;

            case FilledCircleCommand filledCircle:
                var fillBrush = new SolidColorBrush(ToAvaloniaColor(filledCircle.FillColor));
                context.DrawEllipse(fillBrush, null, new Point(filledCircle.CenterX, filledCircle.CenterY), filledCircle.Radius, filledCircle.Radius);
                break;

            case TextCommand text:
                var textBrush = new SolidColorBrush(ToAvaloniaColor(text.Color));
                var formattedText = new FormattedText(
                    text.Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(text.FontFamily),
                    text.FontSize,
                    textBrush);

                var x = text.Alignment switch
                {
                    Core.TextAlignment.Center => text.X - formattedText.Width / 2,
                    Core.TextAlignment.Right => text.X - formattedText.Width,
                    _ => text.X
                };

                context.DrawText(formattedText, new Point(x, text.Y - formattedText.Height));
                break;

            case EndFillCommand fill:
                if (fill.Points.Count >= 3)
                {
                    var geometry = new StreamGeometry();
                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(new Point(fill.Points[0].X, fill.Points[0].Y), true);
                        for (int i = 1; i < fill.Points.Count; i++)
                        {
                            ctx.LineTo(new Point(fill.Points[i].X, fill.Points[i].Y));
                        }
                        ctx.EndFigure(true);
                    }
                    var fillBrush2 = new SolidColorBrush(ToAvaloniaColor(fill.FillColor));
                    context.DrawGeometry(fillBrush2, null, geometry);
                }
                break;
        }
    }

    private void DrawTurtle(DrawingContext context, TurtleState state)
    {
        var radians = state.Heading * Math.PI / 180;
        
        // Save transform state by using a transform group
        using (context.PushTransform(Matrix.CreateTranslation(state.X, state.Y)))
        using (context.PushTransform(Matrix.CreateRotation(radians + Math.PI / 2)))
        {
            var shellBrush = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Forest green
            var shellDarkBrush = new SolidColorBrush(Color.FromRgb(0, 100, 0)); // Dark green
            var skinBrush = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // Light green
            var eyeBrush = new SolidColorBrush(Colors.Black);
            var outlinePen = new Pen(shellDarkBrush, 1.5);

            // Legs (draw first so they appear behind shell)
            var legLength = 8.0;
            var legWidth = 4.0;
            
            // Front left leg
            var frontLegGeom = CreateLeg(-6, -8, legLength, legWidth, -0.4);
            context.DrawGeometry(skinBrush, outlinePen, frontLegGeom);
            
            // Front right leg
            var frontRightLegGeom = CreateLeg(6, -8, legLength, legWidth, 0.4);
            context.DrawGeometry(skinBrush, outlinePen, frontRightLegGeom);
            
            // Back left leg
            var backLeftLegGeom = CreateLeg(-6, 8, legLength, legWidth, -2.7);
            context.DrawGeometry(skinBrush, outlinePen, backLeftLegGeom);
            
            // Back right leg
            var backRightLegGeom = CreateLeg(6, 8, legLength, legWidth, 2.7);
            context.DrawGeometry(skinBrush, outlinePen, backRightLegGeom);

            // Tail
            var tailGeom = new StreamGeometry();
            using (var ctx = tailGeom.Open())
            {
                ctx.BeginFigure(new Point(-2, 12), true);
                ctx.LineTo(new Point(0, 18));
                ctx.LineTo(new Point(2, 12));
                ctx.EndFigure(true);
            }
            context.DrawGeometry(skinBrush, outlinePen, tailGeom);

            // Shell (oval body)
            context.DrawEllipse(shellBrush, outlinePen, new Point(0, 0), 10, 12);
            
            // Shell pattern - hexagonal segments
            var patternPen = new Pen(shellDarkBrush, 1);
            context.DrawEllipse(null, patternPen, new Point(0, 0), 5, 6);
            
            // Shell pattern lines
            context.DrawLine(patternPen, new Point(0, -12), new Point(0, -6));
            context.DrawLine(patternPen, new Point(0, 12), new Point(0, 6));
            context.DrawLine(patternPen, new Point(-10, 0), new Point(-5, 0));
            context.DrawLine(patternPen, new Point(10, 0), new Point(5, 0));
            context.DrawLine(patternPen, new Point(-7, -7), new Point(-3.5, -4));
            context.DrawLine(patternPen, new Point(7, -7), new Point(3.5, -4));
            context.DrawLine(patternPen, new Point(-7, 7), new Point(-3.5, 4));
            context.DrawLine(patternPen, new Point(7, 7), new Point(3.5, 4));

            // Head
            context.DrawEllipse(skinBrush, outlinePen, new Point(0, -16), 5, 5);
            
            // Eyes
            context.DrawEllipse(eyeBrush, null, new Point(-2, -17), 1.2, 1.2);
            context.DrawEllipse(eyeBrush, null, new Point(2, -17), 1.2, 1.2);
        }
    }

    private static StreamGeometry CreateLeg(double x, double y, double length, double width, double angle)
    {
        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var perpCos = Math.Cos(angle + Math.PI / 2);
            var perpSin = Math.Sin(angle + Math.PI / 2);
            
            var halfWidth = width / 2;
            
            // Base of leg (attached to body)
            var baseLeft = new Point(x + perpCos * halfWidth, y + perpSin * halfWidth);
            var baseRight = new Point(x - perpCos * halfWidth, y - perpSin * halfWidth);
            
            // Tip of leg (foot)
            var tipX = x + cos * length;
            var tipY = y + sin * length;
            var tipLeft = new Point(tipX + perpCos * halfWidth * 0.7, tipY + perpSin * halfWidth * 0.7);
            var tipRight = new Point(tipX - perpCos * halfWidth * 0.7, tipY - perpSin * halfWidth * 0.7);
            
            ctx.BeginFigure(baseLeft, true);
            ctx.LineTo(tipLeft);
            ctx.LineTo(tipRight);
            ctx.LineTo(baseRight);
            ctx.EndFigure(true);
        }
        return geom;
    }

    private static Color ToAvaloniaColor(TurtleColor color) =>
        Color.FromArgb(color.A, color.R, color.G, color.B);
}
