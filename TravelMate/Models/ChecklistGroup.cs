using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate.Models
{
    public class ChecklistGroup : ObservableCollection<Checklist>
    {
        public string Key { get; set; }
        public bool isExpended { get; set; }

        public ChecklistGroup(string key, IEnumerable<Checklist> items) : base(items)
        {
            Key = key;
        }
    }
}
