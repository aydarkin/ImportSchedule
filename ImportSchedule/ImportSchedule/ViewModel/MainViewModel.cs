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
using ImportSchedule.Base;

namespace ImportSchedule.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private List<Group> _groups;
        public List<Group> Groups 
        { 
            get { return _groups; }
            set
            {
                _groups = value;
                RaisePropertyChanged("Groups");
            }
        }
        private readonly HttpClient http;

        public MainViewModel()
        {
            _groups = new List<Group>();

            //костыль для ssl сертификатов
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    http = new HttpClient(DependencyService.Get<IHTTPClientHandlerCreationService>().GetInsecureHandler());
                    break;
                default:
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    http = new HttpClient(new HttpClientHandler());
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

                Groups = JObject.Parse(json)["body"]
                    .Select(g => new Group((int)g["id"], (string)g["title"]))
                    .OrderBy(g => g.Title)
                    .ToList();
            }
        }

        private Group _selected = new Group(0, "Группа");
        public Group Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
            }
        }
    }
}
