using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Cecer.ConsoleSplit;
using IHI.Server.Console;
using IHI.Server.Install;
using IHI.Server.Plugins;
using IHI.Server.Configuration;
using IHI.Server.Events;
using IHI.Server.Rooms;
using IHI.Server.Rooms.Figure;
using IHI.Server.Database;
using IHI.Server.Network.WebAdmin;
using IHI.Server.Permissions;
using IHI.Server.Habbos;
using IHI.Server.Network;

using System.Collections.Generic;
using IHI.Server.Network.GameSockets;
using System.ComponentModel;
using System.Threading;

namespace IHI.Server
{
    public class ServerCore
    {
        #region Fields
        #region Field: _bootInProgressLocker
        private readonly object _bootInProgressLocker = new object();
        #endregion
        
        #endregion

        #region Properties
        #region Property: MySqlConnectionProvider
        public MySqlConnectionProvider MySqlConnectionProvider
        {
            get;
            private set;
        }
        #endregion
        #region Property: Config
        public XmlConfig Config
        {
            get;
            private set;
        }
        #endregion
        #region Property: GameSocketManager
        public Dictionary<string, GameSocketManager> GameSocketManagers
        {
            get;
            private set;
        }
        #endregion
        #region Property: HabboDistributor
        public HabboDistributor HabboDistributor
        {
            get;
            private set;
        }
        #endregion
        #region Property: HabboFigureFactory
        public HabboFigureFactory HabboFigureFactory
        {
            get;
            private set;
        }
        #endregion
        #region Property: PermissionDistributor
        public PermissionDistributor PermissionDistributor
        {
            get;
            private set;
        }
        #endregion
        #region Property: ConsoleManager
        public ConsoleManager ConsoleManager
        {
            get;
            private set;
        }
        #endregion

        #region Property: StringLocale
        /// <summary>
        /// 
        /// </summary>
        public StringLocale StringLocale
        {
            get;
            private set;
        }
        #endregion

        #region Property: Installer
        /// <summary>
        /// 
        /// </summary>
        public Installer Installer
        {
            get;
            private set;
        }
        #endregion

        public WebAdminManager WebAdminManager
        {
            get;
            private set;
        }

        #region Property: EventManager
        public EventManager EventManager
        {
            get;
            private set;
        }
        internal EventFirer OfficalEventFirer
        {
            get;
            private set;
        }
        #endregion
        #region Property: RoomDistributor
        public RoomDistributor RoomDistributor
        {
            get;
            private set;
        }
        #endregion
        #region Property: PluginManager
        public PluginManager PluginManager
        {
            get;
            private set;
        }
        #endregion
        #endregion

        #region Methods
        #region Method: ServerCore (Constructor)
        public ServerCore()
        {
            EventManager = new EventManager();
            OfficalEventFirer = EventManager.NewEventFirer(null);

            StringLocale = new StringLocale();
            PluginManager = new PluginManager();
            GameSocketManagers = new Dictionary<string, GameSocketManager>();
        }
        #endregion
        #region Method: Boot
        internal void Boot()
        {
            lock (_bootInProgressLocker)
            {
                ConsoleManager = new ConsoleManager();

                StringLocale.SetDefaults();

                #region Ensure Directory Structure
                new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "dumps")).EnsureExists();
                new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "plugins")).EnsureExists();
                new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "libs")).EnsureExists();
                #endregion
                
                Installer = new Installer();

                BootTaskLoadConfig();
                BootTaskLoadPlugins();

                BootTaskRunInstaller();

                if (!BootTaskConnectMySql())
                    return;

                Task.WaitAll(new[]
                                 {
                                     Task.Factory.StartNew(BootTaskPreparePermissions),
                                     Task.Factory.StartNew(BootTaskPrepareFigures),
                                     Task.Factory.StartNew(BootTaskPrepareHabbos),
                                     Task.Factory.StartNew(BootTaskPrepareRooms)
                                 });

                BootTaskStartWebAdmin();

                BootTaskStartPlugins();

                ConsoleManager.Important("Core", StringLocale.GetString("CORE:BOOT_COMPLETE"));

                EventManager.WeakBind<ConsoleInputEventArgs>("stdin:after", ProcessTempInput);

                new Thread(ConsoleManager.ReadInput)
                {
                    IsBackground = true,
                    Name = "IHI-STDIN"
                }.Start();

                System.Console.Beep(500, 250);
            }
        }
        #endregion

        private void ProcessTempInput(ConsoleInputEventArgs eventArgs)
        {
            // TODO: Proper commands

            if (eventArgs.Message != null && eventArgs.Message.Trim() == "shutdown")
                EntryPoint.Exit(true, -1, 5000, true);
        }

        #region Method: BootTaskLoadConfig
        private void BootTaskLoadConfig()
        {
            string configPath = Environment.GetEnvironmentVariable("BLUEDOT_CONFIG_PATH");

            ConsoleManager.Notice("Boot", StringLocale.GetString("CORE:BOOT_LOADING_CONFIG_AT") + configPath);
            Config = new XmlConfig(configPath);

            ConsoleManager.Notice("Boot", StringLocale.GetString("CORE:BOOT_INSTALL_CHECKING"));
            PrepareInstall();
        }
        #endregion
        #region Method: BootTaskRunInstaller
        private void BootTaskRunInstaller()
        {
            Installer.Run();
        }
        #endregion

        #region Method: BootTaskConnectMySql
        private bool BootTaskConnectMySql()
        {
            ConsoleManager.Notice("MySQL", StringLocale.GetString("CORE:BOOT_MYSQL_PREPARE"));
            MySqlConnectionProvider = new MySqlConnectionProvider
            {
                Host = Config.GetValue("mysql:host", "localhost").Value,
                Port = Config.GetValue<ushort>("mysql:port", 3306).Value,
                User = Config.GetValue("mysql:username", "ihi").Value,
                Password = Config.GetValue("mysql:password", "").Value,
                Database = Config.GetValue("mysql:database", "ihi").Value,
                MinimumPoolSize = Config.GetValue<uint>("mysql:minpoolsize", 1).Value,
                MaximumPoolSize = Config.GetValue<uint>("mysql:maxpoolsize", 5).Value
            };
            MySqlConnectionProvider.HelperGetAction<int>("SELECT 1", null); // Testing the connection to the MySQL server

            ConsoleManager.Notice("MySQL", StringLocale.GetString("CORE:BOOT_MYSQL_READY"));
            return true;
        }
        #endregion
        #region Method: BootTaskLoadPlugins
        private void BootTaskLoadPlugins()
        {
            List<Task> taskList = new List<Task>();
            ConsoleManager.Notice("Plugin Manager", StringLocale.GetString("CORE:BOOT_PLUGINS_LOADING"));
            foreach (string path in PluginManager.GetAllPotentialPluginPaths())
            {
                taskList.Add(Task.Factory.StartNew(() => { PluginManager.LoadPluginAtPath(path); }));
            }
            Task.WaitAll(taskList.ToArray());
            ConsoleManager.Notice("Plugin Manager", StringLocale.GetString("CORE:BOOT_PLUGINS_LOADED"));
        }
        #endregion
        #region Method: BootTaskStartPlugins
        private void BootTaskStartPlugins()
        {
            List<Task> taskList = new List<Task>();
            ConsoleManager.Notice("Plugin Manager", StringLocale.GetString("CORE:BOOT_PLUGINS_STARTING"));
            foreach (Plugin plugin in PluginManager.GetLoadedPlugins())
            {
                taskList.Add(Task.Factory.StartNew(() => { PluginManager.StartPlugin(plugin); }));
            }
            Task.WaitAll(taskList.ToArray());
            ConsoleManager.Notice("Plugin Manager", StringLocale.GetString("CORE:BOOT_PLUGINS_STARTED"));
        }
        #endregion


        #region Method: BootTaskPrepareFigures
        public void BootTaskPrepareFigures()
        {
            ConsoleManager.Notice("Habbo Figure Factory", StringLocale.GetString("CORE:BOOT_FIGURES_PREPARE"));
            HabboFigureFactory = new HabboFigureFactory();
            ConsoleManager.Notice("Habbo Figure Factory", StringLocale.GetString("CORE:BOOT_FIGURES_READY"));
        }
        #endregion
        #region Method: BootTaskPreparePermissions
        public void BootTaskPreparePermissions()
        {
            ConsoleManager.Notice("Permission Distributor", StringLocale.GetString("CORE:BOOT_PERMISSIONS_PREPARE"));
            PermissionDistributor = new PermissionDistributor();
            ConsoleManager.Notice("Permission Distributor", StringLocale.GetString("CORE:BOOT_PERMISSIONS_READY"));
        }
        #endregion
        #region Method: BootTaskPrepareHabbos
        public void BootTaskPrepareHabbos()
        {
            ConsoleManager.Notice("Habbo Distributor", StringLocale.GetString("CORE:BOOT_HABBODISTRIBUTOR_PREPARE"));
            HabboDistributor = new HabboDistributor();
            ConsoleManager.Notice("Habbo Distributor", StringLocale.GetString("CORE:BOOT_HABBODISTRIBUTOR_READY"));
        }
        #endregion
        #region Method: BootTaskPrepareRooms
        public void BootTaskPrepareRooms()
        {
            ConsoleManager.Notice("Room Distributor", StringLocale.GetString("CORE:BOOT_ROOMDISTRIBUTOR_PREPARE"));
            RoomDistributor = new RoomDistributor();
            ConsoleManager.Notice("Room Distributor", StringLocale.GetString("CORE:BOOT_ROOMDISTRIBUTOR_READY"));
        }
        #endregion

        #region Method: NewGameSocketManager
        public GameSocketManager NewGameSocketManager(string socketManagerName, IPEndPoint ipEndpoint, GameSocketProtocol protocol)
        {
            return NewGameSocketManager(socketManagerName, ipEndpoint.Address, (ushort)ipEndpoint.Port, protocol);
        }
        public GameSocketManager NewGameSocketManager(string socketManagerName, ushort port, GameSocketProtocol protocol)
        {
            return NewGameSocketManager(socketManagerName, IPAddress.Any, port, protocol);
        }
        public GameSocketManager NewGameSocketManager(string socketManagerName, IPAddress ipAddress, ushort port, GameSocketProtocol protocol)
        {
            GameSocketManager gameSocketManager = new GameSocketManager
            {
                Address = IPAddress.Any,
                Port = port,
                Protocol = protocol
            };

            GameSocketManagerEventArgs eventArgs = new GameSocketManagerEventArgs(gameSocketManager, socketManagerName);
            OfficalEventFirer.Fire("gamesocket_manager_added:before", eventArgs);
            if (eventArgs.IsCancelled)
                return null;
            GameSocketManagers.Add(socketManagerName, gameSocketManager);
            OfficalEventFirer.Fire("gamesocket_manager_added:after", eventArgs);

            return gameSocketManager;
        }
        #endregion

        #region Method: BootTaskStartWebAdmin
        public void BootTaskStartWebAdmin()
        {
            ConsoleManager.Notice("Web Admin", StringLocale.GetString("CORE:BOOT_WEBADMIN_PREPARE"));
            WebAdminManager = new WebAdminManager(Config.GetValue<ushort>("webadmin:port", 14480).Value);
            ConsoleManager.Notice("Web Admin", StringLocale.GetString("CORE:BOOT_WEBADMIN_READY"));
        }
        #endregion

        #region Method: SubBootStepPrepareInstall
        private void PrepareInstall()
        {
            // Yes, add to the Installer Core.
            Installer
                .AddStepIfMissing(new Step("mysql:host", typeof (IPAddress), "MySQL Host",
                    "This is the IP address used to connect to the MySQL server.", "127.0.0.1")
                    .AddExample("127.0.0.1")
                    .AddExample("192.168.1.80")
                    .AddExample("123.123.123.123"))
                .AddStepIfMissing(new Step("mysql:port", typeof (ushort), "MySQL Port",
                    "This is the port used to connect to the MySQL server.", "3306")
                    .AddExample("3306"))
                .AddStepIfMissing(new Step("mysql:username", typeof(string), "MySQL Username",
                    "This is the username used to authenticate with the MySQL server.", "ihi")
                    .AddExample("ihi")
                    .AddExample("chris")
                    .AddExample("root (INSECURE! NOT RECOMMENDED!"))
                .AddStepIfMissing(new Step("mysql:password", typeof(string), "MySQL Password",
                    "This is the password used to authenticate with the MySQL server.", ""))
                .AddStepIfMissing(new Step("mysql:database", typeof(string), "MySQL Database",
                    "This is the database used for IHI.", "ihi")
                    .AddExample("ihi")
                    .AddExample("ihidb"))
                .AddStepIfMissing(new Step("mysql:minpool", typeof(int), "MySQL Minimum Pool Size",
                    "This is the minimum amount of MySQL connections to maintain in the connection pool.", "1")
                    .AddExample("1")
                    .AddExample("5"))
                .AddStepIfMissing(new Step("mysql:maxpool", typeof(int), "MySQL Maximum Pool Size",
                    "This is the minimum amount of MySQL connections to maintain in the connection pool.", "5")
                    .AddExample("1")
                    .AddExample("5"))
                .AddStepIfMissing(new Step("webadmin:port", typeof(ushort), "WebAdmin Port",
                    "This is the port to bind the WebAdmin listener.", "14480")
                    .AddExample("14480")
                    .AddExample("80")
                    .AddExample("8080"));
        }
        #endregion
        
        #region Method: Shutdown
        public void Shutdown(bool terminate = true)
        {
            lock (_bootInProgressLocker)
            {
                if (ConsoleManager == null)
                    return;

                ConsoleManager.Warning("Core", "Shutting down IHI!");
                
                if (CoreManager.ServerCore.OfficalEventFirer == null)
                    return;

                IHIEventArgs eventArgs = new IHIEventArgs();
                CoreManager.ServerCore.OfficalEventFirer.Fire("shutdown:before", eventArgs);
                CoreManager.ServerCore.OfficalEventFirer.Fire("shutdown:after", eventArgs);
                
                if (WebAdminManager == null)
                    return;
                
                WebAdminManager.Stop();

                Config = null;

                System.Console.Beep(4000, 100);
                System.Console.Beep(3500, 100);
                System.Console.Beep(3000, 100);
                System.Console.Beep(2500, 100);
                System.Console.Beep(2000, 100);
                System.Console.Beep(1500, 100);
                System.Console.Beep(1000, 100);
            }
        }
        #endregion

        #region Method: DumpException
        public string DumpException(Exception exception)
        {
            return EntryPoint.DumpException(exception, "exception");
        }
        #endregion

        #endregion
    }
}
