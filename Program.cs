using System.Diagnostics;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

class InterceptKeys
{
    //WH => 
    //LL => low level
    //Process:
    //     Provides access to local and remote processes and enables you to start and stop
    //     local system processes.

    //13 Global LowLevel keyboard hook number
    // private const int WindowsHook_KEYBOARD_LowLevel = 13;
    private const int WindowsHook_Mouse_LowLevel = 14;

    private const string WIFI_INTERFACE_NAME = "Wi-Fi 2";
    private const string ETHERNET_INTERFACE_NAME = "Ethernet";
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
        var a = Process.GetProcesses();
        foreach (var item in a)
        {


        }
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WindowsHook_Mouse_LowLevel, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_XBUTTONUP || wParam == (IntPtr)WM_XBUTTONDOWN))
        {

            try
            {
                if (IsInterfaceEnabled(ETHERNET_INTERFACE_NAME))
                {
                    DisableAdapter(ETHERNET_INTERFACE_NAME);
                    EnableAdapter(WIFI_INTERFACE_NAME);
                }
                else
                {
                    EnableAdapter(ETHERNET_INTERFACE_NAME);
                    DisableAdapter(WIFI_INTERFACE_NAME);
                }
            }
            catch (System.Exception e)
            {
                    Console.WriteLine(e.Message);
            }
            int vkCode = Marshal.ReadInt32(lParam);
            Console.WriteLine((Keys)vkCode);
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }


    static void EnableAdapter(string interfaceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" enable");
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.UseShellExecute = true;
        psi.Verb = "runas";
        Process p = new Process();
        p.StartInfo = psi;
        p.Start();

    }

    static void DisableAdapter(string interfaceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.UseShellExecute = true;
        psi.Verb = "runas";
        Process p = new Process();
        p.StartInfo = psi;
        p.Start();

    }


    public static bool IsInterfaceEnabled(string name)
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        return adapters.ToList().Exists(e => e.Name == name);
    }




    //Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the system for certain types of events.
    // These events are associated either with a specific thread or with all threads in the same desktop as the calling thread.
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    //Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    //Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    //Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
