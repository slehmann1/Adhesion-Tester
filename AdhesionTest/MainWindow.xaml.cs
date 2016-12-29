using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdhesionTest.Properties;
using MahApps.Metro.Controls;
using System.IO;
using System.Reflection;

namespace AdhesionTest
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static Frame mainFrame;

        public delegate void errorThrown(object sender, EventArgs e);
        public static event errorThrown errorThrownEvent;

        public MainWindow()
        {
            InitializeComponent();

            //update flyout settings
            comPortBox.Text = Settings.Default.motorControllerPort.ToString();
            normalIdBox.Text = Settings.Default.normalChannelId;
            shearIdBox.Text = Settings.Default.shearChannelId;

            mainFrame = MyFrame;
            mainFrame.Navigate(new calibration());
            settingsFlyout.ClosingFinished += flyoutClosed;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ErrorHandler);
        }

        static void ErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("ERROR log saved");
            string m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MessageBox.Show("An error has occurred. The error log is located at: " + m_exePath);
            string specifier = getSpecifier(m_exePath + "\\" + "log", ".txt");
            using (StreamWriter writer = File.AppendText(m_exePath + "\\" + "log" + specifier + ".txt"))
            {
                Exception ex = (Exception)args.ExceptionObject;
                writer.WriteLine("Message :" + ex.Message + Environment.NewLine + ex.TargetSite + Environment.NewLine + "DATA" + ex.Data + Environment.NewLine + "INNER EXCEPTION: " + ex.InnerException + Environment.NewLine + "SOURCE" + ex.Source + Environment.NewLine + "TARGET SITE:" + ex.TargetSite + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
  "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
            errorThrownEvent(typeof(MainWindow), new EventArgs());

        }

        /// <summary>
        ///     returns the specifier needed at the end of the file -1, -2 ...
        /// </summary>
        /// <param name="filePath"> the file path, including the file itself, but not the extension </param>
        /// <param name="fileExtension"> the file extension </param>
        /// <returns></returns>
        private static string getSpecifier(string filePath, string fileExtension)
        {
            if (File.Exists(filePath + fileExtension))
            {
                Console.WriteLine("File exists already");
                var currentSpecifier = 1;
                while (File.Exists(filePath + "-" + currentSpecifier + fileExtension))
                {
                    currentSpecifier++;
                }
                return "-" + currentSpecifier;
            }
            return ""; //no specifier needed, don't return one
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            settingsFlyout.IsOpen = true;
        }

        //The flyout has closed, save its settings
        private void flyoutClosed(object sender, EventArgs e)
        {
            var update = false;
            if (normalIdBox.Text != Settings.Default.normalChannelId ||
                shearIdBox.Text != Settings.Default.shearChannelId)
            {
                update = true;
            }

            Settings.Default.motorControllerPort = Convert.ToInt32(comPortBox.Text);
            Settings.Default.normalChannelId = normalIdBox.Text;
            Settings.Default.shearChannelId = shearIdBox.Text;

            Settings.Default.Save();

            if (update)
            {
                var dataAcquirer = new DAQ(Settings.Default.normalChannelId, Settings.Default.shearChannelId);
                MainViewModel.setDataAcquirer(dataAcquirer);
                mainFrame.Navigate(new calibration());
            }

            Console.WriteLine(Settings.Default.shearChannelId + Settings.Default.normalChannelId);
        }

        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!sanitizeInput(e.Text))
            {
                //Invalid input
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Ensures that only numbers
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Returns true if valid</returns>
        private bool sanitizeInput(string inputString)
        {
            if (!char.IsDigit(inputString, inputString.Length - 1))
            {
                return false;
            }
            return true;
        }
    }
}