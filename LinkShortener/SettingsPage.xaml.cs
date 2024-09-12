using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Reflection;

namespace LinkShortener
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            string apiUrl;
            bool saveData;

            if (!settings.TryGetValue<string>("server_url", out apiUrl))
            {
                apiUrl = "https://short.marmak.net.pl/api";
            }

            if (settings.TryGetValue<bool>("save_data", out saveData)) {
                SaveDataSwitch.IsChecked = saveData;
            }

            ServerUrlTextBox.Text = apiUrl;
        }

        private async void ChangeServerUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MainPage.IsMobileData())
            {
                using (HttpClient client = new HttpClient())
                {
                    var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

                    var version = nameHelper.Version;
                    client.DefaultRequestHeaders.Add("User-Agent", "LinkShortener for WP8 v" + version.Major + "." + version.Minor + "." + version.Build);

                    HttpResponseMessage response = await client.GetAsync(ServerUrlTextBox.Text + "/verion");

                    string resultJson = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(resultJson) || resultJson.StartsWith("<"))
                    {
                        MessageBox.Show("The server returned an incorrect value! Please verify the server adress and your Internet connection.", "LinkShortener", MessageBoxButton.OK);
                        return;
                    }
                }
            }
            MessageBox.Show("Server URL changed successfully!", "LinkShortener", MessageBoxButton.OK);
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings["server_url"] = ServerUrlTextBox.Text;
            settings.Save();
        }

        private void SaveDataSwitch_Checked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings["save_data"] = SaveDataSwitch.IsChecked;
            settings.Save();
        }
    }
}