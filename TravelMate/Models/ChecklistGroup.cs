using System.Collections.ObjectModel;

namespace TravelMate.Models
{
    public class ChecklistGroup : ObservableCollection<Checklist>
    {
        public string Key { get; set; }
        public bool IsExpanded { get; set; }

        public ChecklistGroup(string key, IEnumerable<Checklist> items) : base(items)
        {
            Key = key;
            IsExpanded = true;
        }
    }
}
