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
using ImportSchedule.Model;
using Plugin.Calendars;
using Plugin.Calendars.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace ImportSchedule.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly HttpClient http;
        public MainViewModel()
        {
            _groups = new List<Group>();
            classNumbers = new List<ClassNumber>();

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
                RequestPermissions();
                LoadData();
            }
            
        }

        async void RequestPermissions()
        {
            var statusCalendar = await CrossPermissions.Current.CheckPermissionStatusAsync<CalendarPermission>();
            if(statusCalendar != PermissionStatus.Granted)
                await CrossPermissions.Current.RequestPermissionAsync<CalendarPermission>();


            var statusStorage = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (statusStorage != PermissionStatus.Granted)
                await CrossPermissions.Current.RequestPermissionAsync<CalendarPermission>();
        }

        private List<Group> allGroups;
        private Group _selectedGroup;
        private List<Group> _groups;
        private List<Semester> _semesters;
        private Semester _selectedSemester;
        private bool _isDoneLoading = false;

        private List<ClassNumber> classNumbers;

        #region mvvmProps
        public List<Group> Groups
        {
            get { return _groups; }
            set { _groups = value; RaisePropertyChanged(); }
        }
        
        public Group SelectedGroup
        {
            get { return _selectedGroup; }
            set { _selectedGroup = value; RaisePropertyChanged(); }
        }
        
        public List<Semester> Semesters
        {
            get { return _semesters; }
            set { _semesters = value; RaisePropertyChanged(); }
        }
        
        public Semester SelectedSemester
        {
            get { return _selectedSemester; }
            set { _selectedSemester = value; RaisePropertyChanged(); }
        }

        public bool IsDoneLoading
        {
            get { return _isDoneLoading; }
            set
            {
                _isDoneLoading = value; RaisePropertyChanged();
            }
        }
        #endregion

        async void LoadData()
        {
            var url = "https://lk.ugatu.su/webapi/filter-data/";
            try
            {
                var response = await http.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    HttpContent responseContent = response.Content;
                    var json = await responseContent.ReadAsStringAsync();

                    //загружаем группы
                    allGroups = JObject.Parse(json)["groups"]
                        .Select(g => new Group((string)g["name"], (string)g["faculty"], (int)g["year"]))
                        .OrderBy(g => g.Name)
                        .ToList();

                    Groups = allGroups;

                    //список семестров
                    Semesters = JObject.Parse(json)["semesters"]
                        .Select(s => new Semester(
                            (string)s["type"]
                            , (string)s["academic_year"]
                            , (string)s["start_date"]
                            , (string)s["end_date"]
                            , (int)s["week_offset"]))
                        .ToList();

                    //номера пар
                    classNumbers = JObject.Parse(json)["class_numbers"]
                        .Select(c => new ClassNumber(
                            (string)c["start_time"]
                            , (string)c["end_time"]
                            , (int)c["number"]))
                        .ToList();

                    IsDoneLoading = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                DependencyService.Get<IToast>().Show("Ошибка получения данных");

                //тестовые данные
                allGroups = new List<Group>();
                for (int i = 0; i < 10; i++)
                {
                    var rnd = (new Random()).Next();
                    allGroups.Add(new Group($"Группа {rnd}"));
                }
                Groups = allGroups;
            }         
        }

        
        //поиск группы
        public ICommand PerformSearch => new Command<string>((string query) =>
        {
            if (query != string.Empty)
                Groups = allGroups.Where(g => g.Name.ToLower().StartsWith(query.ToLower())).ToList();
            else
                Groups = allGroups;
        });

        //удаление календаря
        public ICommand ClearCalendar => new Command(async () =>
        {
            if (App.Current.Properties.ContainsKey("myCalendar"))
            {
                var calendarId = (string)App.Current.Properties["myCalendar"];
                var calendar = await CrossCalendars.Current.GetCalendarByIdAsync(calendarId);
                await CrossCalendars.Current.DeleteCalendarAsync(calendar);

                App.Current.Properties.Remove("myCalendar");
                await App.Current.SavePropertiesAsync();

                DependencyService.Get<IToast>().Show("Календарь очищен");
            }
        });

        //импорт
        public ICommand Import => new Command(async () =>
        {
            if(SelectedGroup != null && SelectedSemester != null)
            {
                IsDoneLoading = false;
                var url = "https://lk.ugatu.su/webapi/schedule/";
                try
                {
                    //формируем запрос
                    var jsonObject = new JObject();
                    jsonObject["exams"] = false;
                    jsonObject["group"] = SelectedGroup.Name;
                    jsonObject["semester"] = new JObject();
                    jsonObject["semester"]["type"] = SelectedSemester.Type;
                    jsonObject["semester"]["year"] = SelectedSemester.AcademicYear;

                    var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
                    var response = await http.PostAsync(url, content);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //создаем календарь и запоминаем
                        if (!App.Current.Properties.ContainsKey("myCalendar")) {
                            var myCalendar = await CrossCalendars.Current.CreateCalendarAsync("УГАТУ расписание");
                            App.Current.Properties.Add("myCalendar", myCalendar.ExternalID);
                            await App.Current.SavePropertiesAsync();
                        }    
 
                        var calendarId = (string)App.Current.Properties["myCalendar"];
                        var calendar = await CrossCalendars.Current.GetCalendarByIdAsync(calendarId);

                        var result = await response.Content.ReadAsStringAsync();
                        var classes = JObject.Parse(result)["classes"];
                        CalendarEvent para;
                        foreach (var c in classes)
                        {
                            //собираем ФИО преподавателя
                            string teacher = $"{(string)(c["teacher_surname"] ?? "")} {(string)(c["teacher_name"] ?? "")} {(string)(c["teacher_patronymic"] ?? "")}";

                            //нач.семестра + 7*(неделя-1) + (день-1) + время пары
                            var classNum = classNumbers.Find(t => t.Number == (int)c["class_number"]);
                            var date = SelectedSemester.StartDate.AddDays(7 * ((int)c["week_number"] - 1)).AddDays((int)c["day_number"] - 1);
                            var dateStart = date.AddMinutes(classNum.Start.TotalMinutes);
                            var dateEnd = date.AddMinutes(classNum.End.TotalMinutes);

                            para = new CalendarEvent
                            {
                                Name = (string)c["subject"],
                                Description = $"{(string)c["class_type"]}\n{teacher}",
                                Start = dateStart,
                                End = dateEnd,
                            };

                            await CrossCalendars.Current.AddOrUpdateEventAsync(calendar, para);
                        }
                        IsDoneLoading = true;
                        DependencyService.Get<IToast>().Show("Импорт завершен");
                    }
                    else
                    {
                        DependencyService.Get<IToast>().Show("Ошибка получения данных расписания");
                        IsDoneLoading = true;
                    }
                        
                }
                catch (Exception e) { Debug.WriteLine(e.Message); IsDoneLoading = true; } 
            }
        });

    }
}
