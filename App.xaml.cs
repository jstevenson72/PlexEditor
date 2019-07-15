using Plex_Database_Editor.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Plex_Database_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // UI Thread Exceptions
            DispatcherUnhandledException += Application_DispatcherUnhandledException;

            // Non-UI Thread Exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task Exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Locator.MainVM.Message = $"Task Exception: {e.Exception.Message}";            
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Locator.MainVM.Message = $"Domain Exception: {e.ExceptionObject}";
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Locator.MainVM.Message = $"Unhandled Exception: {e.Exception.Message}";
            e.Handled = true;
        }
    }
}
