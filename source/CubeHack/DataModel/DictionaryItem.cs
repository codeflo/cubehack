using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CubeHack.DataModel
{
    public class DictionaryItem : Item
    {
        readonly Type _valueType;

        public DictionaryItem(Type valueType)
        {
            _valueType = valueType;
            SharedSizeGroupName = "x_" + Guid.NewGuid().ToString().Replace('-', '_');
            Entries = new ObservableCollection<Entry>();
            AddEntryCommand = new Command(AddEntry);
            IsExpanded = true;
        }

        public ICommand AddEntryCommand
        {
            get;
            private set;
        }

        public string SharedSizeGroupName
        {
            get;
            private set;
        }

        ObservableCollection<Entry> _entries;
        public ObservableCollection<Entry> Entries
        {
            get
            {
                return _entries;
            }

            private set
            {
                SetAndNotify(ref _entries, value);
            }
        }

        public override JToken Save()
        {
            var sortedEntries = new ObservableCollection<Entry>(Entries.OrderBy(e => e.Name));
            if (Entries.Zip(sortedEntries, (e1, e2) => e1.Name == e2.Name).Any(b => !b))
            {
                // List is not ordered.
                Entries = sortedEntries;
            }

            var output = new JObject();

            foreach (var entry in Entries)
            {
                output[entry.Name] = entry.Value.Save();
            }

            return output;
        }

        public override void Load(JToken data)
        {
            JObject o = data as JObject;
            if (o == null)
            {
                return;
            }

            foreach (var entry in o)
            {
                var item = Item.Create(_valueType);
                item.Load(entry.Value);
                Entries.Add(new Entry(this) { Name = entry.Key, Value = item });
            }

            IsExpanded = Entries.Count <= ExpansionLimit;
        }

        public override object GetObject()
        {
            dynamic instance = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(string), _valueType));
            foreach (var entry in Entries)
            {
                instance.Add(entry.Name, (dynamic)entry.Value.GetObject());
            }
            return instance;
        }

        void AddEntry()
        {
            Entries.Add(new Entry(this) { Name = "New entry", Value = Item.Create(_valueType) });
        }

        public class Entry : NotifyPropertyChanged
        {
            private DictionaryItem _parent;

            public Entry(DictionaryItem parent)
            {
                _parent = parent;
                RemoveCommand = new Command(Remove);
            }

            public ICommand RemoveCommand
            {
                get;
                private set;
            }

            private string _name;
            public string Name
            {
                get { return _name; }
                set { SetAndNotify(ref _name, value); }
            }

            private Item _value;
            public Item Value
            {
                get { return _value; }
                set { SetAndNotify(ref _value, value); }
            }

            public string SharedSizeGroupName
            {
                get
                {
                    return _parent.SharedSizeGroupName;
                }
            }

            private void Remove()
            {
                _parent.Entries.Remove(this);
            }
        }
    }
}
