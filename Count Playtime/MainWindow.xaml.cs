﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Count_Playtime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private string _appPath;
        private int _usageTime; // Minutes
        private const string ConfigFilePath = "appUsage.json";

        public MainWindow()
        {
            InitializeComponent();

        }

        private void SearchUpdated(object sender, TextChangedEventArgs e)
        {
            Process[] searchPIDs = GetRunningProcessesWithFilter(AppSearch.Text);
            AppsPanel.Children.Clear();
            foreach (var process in searchPIDs)
            {
                AppsPanel.Children.Add(new AppControl(process.ProcessName));
            }

        }

        public static Process[] GetRunningProcessesWithFilter(string filter)
        {
            Process[] processes = Process.GetProcesses();

            var filtered = processes
                .Where(p => p.ProcessName.ToLower().Contains(filter.ToLower()))
                .ToArray();

            return filtered;
        }
        static string GetProcessNameByPid(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                // In case of an error (e.g., process not found, access denied), return null
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }
        public static bool IsProgramRunning(string programName)
        {
            // Create a new process to run the tasklist command
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"@echo off\r\nfor /f \"tokens=2\" %%A in ('tasklist /fi \"imagename eq *a*\" /fo csv /nh') do (\r\n    echo %%~A\r\n)\r\n";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Start the process
            process.Start();

            // Read the output of the tasklist command
            string output = process.StandardOutput.ReadToEnd();

            // Close the process
            process.WaitForExit();
            process.Close();

            // Check if the program name appears in the output
            return output.Contains(programName);
        }
        
    }
}