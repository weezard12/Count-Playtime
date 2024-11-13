using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
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

namespace Count_Playtime
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        private bool _doesServiceExsist;
        public SetupWindow(bool doesServiceExsist)
        {
            InitializeComponent();
            this._doesServiceExsist = doesServiceExsist;

            if (_doesServiceExsist)
                ActionButton.Content = "Enable";

            WarningText.Content += doesServiceExsist ? "Enabled" : "Installed";

        }
        public static bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("For any further steps, close this app and OPEN IT AS AN ADMINISTRATOR!");
                return;
            }

            if (_doesServiceExsist)
            {
                StartService("Count Playtime");
                return;
            }
                

            RunInstallUtil(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe", @"""C:\Program Files\RGS\Count Playtime\Count Playtime Service\Count Playtime Service.exe""");

        }
        static void StartService(string serviceName)
        {
            try
            {
                // Create a ServiceController object for the specified service name
                ServiceController service = new ServiceController(serviceName);

                // Check if the service is already running
                if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.Paused)
                {
                    // Start the service if it is stopped or paused
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running); // Wait until the service is running
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting service {serviceName}: {ex.Message}");
            }
        }
        static void RunInstallUtil(string installUtilPath, string servicePath)
        {
            try
            {
                // Prepare the ProcessStartInfo
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = installUtilPath,
                    Arguments = servicePath,  
                    UseShellExecute = false,  // Don't use shell execution
                    CreateNoWindow = true,
                    Verb = "runas"            // Use runas to ensure administrator privileges
                };

                // Start the process
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();  // Wait for the command to complete

                if (process.ExitCode == 0)
                {
                    MessageBox.Show("Sucsess");
                }
                else
                {
                    MessageBox.Show("Failed to install service. Exit code: " + process.ExitCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
