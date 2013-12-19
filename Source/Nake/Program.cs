﻿using System;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Nake
{
    public class Program
    {
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {
            CatchUnhandledExceptions();
            
            BootstrapRoslyn();
            
            StartApplication(args);
        }

        static void CatchUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var e = (Exception) args.ExceptionObject;

                Log.Error(e);
                Exit.Fail(e);
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                var e = args.Exception;
                args.SetObserved();

                foreach (var inner in e.Flatten().InnerExceptions)
                    Log.Error(inner);

                Exit.Fail(e);
            };
        }

        static void BootstrapRoslyn()
        {
            Roslyn.Bootstrap();
        }

        static void StartApplication(string[] args)
        {
            try
            {
                var options = Options.Parse(args);
                new Application(options).Start();
            }
            catch (OptionParseException e)
            {
                Log.Error(e.Message);
                Options.PrintUsage();
                Exit.Fail(e);
            }
            catch (TaskInvocationException e)
            {
                Log.Error(e.SourceException);
                Exit.Fail(e);
            }
        }
    }
}
