using System;
using System.Diagnostics;
using AltiumTestConsole.Const;
using AltiumTestConsole.Seeder;

Console.WriteLine("Hello, Altium!");
Seeder seeder = new ();
Seeder3 seeder3 = new ();
Thread memoryMonitorThread = new Thread(MemoryMonitor);
memoryMonitorThread.Start();

// Fast, but not fastest.
// seeder.MakeFileSync_SB();            // Total size: 5368709140, time elapsed: 00:00:48.7677049


// Faster, but can be faster.
// seeder.MakeFileSync_BytesDict();     // Total size: 5368709135, time elapsed: 00:00:36.5458310

seeder3.MakeFileSync_Optimized();


// Fastest, but not paralleled.
seeder.MakeFileSync_Optimized();     // Total size: 5368709136, time elapsed: 00:00:34.6026331
// Остановка потока мониторинга памяти
memoryMonitorThread.Interrupt();
memoryMonitorThread.Join();


static void MemoryMonitor()
{
    try
    {
        while (true)
        {
            double memoryUsed = GetUsedMemoryInMb();
            double managedMemoryUsed = GetManagedMemoryInMb();
            Console.WriteLine($"Ram size: {memoryUsed} MB, Managed Memory: {managedMemoryUsed} MB");
            Thread.Sleep(APPLICATION_CONST.SEEDER_PARAMS.CONSOLE_LOG_INTERVAL);
        }
    }
    catch (ThreadInterruptedException)
    {
        // Завершение потока
    }
}

static double GetUsedMemoryInMb()
{
    using (Process currentProcess = Process.GetCurrentProcess())
    {
        return currentProcess.WorkingSet64 / 1024.0 / 1024.0;
    }
}

static double GetManagedMemoryInMb()
{
    return GC.GetTotalMemory(false) / 1024.0 / 1024.0;
}