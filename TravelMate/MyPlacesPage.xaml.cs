using TravelMate.ViewModels;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class MyPlacesPage : ContentPage
    {
        public MyPlacesPage(int userId, string destination)
        {
            InitializeComponent();
            NavigationContainer.Content = new NavigationBar(userId, destination);
            BindingContext = new MyPlacesViewModel(userId, destination);
        }
    }
}
