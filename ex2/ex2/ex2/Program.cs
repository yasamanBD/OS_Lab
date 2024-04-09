using System;
using System.Diagnostics;
using System.Management;

class Program
{
    static void Main(string[] args)
    {
        using (var watcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")))
        {
            watcher.EventArrived += (sender, eventArgs) =>
            {
                Console.WriteLine("USB device connected!");
                Process.Start("mspaint");
            };
            watcher.Start();

            Console.WriteLine("Listening for USB device connection events. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
