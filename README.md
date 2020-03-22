# Avalonia.Veldrid
Integration of AvaloniaUI and Veldrid

![Preview of Avalonia XAML rendered in Veldrid scene in VR](https://github.com/gleblebedev/Avalonia.Veldrid/raw/master/docs/images/Avalonia.Veldrid.VR.gif)

## Setting up AvaloniaUI

```c#
_veldridScreen = new VeldridScreenStub() { Size = new FramebufferSize(_window.Width, _window.Height) };
_veldridContext = new AvaloniaVeldridContext(_graphicsDevice, screenImpl: _veldridScreen);
PortableAppBuilder.Configure<App>()
    .UsePortablePlatfrom()
    .With(_veldridContext)
    .UseSkia()
    .UseManagedSystemDialogs()
    .SetupWithoutStarting();
```

You can start with no graphics device and set it later via_veldridContext.SetGraphicsDevice(...)

In a main application loop you should execute the following:
```c#
_veldridContext.ProcessMainThreadQueue();
```

## Creating a window

You can create a normal AvaloniaUI window just by creating an instance of window class.
```c#
var mainWindow = new MainWindow();
mainWindow.Show();
```

If you maximise the window it is then rendered as a fullscreen quad.

You can use the following helper functions to manipulate window properties:

```c#
VeldridProperty.TrySetWorldTransform(_window, Matrix4x4.CreateTranslation(0.15f, 0.1f, 0) * transform);

VeldridProperty.TrySetDpi(_window, 96 * 4);
```


## Rendering windows

```c#
//Thread safe window collection view
WindowsCollectionView _windows;

...

//Somewhere in a render code...
_veldridContext.Projection = _projection;
_veldridContext.View = _view;
_windows.Fetch(_veldridContext);
foreach (var window in _windows)
{
    window.Render(_commandList);
}
```

## Processing input

### Touch

You can translate your platform specific input into touch events of AvaloniaUI:

```c#
//Create touch adapter
_touch = _veldridContext.CreateTouchAdapter();

...

//Update touch position
_touch.Move(..)

...

//Destroy touch adapter, also equivalent of ending (not canceling) the touch event
_touch.Dispose();
```

There are two ways to move touch:
```c#
/// <summary>
/// Move touch as a ray in 3D space.
/// </summary>
/// <param name="from">Origin of the ray.</param>
/// <param name="to">Target of the ray (not direction!).</param>
public void Move(Vector3 @from, Vector3 to);

        /// <summary>
/// Move touch as a point in space with a certain tolerance distance.
/// </summary>
/// <param name="worldPosition">Position of the touch (tip of a finger) in world space.</param>
/// <param name="toleranceInMeters">Distance to window at which touch is registered.</param>
public void Move(Vector3 worldPosition, float toleranceInMeters = 0.03f);
```

### Mouse

todo...

### Keyboard

todo...
