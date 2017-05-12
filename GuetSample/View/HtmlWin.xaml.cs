using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for HtmlWin.xaml
    /// </summary>
    public partial class HtmlWin : Window
    {
        private string htmlValue;
        public HtmlWin(string htmlValue)
        {
            InitializeComponent();
            this.htmlValue = htmlValue;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.Unloaded += OnThisUnloaded;
            contentRoot.ManipulationBoundaryFeedback += StopFeedback;
            this.browser.NavigateToString(ConvertExtendedASCII(htmlValue));
        }

        private void OnThisUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= OnThisUnloaded;
            contentRoot.ManipulationBoundaryFeedback -= StopFeedback;
        }

        //Disable the ManipulationBoundaryFeedback event to prevent window shake.
        private void StopFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private static string ConvertExtendedASCII(string HTML)
        {
            string retVal = "";
            char[] s = HTML.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    retVal += "&#" + Convert.ToInt32(c) + ";";
                else
                    retVal += c;
            }

            return retVal;
        }
    }
}
