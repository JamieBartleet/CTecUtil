using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil
{
    /// <summary>
    /// Maintains a list of up to Maximum strings, most-recently added first.
    /// </summary>
    public class RecentItemsList
    {
        public RecentItemsList() => Maximum = 10;


        /// <summary>The maximum number of items in the list</summary>
        public int Maximum { get; set; }


        private List<string> _items = new();

        
        /// <summary>The list of items</summary>
        public List<string> Items { get => _items; }


        /// <summary>Add a new name to the start of the list - the most recent items are listed first.<br/>
        /// If name is already in the list it will be moved to the first item.</summary>
        public void Add(string name)
        {
            if (_items.Contains(name))
                _items.Remove(name);
            
            while (_items.Count >= Maximum)
                _items.RemoveAt(_items.Count - 1);
         
            _items.Insert(0, name);

            ApplicationConfig.SaveSettings();
        }
    }
}
