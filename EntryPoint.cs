#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Cecer.ConsoleSplit;
using IHI.Server.Useful;

#endregion

namespace IHI.Server
{
    internal static class EntryPoint
    {
        // ReSharper disable InconsistentNaming
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        private const UInt32 SC_CLOSE = 0xF060;
        private const UInt32 MF_ENABLED = 0x00000000;
        private const UInt32 MF_GRAYED = 0x00000001;
        // ReSharper restore InconsistentNaming


        private static ConsoleColor entryForeground;
        private static ConsoleColor entryBackground;

        internal static void Main(string[] arguments)
        {
            entryForeground = System.Console.ForegroundColor;
            entryBackground = System.Console.BackgroundColor;

            System.Console.Title = "Bluedot Habbo Server";

            System.Console.WriteLine("Bluedot Habbo Server - Preparing...");
            
            #region Exit management
            System.Console.WriteLine("Disabling close window button...");
            // Disable close button to prevent unsafe closing.
            SetExitButtonEnabled(false);

            System.Console.WriteLine("Binding shutdown to CTRL + C and CTRL + Break...");
            // Reassign CTRL+C and CTRL+BREAK to safely shutdown.
            System.Console.CancelKeyPress += HandleShutdownKey;
            System.Console.TreatControlCAsInput = false;
            SetShutdownKeyMode(ShutdownKeyMode.AttemptShutdown);
            #endregion


            Thread.CurrentThread.Name = "BLUEDOT-EntryThread";

            System.Console.WriteLine("Setting up packaged reference loading...");
            // Allows embedded resources to be loaded.
            AppDomain.CurrentDomain.AssemblyResolve += LoadPackagedReferences;

            System.Console.WriteLine("Setting up fatal exception handler (BSOD style)...");
            // Bluescreen in the event of a fatal unhandled exception.
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;


            string configFile = Path.Combine(Environment.CurrentDirectory, "config.xml");

            System.Console.WriteLine("Parsing command line arguments...");
            Regex nameValueRegex = new Regex("^--(?<name>[\\w-]+)=(?<value>.+)$");

            foreach (string argument in arguments)
            {
                Match nameValueMatch = nameValueRegex.Match(argument);
                string name = nameValueMatch.Groups["name"].Value;
                string value = nameValueMatch.Groups["value"].Value;

                System.Console.WriteLine("  " + name + " - " + value);

                switch (name)
                {
                    case "config-file":
                        {
                            configFile = value;
                            break;
                        }
                    default:
                        {
                            System.Console.WriteLine("Unknown command line argument (" + name + "=" + value + ")");
                            break;
                        }
                }
            }
            System.Console.WriteLine("Config location: " + configFile);
            Environment.SetEnvironmentVariable("BLUEDOT_CONFIG_PATH", configFile);

            System.Console.WriteLine("Preparing server core...");
            CoreManager.InitialiseServerCore();

            System.Console.WriteLine("Starting server core...");

            try
            {
                CoreManager.ServerCore.Boot();
            }
            catch (Exception e)
            {
                Crash(e);
            }
        }

        private static Assembly LoadPackagedReferences(object sender, ResolveEventArgs args)
        {
            return LoadPackagedDll(new AssemblyName(args.Name).Name);
        }
        public static Assembly LoadPackagedDll(string name)
        {
            String resourceName = "IHI.Server.Reference_Packaging." + name + ".dll";

            using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
            {
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }


        private static void PrintStopError(Exception exception)
        {
            System.Console.SetWindowPosition(0, 0);
            System.Console.SetBufferSize(System.Console.LargestWindowWidth - 3, System.Console.LargestWindowHeight - 2);
            System.Console.SetWindowSize(System.Console.LargestWindowWidth - 3, System.Console.LargestWindowHeight - 2);

            System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Clear();

            System.Console.WriteLine("[===[ IHI STOP ERROR ]===]");
            System.Console.WriteLine("An unhandled exception has caused IHI to close!");
            System.Console.WriteLine("Details of the exception are below.");
            System.Console.WriteLine();
            System.Console.WriteLine("Time (UTC): " + DateTime.UtcNow);

            System.Console.WriteLine();
            System.Console.WriteLine("Exception Assembly: " + Assembly.GetCallingAssembly().FullName);
            System.Console.WriteLine("Exception Thread: " + Thread.CurrentThread.Name);
            System.Console.WriteLine("Exception Type: " + exception.GetType().FullName);
            System.Console.WriteLine("Exception Message: " + exception.Message);

            System.Console.Write("Has Inner Exception: ");
            System.Console.WriteLine(exception.InnerException == null ? "NO" : "YES");

            System.Console.WriteLine("Stack Trace:");
            System.Console.WriteLine("  " + exception.StackTrace.Replace(Environment.NewLine, Environment.NewLine + "  "));

            System.Console.WriteLine();
            System.Console.WriteLine("Loaded Assemblies: ");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Assembly.GetCallingAssembly() == assembly)
                    System.Console.WriteLine("!! " + assembly.FullName);
                else
                    System.Console.WriteLine("   " + assembly.FullName);
            }
        }


        private static int _dumpCounter = 0;
        internal static string DumpException(Exception exception, string filenamePrefix)
        {
            DateTime time = DateTime.UtcNow;
            string timeString = time.ToString("o");

            string filename = filenamePrefix + "-" + timeString.Replace(':', ';') + "-" + (_dumpCounter++).ToString("#####") + ".xml";
            string path = Path.Combine(Environment.CurrentDirectory, "dumps", filename);

            ExceptionXElement exceptionElement = new ExceptionXElement(exception);
            exceptionElement.Add(new XElement("Time", timeString));
            exceptionElement.Add(new XElement("Assemblies", AppDomain.CurrentDomain.GetAssemblies().Select(assembly => new XElement("Assembly", assembly.FullName))));

            XDocument document = new XDocument();
            document.Add(exceptionElement);
            document.Save(path);

            return path;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
                return;

            Exception exception = e.ExceptionObject as Exception;

            Crash(exception);
        }

        internal static void Crash(Exception exception)
        {
            if (ConsoleContainer.Instance != null)
                ConsoleContainer.Instance.SilentAllConsoles();


            PrintStopError(exception);
            DumpException(exception, "crash");

            System.Console.WriteLine();
            System.Console.WriteLine("Shutting down IHI...");

            Exit(true, 2);
        }

        private static void KillConsoleSplit()
        {
            if (ConsoleContainer.Instance != null)
                ConsoleContainer.Instance.SilentAllConsoles();

            System.Console.ForegroundColor = entryForeground;
            System.Console.BackgroundColor = entryBackground;
            System.Console.Clear();
            System.Console.SetCursorPosition(0, 0);
        }

        internal static void Exit(bool attemptShutdown, int exitCode = -1, int timeout = 5000, bool closeAfterExit = false)
        {

            // If the server hasn't started, just exit.
            if (attemptShutdown && CoreManager.ServerCore != null)
                SafeExit(exitCode, timeout, closeAfterExit);
            else
                ExitWithExitCode(exitCode);
        }

        private static void SafeExit(int exitCode, int timeout, bool closeAfterExit)
        {
            bool shutdownFinished = false;
            
            new Thread(() =>
            {
                Thread.Sleep(timeout);

                if (shutdownFinished)
                    return;

                KillConsoleSplit();
                System.Console.WriteLine("IHI doesn't appear to be fully down safely (it might just be taking a while). Close button enabled!");
                System.Console.WriteLine("Press CTRL+C or the close button to force IHI to exit.");

                System.Console.CursorVisible = false;
                SetShutdownKeyMode(ShutdownKeyMode.JustExit, exitCode);
                SetExitButtonEnabled(true);
            })
            {
                IsBackground = true,
                Name = "IHI-ShutdownExiter"
            }.Start();

            CoreManager.ServerCore.Shutdown();

            if (exitCode == -1)
                exitCode = 0;
            shutdownFinished = true;

            if (closeAfterExit)
                ExitWithExitCode(exitCode);

            new Thread(() => Thread.Sleep(Timeout.Infinite)).Start(); // Add a thread to keep the application open!

            KillConsoleSplit();
            System.Console.WriteLine("IHI has now fully shutdown safely. Close button enabled!");
            System.Console.WriteLine("Press CTRL+C or the close button to close IHI.");

            System.Console.CursorVisible = false;
            SetShutdownKeyMode(ShutdownKeyMode.JustExit, exitCode);
            SetExitButtonEnabled(true);

            new Thread(() => Thread.Sleep(Timeout.Infinite)).Start();
        }

        private static void ExitWithExitCode(int exitCode)
        {
            if (exitCode == -1)
                exitCode = 1;

            Environment.Exit(exitCode);
        }

        private static void SetExitButtonEnabled(bool enable)
        {
            if (enable)
            {
                IntPtr current = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                EnableMenuItem(GetSystemMenu(current, false), SC_CLOSE, MF_ENABLED);
            }
            else
            {
                IntPtr current = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                EnableMenuItem(GetSystemMenu(current, false), SC_CLOSE, MF_GRAYED);
            }
        }

        private static ShutdownKeyMode _shutdownKeyMode = ShutdownKeyMode.JustExit;
        private static int _exitCode;
        private static void HandleShutdownKey(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                e.Cancel = true;

            switch (_shutdownKeyMode)
            {
                case ShutdownKeyMode.Ignore:
                    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                        e.Cancel = true;
                    break;
                case ShutdownKeyMode.AttemptShutdown:
                    Exit(true);
                    break;
                case ShutdownKeyMode.JustExit:
                {
                    KillConsoleSplit();
                    System.Console.WriteLine();
                    System.Console.WriteLine("Closing...");
                    ExitWithExitCode(_exitCode);
                    break;
                }
            }
        }
        private static void SetShutdownKeyMode(ShutdownKeyMode shutdownKeyMode, int exitCode = 0)
        {
            _shutdownKeyMode = shutdownKeyMode;
            _exitCode = exitCode;
        }

        private enum ShutdownKeyMode
        {
            Ignore,
            AttemptShutdown,
            JustExit
        }
    }
}