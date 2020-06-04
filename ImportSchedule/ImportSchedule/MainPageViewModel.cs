using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;

namespace ImportSchedule
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Group> _groups;
        public List<Group> Groups 
        { 
            get { return _groups; }
            set
            {
                _groups = value;
                OnPropertyChanged("Groups");
            }
        }
        private readonly HttpClient http;

        public MainPageViewModel()
        {
            _groups = new List<Group>();

            //костыль для ssl сертификатов
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    this.http = new HttpClient(DependencyService.Get<IHTTPClientHandlerCreationService>().GetInsecureHandler());
                    break;
                default:
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    this.http = new HttpClient(new HttpClientHandler());
                    break;
            }


            LoadGroups();
        }

        async void LoadGroups()
        {
            var url = "https://ugatoo.ru/service/api/v2/find/group?term=";

            var response = await http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();

                Groups = JObject.Parse(json)["body"].Select(g => new Group((int)g["id"], (string)g["title"])).ToList();
            }
        }

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
