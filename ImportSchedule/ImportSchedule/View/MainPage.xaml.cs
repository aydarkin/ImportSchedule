using ImportSchedule.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ImportSchedule.View
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainViewModel ViewModel { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            BindingContext = ViewModel;

        }

        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchBar.SearchCommand.Execute(searchBar.SearchCommandParameter);
        }

        private void searchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            searchBar.Text = searchResults.SelectedItem.ToString();
        }
    }
}
