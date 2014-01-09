#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Xsl;
using Cecer.ConsoleSplit;
using IHI.Server.Console;
using IHI.Server.Events;

#endregion

namespace IHI.Server.Install
{
    public class Installer
    {
        private readonly List<Step> _steps;


        #region Console Layout
        public ConsoleLayout ConsoleLayout
        {
            get;
            private set;
        }

        private OutLogicalConsole _installerProgressConsole;
        private OutLogicalConsole _installerTitleConsole;
        private OutLogicalConsole _installerDescriptionConsole;
        private InLogicalConsole _installerAnswerConsole;
        private OutLogicalConsole _standardOutputConsole;

        private void InitConsoleLayout()
        {
            ConsoleLayout = new ConsoleLayout();

            _installerProgressConsole = new OutLogicalConsole(13, 1, 104, 3);
            _installerTitleConsole = new OutLogicalConsole(116, 1, 2, 8);
            _installerDescriptionConsole = new OutLogicalConsole(116, 7, 2, 10);
            _installerAnswerConsole = new InLogicalConsole(116, 1, 2, 20);
            _standardOutputConsole = new OutLogicalConsole(116, 11, 2, 27);

            ConsoleLayout
                .AddLogicalConsole(_installerProgressConsole)
                .AddLogicalConsole(_installerTitleConsole)
                .AddLogicalConsole(_installerDescriptionConsole)
                .AddLogicalConsole(_installerAnswerConsole)
                .AddLogicalConsole(_standardOutputConsole)
                .Background = @"                                                                                                                        " + 
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
        #endregion

        internal Installer()
        {
            _steps = new List<Step>();

            InitConsoleLayout();
        }

        public Installer AddStepIfMissing(Step step)
        {
            if (!CoreManager.ServerCore.Config.HasValue(step.Path))
                AddStep(step);
            return this;
        }

        public Installer AddStep(Step step)
        {
            _steps.Add(step);
            return this;
        }

        private Step DrawStep(int stepNumber)
        {
            if (stepNumber < 0 || stepNumber >= _steps.Count)
                throw new IndexOutOfRangeException();

            _installerProgressConsole.Clear();
            _installerProgressConsole.Write(stepNumber+1 + "/" + _steps.Count);

            Step step = _steps[stepNumber];

            _installerTitleConsole.Clear();
            _installerTitleConsole.Write(step.Title);

            _installerDescriptionConsole.Clear();
            _installerDescriptionConsole.WriteLine(step.Description);
            if (step.Examples.Any())
            {
                _installerDescriptionConsole.WriteLine();
                _installerDescriptionConsole.WriteLine("Examples: \"" + String.Join("\",  ", step.Examples) + "\"");
            }

            return step;
        }

        private void RunStep(int stepNumber)
        {
            Step step = DrawStep(stepNumber);

            string input = _installerAnswerConsole.ReadLine();

            if (String.IsNullOrEmpty(input))
                input = step.Default;

            object[] invokeParams = { input, null};

            while (!(bool)CoreManager.ServerCore.Config.GetType().GetMethod("TryParseValue").MakeGenericMethod(step.Type).Invoke(CoreManager.ServerCore.Config, invokeParams))
            {
                CoreManager.ServerCore.ConsoleManager.Error("Install", "Bad input! \"" + input + "\" could not be parsed as type \"" + step.Type.FullName + "\"");
                input = _installerAnswerConsole.ReadLine();
                invokeParams = new object[] { input, null};
            }
            CoreManager.ServerCore.Config.SetValue(step.Path, invokeParams[1], true);
            CoreManager.ServerCore.ConsoleManager.Important("Install", "Config value set! " + step.Path + " has been set to \"" + input + "\"");
        }

        internal bool Run()
        {
            if (_steps.Count == 0)
            {
                CoreManager.ServerCore.ConsoleManager.Notice("Install", "No install tasks detected.");
                return false;
            }

            ConsoleLayout originalLayout = CoreManager.ServerCore.ConsoleManager.ConsoleLayout;
            CoreManager.ServerCore.ConsoleManager.ConsoleLayout = ConsoleLayout;

            CoreManager.ServerCore.ConsoleManager.Important("Install", "Installation tasks detected!");

            for (int stepNumber = 0; stepNumber < _steps.Count; stepNumber++)
            {
                RunStep(stepNumber);
            }

            CoreManager.ServerCore.ConsoleManager.Important("Install", "Installation finished!");
            
            CoreManager.ServerCore.ConsoleManager.ConsoleLayout = originalLayout;
            return true;
        }
    }
}