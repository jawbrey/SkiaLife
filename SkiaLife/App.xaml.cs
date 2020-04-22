using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiaLife
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new GamePage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
