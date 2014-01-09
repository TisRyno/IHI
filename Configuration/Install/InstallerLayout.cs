using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cecer.ConsoleSplit;
using IHI.Server.Console;
using IHI.Server.Events;

namespace IHI.Server.Configuration.Install
{
    internal class InstallerLayout
    {
        public ConsoleLayout ConsoleLayout
        {
            get;
            private set;
        }

        public OutLogicalConsole ProgressConsole
        {
            get;
            private set;
        }
        public OutLogicalConsole TitleConsole
        {
            get;
            private set;
        }
        public OutLogicalConsole DescriptionConsole
        {
            get;
            private set;
        }
        public OutLogicalConsole StandardOutputConsole
        {
            get;
            private set;
        }
        public InLogicalConsole AnswerConsole
        {
            get;
            private set;
        }

        internal InstallerLayout()
        {
            ConsoleLayout = new ConsoleLayout();

            ProgressConsole = new OutLogicalConsole(13, 1, 104, 3);
            TitleConsole = new OutLogicalConsole(116, 1, 2, 8);
            DescriptionConsole = new OutLogicalConsole(116, 7, 2, 10);
            AnswerConsole = new InLogicalConsole(116, 1, 2, 20);
            StandardOutputConsole = new OutLogicalConsole(116, 11, 2, 27);

            ConsoleLayout
                .AddLogicalConsole(ProgressConsole)
                .AddLogicalConsole(TitleConsole)
                .AddLogicalConsole(DescriptionConsole)
                .AddLogicalConsole(AnswerConsole)
                .AddLogicalConsole(StandardOutputConsole)
                .BackgroundDrawer = DrawBackground;

            CoreManager.ServerCore.EventManager.StrongBind<ConsoleOutputEventArgs>("stdout:after", PrintStandardOutEvents);
        }

        private void DrawBackground()
        {
            System.Console.Write(@"                                                                                                                        " +
                              @"    ######  ##  ##  ######  ##  ##  ######  ######  ######  ##      ##      ######  #####                               " +
                              @"      ##    ##  ##    ##    ### ##  ##        ##    ##  ##  ##      ##      ##      ##  ##  /-------------------------\ " +
                              @"      ##    ######    ##    ######  ######    ##    ######  ##      ##      ####    #####   | Question:               | " +
                              @"      ##    ##  ##    ##    ## ###      ##    ##    ##  ##  ##      ##      ##      ## ##   \-------------------------/ " +
                              @"    ######  ##  ##  ######  ##  ##  ######    ##    ##  ##  ######  ######  ######  ##  ##                              " +
                              @"                                                                                                                        " +
                              @" /--------------------------------------------------------------------------------------------------------------------\ " +
                              @" |                                                                                                                    | " +
                              @" >--------------------------------------------------------------------------------------------------------------------< " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" \--------------------------------------------------------------------------------------------------------------------/ " +
                              @"                                                                                                                        " +
                              @" /--------------------------------------------------------------------------------------------------------------------\ " +
                              @" |                                                                                                                    | " +
                              @" \--------------------------------------------------------------------------------------------------------------------/ " +
                              @"                                                                                                                        " +
                              @"                                                                                                                        " +
                              @"                                                                                                                        " +
                              @"                                                                                                                        " +
                              @" /--------------------------------------------------------------------------------------------------------------------\ " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" |                                                                                                                    | " +
                              @" \--------------------------------------------------------------------------------------------------------------------/ " +
                              @"                                                                                                                       ");
        }

        private void PrintStandardOutEvents(ConsoleOutputEventArgs eventArgs)
        {
            lock (StandardOutputConsole)
            {
                switch (eventArgs.Level)
                {
                    case ConsoleOutputLevel.Debug:
                        {
                            StandardOutputConsole.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        }
                    case ConsoleOutputLevel.Notice:
                        {
                            StandardOutputConsole.ForegroundColor = ConsoleColor.Gray;
                            break;
                        }
                    case ConsoleOutputLevel.Important:
                        {
                            StandardOutputConsole.ForegroundColor = ConsoleColor.Green;
                            break;
                        }
                    case ConsoleOutputLevel.Warning:
                        {
                            StandardOutputConsole.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        }
                    case ConsoleOutputLevel.Error:
                        {
                            StandardOutputConsole.ForegroundColor = ConsoleColor.Red;
                            break;
                        }
                }
                StandardOutputConsole.WriteLine("[" + eventArgs.Channel + "]" + eventArgs.Message);
            }
        }
    }
}
