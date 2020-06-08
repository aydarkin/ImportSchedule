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
using System.Windows.Input;
using System.Diagnostics;

namespace ImportSchedule.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private List<Group> allGroups;
        private List<Group> _groups;
        public List<Group> Groups 
        {
            get { return _groups; }
            set { _groups = value; RaisePropertyChanged(); }
        }

        private Group _selected;
        public Group Selected
        {
            get { return _selected; }
            set { _selected = value; RaisePropertyChanged(); }
        }
        private readonly HttpClient http;

        public MainViewModel()
        {
            _groups = new List<Group>();
            _selected = new Group(0, "Группа");

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

            if (!DesignMode.IsDesignModeEnabled)
            {
                LoadGroups();
            }
            
        }

        async void LoadGroups()
        {
            var url = "https://ugatoo.ru/service/api/v2/find/group?term=";
            try
            {
                var response = await http.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    HttpContent responseContent = response.Content;
                    var json = await responseContent.ReadAsStringAsync();

                    allGroups = JObject.Parse(json)["body"]
                        .Select(g => new Group((int)g["id"], (string)g["title"]))
                        .OrderBy(g => g.Title)
                        .ToList();

                    Groups = allGroups;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                allGroups = new List<Group>();
                for (int i = 0; i < 10; i++)
                {
                    var rnd = (new Random()).Next();
                    allGroups.Add(new Group(rnd, $"Группа {rnd}"));
                }
                Groups = allGroups;
            }         
        }

        
        public ICommand PerformSearch => new Command<string>((string query) =>
        {
            Groups = allGroups.Where(g => g.Title.ToLower().StartsWith(query.ToLower())).ToList();
        });
    }
}
