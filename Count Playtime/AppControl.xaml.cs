using Count_Playtime.logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


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

        string _saveIconPath;

        public AppControl(string appName)
        {
            InitializeComponent();
            _appName = appName;
            _saveIconPath = Path.Combine(Path.GetTempPath(), $"Count Playtime\\{_appName}.png");

            AppName.Content = appName;

            SetAppToControl(appName);
            SetupPlaytimeText();
            SetupCountTimeButton();

            SetupAppIcon();
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

        private void SetupAppIcon()
        {
            if (SaveAndLoadIconFile())
            {
                BitmapImage appIcon = GetImageSource(_saveIconPath);
                AppImage.Source = appIcon;
                AppImage.Margin = new Thickness(10);
                AppImage.Width = 32;
                AppImage.Height = 32;

                GradientColor gradientColor = GradientColor.GetBestGradient(GradientColor.GetDominantColors(appIcon));
                FirstImageColor.Color = gradientColor.FirstColor;
                SecondImageColor.Color = gradientColor.SecondColor;

                if (gradientColor.FirstColor.A == 0 || gradientColor.SecondColor.A == 0)
                    return;

                BackgroundFirstColor.Color = gradientColor.SecondColor;
                BackgroundSecondColor.Color = gradientColor.FirstColor;
            }
        }

        private bool SaveAndLoadIconFile()
        {
            if (File.Exists(_saveIconPath))
                return true;

            if(!Directory.Exists(Path.GetDirectoryName(_saveIconPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_saveIconPath));

            Process[] processes = Process.GetProcessesByName(_appName);
            if (processes.Length > 0)
            {
                System.Drawing.Image? icon = ProcessIconRetriever.GetProcessIcon(processes[0]);
                if (icon == null)
                    return false;

                icon.Save(_saveIconPath);
                return true;
            }
            else
                return false;
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


        private BitmapImage GetImageSource(string filePath)
        {
            try
            {
                // Create a new BitmapImage
                BitmapImage bitmap = new BitmapImage();

                // Initialize the BitmapImage
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Ensures the file is not locked
                bitmap.EndInit();

                return bitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
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
