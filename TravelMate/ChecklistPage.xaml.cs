using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using TravelMate.Models;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class ChecklistPage : ContentPage
    {
        private readonly int userId;
        private readonly string dest;
        private ObservableCollection<ChecklistGroup> groupedChecklistItems;

        public ChecklistPage(int UserId, string destination)
        {
            InitializeComponent();
            this.userId = UserId;
            this.dest = destination;
            var navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;
            LoadChecklist();
        }

        private async void LoadChecklist()
        {
            checklistLabel.Text = $"Checklist for {dest}";
            var checklistItems = await DatabaseHelper.GetChecklist(userId, dest);

            // If checklist is empty, insert default items
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

            // Convert list to grouped format
            groupedChecklistItems = new ObservableCollection<ChecklistGroup>(
                checklistItems.GroupBy(item => item.Category)
                .Select(g => new ChecklistGroup(g.Key, g.ToList()))
            );

            ChecklistView.ItemsSource = groupedChecklistItems;
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            string itemName = await DisplayPromptAsync("Add Item", "Enter the item name:");
            if (string.IsNullOrEmpty(itemName)) return;

            string category = await DisplayActionSheet("Select Category", "Cancel", null,
                "Important", "Clothes", "Electronics", "Health & Medications", "Money & Documents", "Miscellaneous");

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

            // Find existing category group or create a new one
            var categoryGroup = groupedChecklistItems.FirstOrDefault(g => g.Key == category);
            if (categoryGroup == null)
            {
                categoryGroup = new ChecklistGroup(category, new List<Checklist> { newItem });
                groupedChecklistItems.Add(categoryGroup);
            }
            else
            {
                categoryGroup.Add(newItem);
            }
        }

        private async void OnDeleteItem(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Checklist item)
            {
                bool confirm = await DisplayAlert("Delete Item", $"Are you sure you want to delete '{item.Item}'?", "Yes", "No");
                if (!confirm) return;

                await DatabaseHelper.DeleteChecklistItem(item);

                // Remove from UI
                var categoryGroup = groupedChecklistItems.FirstOrDefault(g => g.Key == item.Category);
                if (categoryGroup != null)
                {
                    categoryGroup.Remove(item);
                    if (categoryGroup.Count == 0)
                    {
                        groupedChecklistItems.Remove(categoryGroup);
                    }
                }
            }
        }


        private void ToggleCategoryExpand(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is ChecklistGroup group)
            {
                group.IsExpanded = !group.IsExpanded;
                ChecklistView.ItemsSource = null; 
                ChecklistView.ItemsSource = groupedChecklistItems;
            }
        }


        private async void OnItemCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.BindingContext is Checklist item)
            {
                item.IsChecked = e.Value;
                await DatabaseHelper.UpdateChecklistItem(item);
            }
        }

    }

    public class ChecklistGroup : ObservableCollection<Checklist>
    {
        public string Key { get; }
        public bool IsExpanded { get; set; } = true;

        public ChecklistGroup(string key, List<Checklist> items) : base(items)
        {
            Key = key;
        }
    }
}
