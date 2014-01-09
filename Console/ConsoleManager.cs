using System;
using Cecer.ConsoleSplit;
using IHI.Server.Events;

namespace IHI.Server.Console
{
    public class ConsoleManager
    {
        private ConsoleLayout _currentConsoleLayout;
        public ConsoleLayout ConsoleLayout
        {
            get
            {
                return _currentConsoleLayout;
            }
            set
            {
                if (ConsoleContainer.Instance == null)
                {
                    InitializeConsoleLayoutSupport();
                    BasicConsole = false;
                }

                if (_currentConsoleLayout == value)
                    return;

                ConsoleContainer.Instance.SilentAllConsoles();

                _currentConsoleLayout = value;
                if (_currentConsoleLayout != null)
                {
                    _currentConsoleLayout.DrawBackground();
                    _currentConsoleLayout.Hidden = false;
                }
            }
        }

        #region Property: BasicConsole
        private bool _basicConsole = true;
        /// <summary>
        /// 
        /// </summary>
        public bool BasicConsole
        {
            get
            {
                return _basicConsole;
            }
            set
            {
                _basicConsole = value;
            }
        }
        #endregion

        internal ConsoleManager()
        {
        }

        public void InitializeConsoleLayoutSupport()
        {
            if (ConsoleContainer.Instance == null)
                ConsoleContainer.Initialize(120, 40);
        }

        #region Standard Out
        public ConsoleManager Debug(string channel, string message)
        {
            FireEvent(new ConsoleOutputEventArgs(ConsoleOutputLevel.Debug, channel, message));
            return this;
        }
        public ConsoleManager Notice(string channel, string message)
        {
            FireEvent(new ConsoleOutputEventArgs(ConsoleOutputLevel.Notice, channel, message));
            return this;
        }
        public ConsoleManager Important(string channel, string message)
        {
            FireEvent(new ConsoleOutputEventArgs(ConsoleOutputLevel.Important, channel, message));
            return this;
        }
        public ConsoleManager Warning(string channel, string message)
        {
            FireEvent(new ConsoleOutputEventArgs(ConsoleOutputLevel.Warning, channel, message));
            return this;
        }
        public ConsoleManager Error(string channel, string message)
        {
            FireEvent(new ConsoleOutputEventArgs(ConsoleOutputLevel.Error, channel, message));
            return this;
        }

        private void FireEvent(ConsoleOutputEventArgs eventArgs)
        {
            if (BasicConsole && _currentConsoleLayout == null)
            {
                string level;
                switch (eventArgs.Level)
                {
                    case ConsoleOutputLevel.Debug:
                        {
                            level = "Debug";
                            break;
                        }
                    case ConsoleOutputLevel.Notice:
                        {
                            level = "Notice";
                            break;
                        }
                    case ConsoleOutputLevel.Important:
                        {
                            level = "Important";
                            break;
                        }
                    case ConsoleOutputLevel.Warning:
                        {
                            level = "Warning";
                            break;
                        }
                    case ConsoleOutputLevel.Error:
                        {
                            level = "Error";
                            break;
                        }
                    default:
                        {
                            level = "?????";
                            break;
                        }
                }
                System.Console.WriteLine("[{0}] [{1}] {2}", level, eventArgs.Channel, eventArgs.Message);
            }

            CoreManager.ServerCore.OfficalEventFirer.Fire("stdout:before", eventArgs);
            if (!eventArgs.IsCancelled)
                CoreManager.ServerCore.OfficalEventFirer.Fire("stdout:after", eventArgs);
        }
        #endregion
    }
}
