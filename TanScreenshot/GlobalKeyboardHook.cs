using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;

class GlobalKeyboardHookEventArgs : HandledEventArgs
{
    public GlobalKeyboardHook.KeyboardState KeyboardState { get; }
    public GlobalKeyboardHook.LowLevelKeyboardInputEvent KeyboardData { get; }
    public bool Control { get; }
    public bool Alt { get; }
    public bool Shift { get; }

    public GlobalKeyboardHookEventArgs(
        GlobalKeyboardHook.LowLevelKeyboardInputEvent keyboardData,
        GlobalKeyboardHook.KeyboardState keyboardState,
        bool control,
        bool alt,
        bool shift)
    {
        KeyboardData = keyboardData;
        KeyboardState = keyboardState;
        Control = control;
        Alt = alt;
        Shift = shift;
    }
}

// Based on https://gist.github.com/Stasonix
class GlobalKeyboardHook : IDisposable
{
    public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

    public GlobalKeyboardHook(Keys[] registeredKeys = null)
    {
        RegisteredKeys = registeredKeys;
        _windowsHookHandle = IntPtr.Zero;
        _hookProc = LowLevelKeyboardProc;

        _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, IntPtr.Zero, 0);
        if (_windowsHookHandle == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _windowsHookHandle != IntPtr.Zero)
        {
            if (!UnhookWindowsHookEx(_windowsHookHandle))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
            _windowsHookHandle = IntPtr.Zero;
        }
    }

    ~GlobalKeyboardHook()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private IntPtr _windowsHookHandle;
    private HookProc _hookProc;

    delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    [DllImport("USER32", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

    [DllImport("USER32", SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hHook);

    [DllImport("USER32", SetLastError = true)]
    static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        public int VirtualCode;
        public Keys Key { get { return (Keys)VirtualCode; } }
        public int HardwareScanCode;
        public int Flags;
        public int TimeStamp;
        public IntPtr AdditionalInformation;
    }

    public const int WH_KEYBOARD_LL = 13;

    public enum KeyboardState
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }

    public Keys[] RegisteredKeys;

    public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        bool fEatKeyStroke = false;

        if (nCode >= 0 && KeyboardPressed != null)
        {
            var keyboardData = (LowLevelKeyboardInputEvent)Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
            var state = (KeyboardState)wParam;
            var control = (GetKeyState((int)Keys.LControlKey) & 0x8000) != 0 || (GetKeyState((int)Keys.RControlKey) & 0x8000) != 0;
            var alt = (GetKeyState((int)Keys.LMenu) & 0x8000) != 0 || (GetKeyState((int)Keys.RMenu) & 0x8000) != 0;
            var shift = (GetKeyState((int)Keys.LShiftKey) & 0x8000) != 0 || (GetKeyState((int)Keys.RShiftKey) & 0x8000) != 0;
            var eventArgs = new GlobalKeyboardHookEventArgs(keyboardData, state, control, alt, shift);
            KeyboardPressed(this, eventArgs);
            fEatKeyStroke = eventArgs.Handled;
        }

        return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }
}