using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ImportSchedule
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPageViewModel ViewModel { get; private set; }

        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            ViewModel = vm;

            BindingContext = ViewModel;
        }
    }
}
