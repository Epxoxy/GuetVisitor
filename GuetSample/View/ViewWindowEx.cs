using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GuetSample
{
    public class ViewWindowEx
    {
        public static void EnsureActive(Window win)
        {
            if (win.WindowState == WindowState.Minimized)
                win.WindowState = WindowState.Normal;
            if (!win.IsActive)
                win.Activate();
        }

        public static void CallDialogForReceiver(DialogContent content, Window container)
        {
            container.Dispatcher.BeginInvoke(new Action(() =>
            {
                ViewWindowEx.EnsureActive(container);
                var dialog = new Epxoxy.Controls.MessageDialog(container)
                {
                    Title = content.Title,
                    Content = content.Content,
                };
                System.Media.SystemSounds.Beep.Play();
                if (content.PlayMusic)
                {
                    MessengerLight.Messenger.Default.Send(new MediaParameters()
                    {
                        Action = PlayAction.Play
                    });
                    System.ComponentModel.CancelEventHandler handler = null;
                    handler = (sender, e) =>
                    {
                        dialog.Closing -= handler;
                        MessengerLight.Messenger.Default.Send(new MediaParameters()
                        {
                            Action = PlayAction.Stop
                        });
                    };
                    dialog.Closing += handler;
                }
                dialog.ShowDialog();
            }));
        }

    }
}
