using Microsoft.Maui.Controls;
using TravelMate.Controls;
using TravelMate.ViewModels;

namespace TravelMate
{
    public partial class CurrencyConverterPage : ContentPage
    {
        public CurrencyConverterPage(int userId, string destination)
        {
            InitializeComponent();
            NavigationBar navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;

            BindingContext = new CurrencyConverterViewModel();
        }
    }
}
