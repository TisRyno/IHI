using System;
using System.Collections.Generic;
using Cecer.ConsoleSplit;

namespace IHI.Server.Console
{
    public class ConsoleLayout
    {
        private List<LogicalConsole> _consoles;

        #region Property: Hidden
        /// <summary>
        /// 
        /// </summary>
        internal bool Hidden
        {
            set
            {
                foreach (LogicalConsole console in _consoles)
                {
                    console.Hidden = value;
                    console.Draw();
                }
            }
        }
        #endregion

        public void DrawBackground()
        {
            System.Console.Clear();
            System.Console.SetCursorPosition(0, 0);
            BackgroundDrawer();
        }

        public Action BackgroundDrawer
        {
            private get;
            set;
        }

        #region Method: ConsoleLayout (Constructor)
        public ConsoleLayout()
        {
            CoreManager.ServerCore.ConsoleManager.InitializeConsoleLayoutSupport();
            _consoles = new List<LogicalConsole>();
        }
        #endregion

        #region Method: AddLogicalConsole
        public ConsoleLayout AddLogicalConsole(LogicalConsole console)
        {
            _consoles.Add(console);
            ConsoleContainer.Instance.AddLogicalConsole(console);
            return this;
        }
        #endregion
    }
}