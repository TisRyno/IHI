using System;
using Cecer.ConsoleSplit;
using IHI.Server.Events;

namespace IHI.Server.Console
{
    public class ConsoleManager
    {
        #region Default Console Layout
        public ConsoleLayout DefaultConsoleLayout
        {
            get;
            private set;
        }

        private OutLogicalConsole _standardOutputConsole;
        private InLogicalConsole _inputConsole;
        private OutLogicalConsole _inputReplyConsole;
        private OutLogicalConsole _statsOutputConsole;

        private void InitDefaultConsoleLayout()
        {
            DefaultConsoleLayout = new ConsoleLayout();

            _standardOutputConsole = new OutLogicalConsole(65, 37, 53, 1);
            _inputConsole = new InLogicalConsole(50, 3, 2, 35);
            _inputReplyConsole = new OutLogicalConsole(50, 26, 2, 8);
            _statsOutputConsole = new OutLogicalConsole(21, 5, 31, 2);

            DefaultConsoleLayout
                .AddLogicalConsole(_standardOutputConsole)
                .AddLogicalConsole(_inputConsole)
                .AddLogicalConsole(_inputReplyConsole)
                .AddLogicalConsole(_statsOutputConsole)
                .Background = @"                                                    /-----------------------------------------------------------------\ " +
                              @"    ######  ##  ##  ######    /---------------------<                                                                 | " +
                              @"      ##    ##  ##    ##      |                     |                                                                 | " +
                              @"      ##    ######    ##      |                     |                                                                 | " +
                              @"      ##    ##  ##    ##      |                     |                                                                 | " +
                              @"    ######  ##  ##  ######    |                     |                                                                 | " +
                              @"                              |                     |                                                                 | " +
                              @" /----------------------------^---------------------<                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" >--------------------------------------------------<                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" |                                                  |                                                                 | " +
                              @" \--------------------------------------------------^-----------------------------------------------------------------/ " +
                              @"                                                                                                                       ";



            CoreManager.ServerCore.EventManager.StrongBind<ConsoleOutputEventArgs>("stdout:after", eventArgs => 
            {
                lock (_standardOutputConsole)
                {
                    switch (eventArgs.Level)
                    {
                        case ConsoleOutputLevel.Debug:
                            {
                                _standardOutputConsole.ForegroundColor = ConsoleColor.DarkGray;
                                break;
                            }
                        case ConsoleOutputLevel.Notice:
                            {
                                _standardOutputConsole.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }
                        case ConsoleOutputLevel.Important:
                            {
                                _standardOutputConsole.ForegroundColor = ConsoleColor.Green;
                                break;
                            }
                        case ConsoleOutputLevel.Warning:
                            {
                                _standardOutputConsole.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            }
                        case ConsoleOutputLevel.Error:
                            {
                                _standardOutputConsole.ForegroundColor = ConsoleColor.Red;
                                break;
                            }
                    }
                    _standardOutputConsole.WriteLine("[" + eventArgs.Channel + "]" + eventArgs.Message);
                }
            });
        }

        internal void ReadInput()
        {
            while (true)
            {
                string inputLine = _inputConsole.ReadLine();

                ConsoleInputEventArgs eventArgs = new ConsoleInputEventArgs(inputLine);

                CoreManager.ServerCore.OfficalEventFirer.Fire("stdin:before", eventArgs);
                if (!eventArgs.IsCancelled)
                    CoreManager.ServerCore.OfficalEventFirer.Fire("stdin:after", eventArgs);
            }
        }

        #endregion

        private ConsoleLayout _currentConsoleLayout;
        public ConsoleLayout ConsoleLayout
        {
            get
            {
                return _currentConsoleLayout;
            }
            set
            {
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

        internal ConsoleManager()
        {
            ConsoleContainer.Initialize(120, 40);

            InitDefaultConsoleLayout();
            ConsoleLayout = DefaultConsoleLayout;
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
            CoreManager.ServerCore.OfficalEventFirer.Fire("stdout:before", eventArgs);
            if (!eventArgs.IsCancelled)
                CoreManager.ServerCore.OfficalEventFirer.Fire("stdout:after", eventArgs);
        }
        #endregion
    }
}
