using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;

namespace LinkShortener
{
    public partial class AboutPage : PhoneApplicationPage
    {
        string apiUrl;

        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

            var version = nameHelper.Version;

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            
            if (!settings.TryGetValue<string>("server_url", out apiUrl))
            {
                apiUrl = "https://short.marmak.net.pl/api";
            }

            if (!MainPage.IsMobileData())
            {
                DisplayServerVersion();
            }

            AppVersionLabel.Text = "Version " + version;
        }

        private async void DisplayServerVersion()
        {
            try
            {
                HttpResponseMessage response = await MainPage.Current.client.GetAsync(apiUrl + "/version");

                string resultJson = await response.Content.ReadAsStringAsync();
                JObject apiResponse = JObject.Parse(resultJson);
                string serverVersion = apiResponse.Value<string>("version");

                ServerVersionLabel.Text = "Server version: " + serverVersion;
            }
            catch (Exception)
            {
                ServerVersionLabel.Text = "Server version: N/A";
            }
        }

        private void VisitWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("https://marmak.net.pl", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void ServerVersionLabel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            DisplayServerVersion();
        }

        private void Kierownik_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailTask = new EmailComposeTask();
            emailTask.To = "dev@marmak.net.pl";
            emailTask.Show();
        }

        private void Olek_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("https://github.com/Olek47", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void VisitGithubButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("https://github.com/Kierownik223/LinkShortenerWP8", UriKind.Absolute);
            webBrowserTask.Show();
        }
    }
}