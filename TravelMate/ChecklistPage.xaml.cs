using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using TravelMate.Models;

namespace TravelMate
{
    public partial class ChecklistPage : ContentPage
    {
        private readonly int userId;
        private readonly string dest;
        private List<Checklist> checklistItems;

        public ChecklistPage(int userId, string destination)
        {
            InitializeComponent();
            this.userId = userId;
            this.dest = destination;

            LoadChecklist();
        }

        private async void LoadChecklist()
        {
            checklistLabel.Text = $"Checklist for {dest}";
            checklistItems = await DatabaseHelper.GetChecklist(userId, dest);

            if (!checklistItems.Any())
            {
                checklistItems = new List<Checklist>
                {
                    new() { UserId = userId, Destination = dest, Item = "Passport & Visa", Category = "Important", IsChecked = false },
                    new() { UserId = userId, Destination = dest, Item = "Boarding Pass", Category = "Important", IsChecked = false },
                    new() { UserId = userId, Destination = dest, Item = "Hotel Confirmation", Category = "Important", IsChecked = false },
                    new() { UserId = userId, Destination = dest, Item = "Essential Medications", Category = "Health & Medications", IsChecked = false },
                    new() { UserId = userId, Destination = dest, Item = "Local Currency & Cards", Category = "Money & Documents", IsChecked = false }
                };

                foreach (var item in checklistItems)
                    await DatabaseHelper.AddChecklistItem(item);
            }

            var groupedChecklist = checklistItems.GroupBy(item => item.Category).Select(g => new ChecklistGroup(g.Key, g.ToList()))
                            .ToList();

            ChecklistListView.ItemsSource = groupedChecklist;
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            string itemName = await DisplayPromptAsync("Add Item", "Enter the item name:");
            if (string.IsNullOrEmpty(itemName)) return;

            string category = await DisplayActionSheet("Select Category", "Cancel", null, "Important", "Clothes", "Electronics", "Health & Medications", "Money & Documents", "Miscellaneous");
            if (category == "Cancel") return;

            var newItem = new Checklist
            {
                UserId = userId,
                Destination = dest,
                Item = itemName,
                Category = category,
                IsChecked = false
            };

            await DatabaseHelper.AddChecklistItem(newItem);
            checklistItems.Add(newItem);
            var groupedChecklist = checklistItems .GroupBy(item => item.Category).Select(g => new ChecklistGroup(g.Key, g.ToList()))
                                    .ToList();

            ChecklistListView.ItemsSource = null;
            ChecklistListView.ItemsSource = groupedChecklist;
        }

        private async void OnItemCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var item = (Checklist)checkbox.BindingContext;
            item.IsChecked = e.Value;
            await DatabaseHelper.UpdateChecklistItem(item);
        }
        private async void ShowMap(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage(userId));
        }

        private async void ShowHotels(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyHotelsPage(userId));
        }

        private async void ShowFlights(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyFlightsPage(userId));
        }

        private async void ShowHome(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(userId));
        }

        private async void ShowProfileOptions(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyProfilePage(userId));
        }
    }
}
