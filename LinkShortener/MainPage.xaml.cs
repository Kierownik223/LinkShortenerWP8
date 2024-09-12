using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Microsoft.Phone.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using System.Reflection;
using Microsoft.Phone.Tasks;

namespace LinkShortener
{
    public partial class MainPage : PhoneApplicationPage
    {
        public string apiUrl = "https://short.marmak.net.pl/api";
        HttpClient client;

        public MainPage()
        {
            InitializeComponent();

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

            var version = nameHelper.Version;

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "LinkShortener for WP8 v" + version.Major + "." + version.Minor + "." + version.Build);

            if (!settings.TryGetValue<string>("server_url", out apiUrl))
            {
                apiUrl = "https://short.marmak.net.pl/api";
            }

            if (App.ShareURI != null)
            {
                UrlTextBox.Text = App.ShareURI;
                ShortenButton_Click(ShortenButton, null);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!IsMobileData())
                RefreshStats();
        }

        public static bool IsMobileData()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            bool saveData;
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                if (DeviceNetworkInformation.IsWiFiEnabled)
                {
                    return false;
                }
                else if (settings.TryGetValue<bool>("save_data", out saveData))
                {
                    return saveData;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private async void RefreshStats()
        {
            ShowProgressIndicator(true, "Refreshing stats...");
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl + "/stats");

                string resultJson = await response.Content.ReadAsStringAsync();
                JObject apiResponse = JObject.Parse(resultJson);
                string links = apiResponse.Value<string>("linkCount");
                string clicks = apiResponse.Value<string>("totalClicks");

                LabelStats.Text = "Links in database: " + links + "\nTotal clicks: " + clicks;
            }
            catch (Exception)
            {
                LabelStats.Text = "Links in database: N/A\nTotal clicks: N/A";
            }
            finally
            {
                ShowProgressIndicator(false);
            }
        }

        private void LabelStats_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RefreshStats();
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TextBlock item = (TextBlock)sender;
            if (item.Tag != null)
            {
                Clipboard.SetText((string)item.Tag);
                MessageBox.Show("Link copied to clipboard", "LinkShortener", MessageBoxButton.OK);
            }
        }

        private void ShowProgressIndicator(bool isVisible, string text = "")
        {
            ProgressIndicator progressIndicator = new ProgressIndicator
            {
                IsIndeterminate = true,
                IsVisible = isVisible,
                Text = text
            };

            SystemTray.SetProgressIndicator(this, progressIndicator);
        }

        private async void ShortenButton_Click(object sender, RoutedEventArgs e)
        {

            string url = UrlTextBox.Text;

            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                MessageBox.Show("No Internet connection is available. Please check your connection.", "LinkShortener", MessageBoxButton.OK);
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            ShowProgressIndicator(true, "Shortening link...");

            try
            {
                StringContent content = new StringContent("{\"url\": \"" + url + "\"}", System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl + "/url", content);

                string resultJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(resultJson) || resultJson.StartsWith("<"))
                {
                    MessageBox.Show("The server returned an incorrect value! Please verify the server adress and your Internet connection.", "LinkShortener", MessageBoxButton.OK);
                    return;
                }
                JObject apiResponse = JObject.Parse(resultJson);

                string error = apiResponse.Value<string>("error");

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Error", MessageBoxButton.OK);
                }
                else
                {
                    LabelShortnedUrl.Tag = apiResponse.Value<string>("link");
                    LabelShortnedUrl.Text = "Your shortened link has been generated:\r\n" + apiResponse.Value<string>("link");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
            finally
            {
                ShowProgressIndicator(false);
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            string url = CheckTextBox.Text;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            if (string.IsNullOrEmpty(url))
                return;

            ShowProgressIndicator(true, "Checking URL...");

            try
            {
                Uri uri = new Uri(url);

                string id = uri.Segments[uri.Segments.Length - 1];

                if (string.IsNullOrEmpty(id))
                {
                    MessageBox.Show("Invalid URL", "Error", MessageBoxButton.OK);
                    return;
                }

                string apiUrlWithId = apiUrl + "/url?id=" + id;

                HttpResponseMessage response = await client.GetAsync(apiUrlWithId);
                string resultJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(resultJson) || resultJson.StartsWith("<"))
                {
                    MessageBox.Show("The server returned an incorrect value! Please verify the server adress and your Internet connection.", "LinkShortener", MessageBoxButton.OK);
                    return;
                }
                JObject apiResponse = JObject.Parse(resultJson);
                string error = apiResponse.Value<string>("error");

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Error", MessageBoxButton.OK);
                }
                else
                {
                    LabelCheckedUrl.Tag = apiResponse.Value<string>("url");
                    LabelCheckedUrl.Text = "This URL leads to:\r\n" + apiResponse.Value<string>("url");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
            finally
            {
                ShowProgressIndicator(false);
            }
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void RefreshStatsButton_Click(object sender, EventArgs e)
        {
            RefreshStats();
        }

        private void ShareUrlButton_Click(object sender, EventArgs e)
        {
            if (LabelShortnedUrl.Tag != null)
            {
                ShareLinkTask task = new ShareLinkTask();
                task.LinkUri = new Uri((string)LabelShortnedUrl.Tag);
                task.Show();
            }
            else
            {
                MessageBox.Show("Please shorten a link first", "LinkShortener", MessageBoxButton.OK);
            }
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void UrlTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                ShortenButton_Click(null, null);
        }

        private void CheckTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                CheckButton_Click(null, null);
        }
    }
}