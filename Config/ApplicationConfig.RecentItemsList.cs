using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CTecUtil.Config
{
    /// <summary>
    /// Maintains a list of up to Maximum file paths, most-recently added first.
    /// </summary>
    public class RecentFilesList
    {
        private const int _limit = 10;
        private int _maximum = _limit;
        private List<string> _items = new();
        

        /// <summary>Delegate to send notification when a recent files list has changed</summary>
        public delegate void RecentFileListChangeNotifier();

        /// <summary>Sends notification when the item list has changed</summary>
        [JsonIgnore]
        public RecentFileListChangeNotifier RecentFileListHasChanged;


        /// <summary>The maximum number of items in the list</summary>
        public int Maximum { get => _maximum; set { _maximum = Math.Min(value, _limit); } }


        /// <summary>The list of file paths</summary>
        public List<string> Items { get => _items; }


        /// <summary>Add a new file path to the list.  The most recent items are listed first;
        /// if path is already in the list it will be moved to the first item.</summary>
        public void Add(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (_items.Contains(path))
                _items.Remove(path);

            while (_items.Count >= Maximum)
                _items.RemoveAt(_items.Count - 1);

            _items.Insert(0, path);

            ApplicationConfig.Save = true;
            RecentFileListHasChanged?.Invoke();
        }


        /// <summary>Remove a file path from the list.</summary>
        public void Remove(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (_items.Contains(path))
                _items.Remove(path);

            ApplicationConfig.Save = true;
            RecentFileListHasChanged?.Invoke();
        }
    }
}
