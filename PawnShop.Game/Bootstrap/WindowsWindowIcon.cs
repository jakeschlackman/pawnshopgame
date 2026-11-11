using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PawnShop.Game.Bootstrap;

// Application-window integration belongs to the executable host, not a gameplay feature.
internal sealed class WindowsWindowIcon : IDisposable
{
    private const uint SetIconMessage = 0x0080, ImageIcon = 1, LoadFromFile = 0x0010;
    private const int IconWidth = 11, IconHeight = 12, SmallIconWidth = 49, SmallIconHeight = 50;
    private readonly IntPtr window;
    private IntPtr smallIcon, largeIcon, previousSmallIcon, previousLargeIcon;

    public WindowsWindowIcon(IntPtr sdlWindow, string iconPath)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();

        // DesktopGL's GameWindow.Handle is SDL_Window*, not a Windows HWND.
        SDL_GetVersion(out var version);
        var info = new WindowManagerInfo { Version = version };
        if (SDL_GetWindowWMInfo(sdlWindow, ref info) == 0)
            throw new InvalidOperationException($"Cannot get the Windows window: {Marshal.PtrToStringUTF8(SDL_GetError())}");
        if (info.Subsystem != 1 || info.Window == IntPtr.Zero) // SDL_SYSWM_WINDOWS
            throw new InvalidOperationException("SDL did not supply a Windows window handle.");
        window = info.Window;

        uint dpi = GetDpiForWindow(window);
        smallIcon = LoadIcon(iconPath, SmallIconWidth, SmallIconHeight, dpi);
        try { largeIcon = LoadIcon(iconPath, IconWidth, IconHeight, dpi); }
        catch
        {
            DestroyIcon(smallIcon);
            smallIcon = IntPtr.Zero;
            throw;
        }

        // Caption/taskbar and Alt+Tab use separate sizes from the same ICO artwork.
        previousSmallIcon = SendMessageW(window, SetIconMessage, IntPtr.Zero, smallIcon);
        previousLargeIcon = SendMessageW(window, SetIconMessage, new IntPtr(1), largeIcon);
    }

    private static IntPtr LoadIcon(string path, int widthMetric, int heightMetric, uint dpi)
    {
        var icon = LoadImageW(IntPtr.Zero, path, ImageIcon,
            GetSystemMetricsForDpi(widthMetric, dpi), GetSystemMetricsForDpi(heightMetric, dpi), LoadFromFile);
        if (icon == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot load application icon '{path}'.");
        return icon;
    }

    public void Dispose()
    {
        if (smallIcon == IntPtr.Zero) return;
        // Restore SDL's icons before releasing ours; SDL retains ownership of its handles.
        SendMessageW(window, SetIconMessage, IntPtr.Zero, previousSmallIcon);
        SendMessageW(window, SetIconMessage, new IntPtr(1), previousLargeIcon);
        DestroyIcon(smallIcon);
        DestroyIcon(largeIcon);
        smallIcon = largeIcon = IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SdlVersion
    {
        public byte Major, Minor, Patch;
    }

    // SDL2 ABI: version at 0, subsystem at 4, then a 64-byte platform union at 8.
    [StructLayout(LayoutKind.Explicit, Size = 72)]
    private struct WindowManagerInfo
    {
        [FieldOffset(0)] public SdlVersion Version;
        [FieldOffset(4)] public int Subsystem;
        [FieldOffset(8)] public IntPtr Window;
    }

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SDL_GetVersion(out SdlVersion version);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_GetWindowWMInfo(IntPtr window, ref WindowManagerInfo info);

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetError();

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImageW(IntPtr instance, string name, uint type, int width, int height, uint flags);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr SendMessageW(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern uint GetDpiForWindow(IntPtr window);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int GetSystemMetricsForDpi(int index, uint dpi);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr icon);
}
