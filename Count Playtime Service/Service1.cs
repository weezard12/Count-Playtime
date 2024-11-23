using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using static Count_Playtime_Service.Service1;

namespace Count_Playtime_Service
{
    public partial class Service1 : ServiceBase
    {
        private Timer _timer;
        private static string SaveFilePath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "appTime.json");
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Initialize the timer to trigger every 1 minute (60000 milliseconds)
            _timer = new Timer(60000); // 1 minute interval
            _timer.Elapsed += OnElapsedTime;
            _timer.AutoReset = true;
            _timer.Start();
        }

        protected override void OnStop()
        {
            // Stop and dispose of the timer
            _timer.Stop();
            _timer.Dispose();
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Load the JSON file
                if (File.Exists(SaveFilePath))
                {
                    string jsonString = File.ReadAllText(SaveFilePath);
                    AppData appData = JsonSerializer.Deserialize<AppData>(jsonString);

                    // Check if each app marked with "is_counting" is currently running
                    foreach (var app in appData.Apps.Where(a => a.IsCounting))
                    {
                        bool isRunning = Process.GetProcessesByName(app.Name).Any();
                        if (isRunning)
                        {
                            app.PlaytimeMinutes += 1; // Increment playtime by 1 minute
                        }
                    }

                    // Save the updated JSON file
                    jsonString = JsonSerializer.Serialize(appData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(SaveFilePath, jsonString);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use Windows Event Viewer or any logging framework)
                EventLog.WriteEntry("Count_Playtime_Service", ex.Message, EventLogEntryType.Error);
            }
        }

        #region Json Classes
        public class AppData
        {
            [JsonPropertyName("apps")]
            public App[] Apps { get; set; }
        }

        public class App
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("playtime_minutes")]
            public int PlaytimeMinutes { get; set; }

            [JsonPropertyName("is_counting")]
            public bool IsCounting { get; set; }

            [JsonPropertyName("first_color")]
            public string FirstColor { get; set; }

            [JsonPropertyName("second_color")]
            public string SecondColor { get; set; }
        }

        #endregion
    }
}
