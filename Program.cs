using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.InteropServices;

class InterceptKeys
{
    //WH => 
    //LL => low level


    //13 Global LowLevel keyboard hook number
    private const int WindowsHook_KEYBOARD_LowLevel = 13;
    private const int WindowsHook_Mouse_LowLevel = 14;


    //https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
    //Keystroke messages

    private const int WM_XBUTTONUP = 0x020C;
    private const int WM_XBUTTONDOWN = 0x020B;



    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Main()
    {
        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WindowsHook_Mouse_LowLevel, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_XBUTTONUP || wParam == (IntPtr) WM_XBUTTONDOWN))
        {

            var ps = PowerShell.Create();
            ps.AddCommand("Disable-NetAdapter")
                   .AddParameter("-Name", "Wi-Fi 2")
                   .Invoke();
            
            int vkCode = Marshal.ReadInt32(lParam);
            Console.WriteLine((Keys)vkCode);
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}