using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

class MouseClicksInterceptor
{
    private const int WindowsHook_Mouse_LowLevel = 14;
    private const string WIFI_INTERFACE_NAME = "Wi-Fi 2";
    private const string ETHERNET_INTERFACE_NAME = "Ethernet";
    private const int WM_XBUTTONUP = 0x020C;
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
        using (Process currentProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = currentProcess.MainModule)
        {
            return SetWindowsHookEx(WindowsHook_Mouse_LowLevel, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_XBUTTONUP)
        {
            SwitchNetworks();
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }


    private static void SwitchNetworks()
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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    static void EnableAdapter(string interfaceName)
    {
        string command = "interface set interface \"" + interfaceName + "\" enable";

        RunNetSHCommand(command);
    }


    static void DisableAdapter(string interfaceName)
    {
        string command = "interface set interface \"" + interfaceName + "\" disable";
        RunNetSHCommand(command);
    }


    static void RunNetSHCommand(string command)
    {
        ProcessStartInfo psi = new ProcessStartInfo("netsh", command)
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            Verb = "runas"
        };


        Process process = new Process
        {
            StartInfo = psi
        };

        process.Start();
    }

    public static bool IsInterfaceEnabled(string name)
    {
        //Given the name of the network interfac,
        // this function returns  the interface is enabled or not
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
