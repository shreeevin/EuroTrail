using EuroTrail.Database;

namespace EuroTrail
{
    public partial class App : Application
    {
        public App()
        {

            InitializeComponent();

            DatabaseInitializer.Initialize();
            
            MainPage = new MainPage();
        }
    }
}
