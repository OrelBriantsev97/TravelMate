using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelMate.Models;
using TravelMate.Services;
using Microsoft.Maui.Controls;

namespace TravelMate.ViewModels
{
    public class ChecklistViewModel : BindableObject
    {
        // ViewModel responsible for loading, grouping and managing checklist items
        /// for a specific user and destination.
        public ObservableCollection<ChecklistGroup> GroupedChecklistItems { get; set; } = new();

        public string Destination { get; } // The destination for which the checklist is being managed.
        public int UserId { get; }

        public ICommand AddItemCommand { get; } // Command to add a new item to the checklist.
        public ICommand DeleteItemCommand { get; }  // Command to delete an item from the checklist.
        public ICommand ToggleExpandCommand { get; }// Command to toggle the expansion of a checklist group.
        public ICommand CheckChangedCommand { get; } // Command to update the checked status of an item.

        public ChecklistViewModel(int userId, string destination)
        {
            UserId = userId;
            Destination = destination;

            AddItemCommand = new Command(async () => await AddItem());
            DeleteItemCommand = new Command<Checklist>(async (item) => await DeleteItem(item));
            ToggleExpandCommand = new Command<ChecklistGroup>(ToggleExpand);
            CheckChangedCommand = new Command<Checklist>(async (item) => await UpdateCheck(item));

            _ = LoadChecklist();
        }

        // Loads existing checklist items from the database. If none exist,
        // seeds a default set of items and saves them. Then groups items
        // by category for display.
        private async Task LoadChecklist()
        {
            var checklistItems = await DatabaseHelper.GetChecklist(UserId, Destination);
            if (!checklistItems.Any())
            {
                checklistItems = new List<Checklist>
                {
                    new() { UserId = UserId, Destination = Destination, Item = "Passport & Visa", Category = "Important", IsChecked = false },
                    new() { UserId = UserId, Destination = Destination, Item = "Boarding Pass", Category = "Important", IsChecked = false },
                    new() { UserId = UserId, Destination = Destination, Item = "Hotel Confirmation", Category = "Important", IsChecked = false },
                    new() { UserId = UserId, Destination = Destination, Item = "Essential Medications", Category = "Health & Medications", IsChecked = false },
                    new() { UserId = UserId, Destination = Destination, Item = "Local Currency & Cards", Category = "Money & Documents", IsChecked = false }
                };

                foreach (var item in checklistItems)
                    await DatabaseHelper.AddChecklistItem(item);
            }

            var grouped = checklistItems.GroupBy(item => item.Category)
                .Select(g => new ChecklistGroup(g.Key, g.ToList()) { IsExpanded = true });

            GroupedChecklistItems = new ObservableCollection<ChecklistGroup>(grouped);
            OnPropertyChanged(nameof(GroupedChecklistItems));
        }

        // Prompts the user to enter a new checklist item and category,
        /// then adds it to both the database and the local collection.
        private async Task AddItem()
        {
            string itemName = await Application.Current.MainPage.DisplayPromptAsync("Add Item", "Enter the item name:");
            if (string.IsNullOrEmpty(itemName)) return;

            string category = await Application.Current.MainPage.DisplayActionSheet("Select Category", "Cancel", null,
                "Important", "Clothes", "Electronics", "Health & Medications", "Money & Documents", "Miscellaneous");
            if (category == "Cancel") return;

            var newItem = new Checklist
            {
                UserId = UserId,
                Destination = Destination,
                Item = itemName,
                Category = category,
                IsChecked = false
            };

            await DatabaseHelper.AddChecklistItem(newItem);

            var group = GroupedChecklistItems.FirstOrDefault(g => g.Key == category);
            if (group == null)
            {
                group = new ChecklistGroup(category, new List<Checklist> { newItem });
                GroupedChecklistItems.Add(group);
            }
            else
            {
                group.Add(newItem);
            }
        }

        // Deletes the specified item from the database and updates the local collection.
        // Removes a category group if it becomes empty.
        private async Task DeleteItem(Checklist item)
        {
            await DatabaseHelper.DeleteChecklistItem(item);
            var group = GroupedChecklistItems.FirstOrDefault(g => g.Key == item.Category);
            if (group != null)
            {
                group.Remove(item);
                if (!group.Any())
                    GroupedChecklistItems.Remove(group);
            }
        }

        // Toggles the expanded/collapsed state of a checklist category group.
        private void ToggleExpand(ChecklistGroup group)
        {
            group.IsExpanded = !group.IsExpanded;
            OnPropertyChanged(nameof(GroupedChecklistItems));
        }

        // Updates the checked state of an item in the database when the user toggles it.
        private async Task UpdateCheck(Checklist item)
        {
            await DatabaseHelper.UpdateChecklistItem(item);
        }
    }
}
