using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using DotNetTurtle.Core;

namespace DotNetTurtle.Avalonia;

/// <summary>
/// A disposable turtle graphics window. Create with TurtleWindow.Create() and use CreateTurtle() to add turtles.
/// The window stays open until you call WaitForClose() or dispose the object.
/// </summary>
public sealed class TurtleWindow : IDisposable
{
    private readonly Thread _uiThread;
    private readonly ManualResetEventSlim _readyEvent = new();
    private readonly ManualResetEventSlim _closedEvent = new();
    private readonly CancellationTokenSource _cts = new();
    private TurtleCanvas? _canvas;
    private Window? _window;
    private bool _disposed;
    private double _width;
    private double _height;

    /// <summary>
    /// Gets the canvas width.
    /// </summary>
    public double Width => _width;

    /// <summary>
    /// Gets the canvas height.
    /// </summary>
    public double Height => _height;

    private TurtleWindow(int width, int height, string title)
    {
        _width = width;
        _height = height;
        
        _uiThread = new Thread(() => RunAvaloniaApp(width, height, title))
        {
            IsBackground = true,
            Name = "TurtleUI"
        };
        if (OperatingSystem.IsWindows())
        {
            _uiThread.SetApartmentState(ApartmentState.STA);
        }
        _uiThread.Start();

        // Wait for the window to be ready
        _readyEvent.Wait();
    }

    /// <summary>
    /// Creates a new turtle graphics window.
    /// </summary>
    /// <param name="width">Window width (default 800).</param>
    /// <param name="height">Window height (default 600).</param>
    /// <param name="title">Window title.</param>
    /// <returns>A disposable TurtleWindow. Use CreateTurtle() to add turtles.</returns>
    public static TurtleWindow Create(int width = 800, int height = 600, string title = "Turtle Graphics")
    {
        return new TurtleWindow(width, height, title);
    }

    /// <summary>
    /// Creates a new turtle on the canvas.
    /// </summary>
    /// <returns>A new Turtle instance.</returns>
    public Turtle CreateTurtle()
    {
        if (_canvas == null)
            throw new InvalidOperationException("Window not ready");
        
        return new Turtle(_canvas);
    }

    /// <summary>
    /// Blocks until the user closes the window.
    /// </summary>
    public void WaitForClose()
    {
        _closedEvent.Wait();
    }

    /// <summary>
    /// Closes the window programmatically.
    /// </summary>
    public void Close()
    {
        if (_window != null)
        {
            Dispatcher.UIThread.Post(() => _window.Close());
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // If window is still open, close it
        if (!_closedEvent.IsSet)
        {
            Close();
            _closedEvent.Wait();
        }

        _cts.Dispose();
        _readyEvent.Dispose();
        _closedEvent.Dispose();
    }

    private void RunAvaloniaApp(int width, int height, string title)
    {
        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .WithInterFont()
            .AfterSetup(_ => { })
            .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            });

        _canvas = new TurtleCanvas(width, height);
        
        // Set up the delay function to use Avalonia's dispatcher
        _canvas.DelayFunc = async (ms) =>
        {
            await Task.Delay(ms);
            Dispatcher.UIThread.Post(() => _canvas.InvalidateVisual());
        };

        _window = new Window
        {
            Title = title,
            Width = width,
            Height = height,
            Content = _canvas
        };

        _window.Closed += (_, _) =>
        {
            _closedEvent.Set();
            _cts.Cancel();
        };

        _window.Opened += async (_, _) =>
        {
            // Small delay to ensure canvas has valid bounds
            await Task.Delay(100);
            // Update dimensions from actual canvas size
            if (_canvas.Bounds.Width > 0)
                _width = _canvas.Bounds.Width;
            if (_canvas.Bounds.Height > 0)
                _height = _canvas.Bounds.Height;
            _canvas.SetDimensions(_width, _height);
            _readyEvent.Set();
        };

        _window.Show();
        
        try
        {
            Dispatcher.UIThread.MainLoop(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when window closes
        }
    }
}

