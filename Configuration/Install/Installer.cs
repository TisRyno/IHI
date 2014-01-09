#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Xsl;
using Cecer.ConsoleSplit;
using IHI.Server.Configuration.Install;
using IHI.Server.Console;
using IHI.Server.Events;

#endregion

namespace IHI.Server.Install
{
    public class Installer
    {
        private readonly List<Step> _steps;

        private InstallerLayout _installerLayout;

        internal Installer()
        {
            _steps = new List<Step>();

            if (Environment.GetEnvironmentVariable("IHI_BASIC_INSTALLER") == "true")
                _installerLayout = new InstallerLayout();
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

            _installerLayout.ProgressConsole.Clear();
            _installerLayout.ProgressConsole.Write(stepNumber + 1 + "/" + _steps.Count);

            Step step = _steps[stepNumber];

            _installerLayout.TitleConsole.Clear();
            _installerLayout.TitleConsole.Write(step.Title);

            _installerLayout.DescriptionConsole.Clear();
            _installerLayout.DescriptionConsole.WriteLine(step.Description);
            if (step.Examples.Any())
            {
                _installerLayout.DescriptionConsole.WriteLine();
                _installerLayout.DescriptionConsole.WriteLine("Examples: \"" + String.Join("\",  ", step.Examples) + "\"");
            }

            return step;
        }

        private void RunStep(int stepNumber, bool justDefault = false)
        {
            Step step;
            string input;
            if (!justDefault)
            {
                step = DrawStep(stepNumber);

                input = _installerLayout.AnswerConsole.ReadLine();
                if (String.IsNullOrEmpty(input))
                    input = step.Default;
            }
            else
            {
                step = _steps[stepNumber];
                input = step.Default;
            }

            object[] invokeParams = { input, null };

            while (!(bool)CoreManager.ServerCore.Config.GetType().GetMethod("TryParseValue").MakeGenericMethod(step.Type).Invoke(CoreManager.ServerCore.Config, invokeParams))
            {
                CoreManager.ServerCore.ConsoleManager.Error("Install", "Bad input! \"" + input + "\" could not be parsed as type \"" + step.Type.FullName + "\"");
                input = _installerLayout.AnswerConsole.ReadLine();
                invokeParams = new object[] { input, null };
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

            if (_installerLayout == null)
            {
                CoreManager.ServerCore.ConsoleManager.Notice("Install", "IHI_BASIC_INSTALLER is set. Using default values.");
                CoreManager.ServerCore.ConsoleManager.Important("Install", "Installation tasks detected!");

                for (int stepNumber = 0; stepNumber < _steps.Count; stepNumber++)
                {
                    RunStep(stepNumber, true);
                }
                CoreManager.ServerCore.ConsoleManager.Important("Install", "Installation finished!");

                return true;
            }

            ConsoleLayout originalLayout = CoreManager.ServerCore.ConsoleManager.ConsoleLayout;
            CoreManager.ServerCore.ConsoleManager.ConsoleLayout = _installerLayout.ConsoleLayout;

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