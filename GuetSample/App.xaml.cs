using MessengerLight;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MediaService BGMService => MediaService.GetMediaService();
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            BGMService.AddSong(new Uri("pack://siteoforigin:,,,/Resources/Audios/katura-AnanRyoko.mp3"));
            Messenger.Default.Register<MediaParameters>(this, onCallPlayMedia);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Messenger.Default.Unregister<MediaParameters>(this);
            base.OnExit(e);
        }

        private void onCallPlayMedia(MediaParameters parameters)
        {
            if(parameters.Action == PlayAction.Play)
            {
                if(!BGMService.IsPlaying) BGMService.Play();
            }else if(parameters.Action == PlayAction.Stop)
            {
                BGMService.Stop();
            }
        }
        
        #region Unexpected exception handler

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
#if DEBUG
            System.Diagnostics.Debug.WriteLine(sender.ToString() + "\n" + e.ExceptionObject);
#endif
            LogExceptionInfo(exception, "AppDomain.CurrentDomain.UnhandledException");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(e.Exception.TargetSite);
#endif
            LogExceptionInfo(e.Exception, "AppDomain.DispatcherUnhandledException");
        }

        private void LogExceptionInfo(Exception exception, string typeName = "Undefined Exception")
        {
            DisposeOnUnhandledException();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------- Begin  ---------");
            sb.AppendLine("--------------------------");
            sb.AppendLine();
            sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"));
            sb.AppendLine();
            sb.AppendLine("--------------------------");
            sb.AppendLine();
            sb.AppendLine(typeName);
            sb.AppendLine();
            sb.AppendLine("[0].TargetSite");
            sb.AppendLine(exception.TargetSite.ToString());
            sb.AppendLine();
            sb.AppendLine("[1].StackTrace");
            sb.AppendLine(exception.StackTrace);
            sb.AppendLine();
            sb.AppendLine("[2].Source");
            sb.AppendLine(exception.Source);
            sb.AppendLine();
            sb.AppendLine("[3].Message");
            sb.AppendLine(exception.Message);
            sb.AppendLine();
            sb.AppendLine("[4].HResult");
            sb.AppendLine(exception.HResult.ToString());
            sb.AppendLine();
            if (exception.InnerException != null)
            {
                sb.AppendLine("--------------");
                sb.AppendLine("InnerException");
                sb.AppendLine("--------------");
                sb.AppendLine();
                sb.AppendLine("[5.0].TargetSite");
                sb.AppendLine(exception.InnerException.TargetSite.ToString());
                sb.AppendLine();
                sb.AppendLine("[5.1].StackTrace");
                sb.AppendLine(exception.InnerException.StackTrace);
                sb.AppendLine();
                sb.AppendLine("[5.2].Source");
                sb.AppendLine(exception.InnerException.Source);
                sb.AppendLine();
                sb.AppendLine("[5.3].Message");
                sb.AppendLine(exception.InnerException.Message);
                sb.AppendLine();
                sb.AppendLine("[5.4].HResult");
                sb.AppendLine(exception.InnerException.HResult.ToString());
                sb.AppendLine();
            }
            sb.AppendLine("--------- End  ---------");
            sb.AppendLine();

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string log = Path.GetDirectoryName(path) + "\\log.txt";
            using (StreamWriter sw = new StreamWriter(log, true, Encoding.UTF8))
            {
                sw.Write(sb.ToString());
            }
        }

        private void DisposeOnUnhandledException()
        {
        }

        #endregion

    }
}
