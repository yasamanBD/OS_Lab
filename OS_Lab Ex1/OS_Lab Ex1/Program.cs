using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OS_LabEx1
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr ExitStatus;
            public IntPtr PebBaseAddress;
            public IntPtr AffinityMask;
            public IntPtr BasePriority;
            public UIntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        static void Main(string[] args)
        {
            bool start = true;
            while (start)
            {
                int select = showMenu();
                Process[] prList = Process.GetProcesses();
                switch (select)
                {
                    case 1:
                        Console.WriteLine("Enter process name please");
                        String pName = Console.ReadLine();
                        try
                        {
                            Process.Start(pName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error starting process: {ex.Message}");
                        }
                        break;
                    case 2:
                        foreach (Process p in prList)
                        {
                            Console.WriteLine($"{p.Id}\t{p.ProcessName}");
                        }
                        break;
                    case 3:
                        Console.WriteLine("Enter process ID please");
                        int pid = int.Parse(Console.ReadLine());
                        foreach (Process p in prList)
                        {
                            if (p.Id == pid)
                                p.Kill();
                        }
                        break;
                    case 4:
                        Console.WriteLine("Enter process ID please");
                        int id = int.Parse(Console.ReadLine());
                        foreach (Process p in prList)
                        {
                            if (p.Id == id)
                            {
                                IntPtr handle = GetProcessHandle(p.Id);
                                if (handle != IntPtr.Zero)
                                {
                                    Process rootProcess = GetRootProcess(handle);
                                    if (rootProcess != null)
                                    {
                                        Console.WriteLine("Root Process Name: " + rootProcess.ProcessName);
                                        Console.WriteLine("Root Process ID: " + rootProcess.Id);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Root Process not found.");
                                    }
                                    CloseHandle(handle);
                                }
                                else
                                {
                                    Console.WriteLine("Failed to obtain process handle.");
                                }
                                break;
                            }
                        }
                        break;
                    case 5:
                        start = false;
                        break;
                }
            }
        }

        static int showMenu()
        {
            Console.WriteLine("1. Start a process");
            Console.WriteLine("2. List of running processes");
            Console.WriteLine("3. Kill a process");
            Console.WriteLine("4. Parent of a process");
            Console.WriteLine("5. Exit");
            return int.Parse(Console.ReadLine());
        }
        static IntPtr GetProcessHandle(int processId)
        {
            Process process = Process.GetProcessById(processId);
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = OpenProcess(ProcessAccessFlags.QueryInformation, false, processId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening process handle: {ex.Message}");
            }
            return handle;
        }

        static Process GetRootProcess(IntPtr handle)
        {
            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);
            if (status == 0)
            {
                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting root process: {ex.Message}");
                }
            }
            return null;
        }
    }
}
