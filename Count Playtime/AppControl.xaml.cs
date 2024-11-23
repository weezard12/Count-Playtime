using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Windows.Shapes;

namespace Count_Playtime
{
    /// <summary>
    /// Interaction logic for AppControl.xaml
    /// </summary>
    public partial class AppControl : UserControl
    {
        private static string FilePath = System.IO.Path.Combine(AppContext.BaseDirectory,"appTime.json");
        public static AppData CurrentAppData;

        App _appToControl;
        string _appName;

        public AppControl(string appName)
        {
            InitializeComponent();
            _appName = appName;

            AppName.Content = appName;

            SetAppToControl(appName);
            SetupPlaytimeText();
            SetupCountTimeButton();
        }

        private void SetAppToControl(string appName)
        {
            if (appName == null)
                return;

            if (CurrentAppData != null)
            {
                if (CurrentAppData.Apps != null)
                {
                    _appToControl = CurrentAppData.Apps.FirstOrDefault(a => a.Name == appName);
                }
            }
        }

        private void SetupPlaytimeText()
        {

            if (_appToControl == null)
                PlaytimeText.Content = "No Playtime Saved";
            else
            {
                if (_appToControl.PlaytimeMinutes == 0)
                    PlaytimeText.Content = "Just Started Counting";
                else
                    PlaytimeText.Content = (_appToControl.PlaytimeMinutes < 60) ? (_appToControl.PlaytimeMinutes + "m") : (_appToControl.PlaytimeMinutes / 60 + "h");
            }
            
        }
        private void SetupCountTimeButton()
        {
            if (_appToControl == null)
            {
                CountTime.Content = "Count Time";
            }
            else
            {
                if (_appToControl.IsCounting)
                    CountTime.Content = "Stop Counting";
                else
                    CountTime.Content = "Count Time";
            }
        }
        private void CountTime_Click(object sender, RoutedEventArgs e)
        {
            if (_appToControl == null)
            {
                _appToControl = new App() {Name = _appName, PlaytimeMinutes = 0, IsCounting = true, FirstColor = "#FF5733" , SecondColor = "#33C5FF" };
                List<App> apps = CurrentAppData.Apps.ToList();
                apps.Add(_appToControl);
                CurrentAppData.Apps = apps.ToArray();

                SetupCountTimeButton();
                SetupPlaytimeText();
            }
            else    
                _appToControl.IsCounting = !_appToControl.IsCounting; //switch only if its not the first time counting the app time
            
            //gets and sets the current json state
            string jsonString = File.ReadAllText(FilePath);
            jsonString = JsonSerializer.Serialize(CurrentAppData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, jsonString);

            SetupCountTimeButton();
        }

        #region Json Classes & Methods
        public static void UpdateAppData()
        {
            if (!File.Exists(FilePath))
                File.WriteAllText(FilePath, @"{""apps"": []}");
            string jsonString = File.ReadAllText(FilePath);
            CurrentAppData = JsonSerializer.Deserialize<AppData>(jsonString);
        }
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
