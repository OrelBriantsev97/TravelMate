using Microsoft.Maui.Controls;
using TravelMate.ViewModels;
using TravelMate.Controls;
using TravelMate.Models;    

namespace TravelMate
{
    public partial class ChecklistPage : ContentPage
    {
        public ChecklistPage(int userId, string destination)
        {
            InitializeComponent();
            var navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;
            BindingContext = new ChecklistViewModel(userId, destination);
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is Checklist checklist)
            {
                checklist.IsChecked = e.Value;

                if (BindingContext is ChecklistViewModel viewModel)
                {
                    viewModel.CheckChangedCommand?.Execute(checklist);
                }
            }
        }

    }
}
