using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.DataModel
{
    public class ObjectItem : Item
    {
        Type _type;

        public ObjectItem(Type type)
        {
            _type = type;

            TypeName = type.Name;
            SharedSizeGroupName = "x_" + Guid.NewGuid().ToString().Replace('-', '_');

            Properties = new ObservableCollection<Property>();
            foreach (var property in type.GetProperties().Where(f => f.GetCustomAttributes(typeof(CubeHack.Data.EditorDataAttribute), false).Length != 0))
            {
                Properties.Add(new Property(this) { Name = property.Name, Value = Item.Create(property.PropertyType) });
            }
        }

        private string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            private set { SetAndNotify(ref _typeName, value); }
        }

        public string SharedSizeGroupName
        {
            get;
            private set;
        }

        public ObservableCollection<Property> Properties { get; private set; }

        public override JToken Save()
        {
            var output = new JObject();

            foreach (var property in Properties)
            {
                output[property.Name] = property.Value == null ? null : property.Value.Save();
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
                var matchingProperty = Properties.Where(p => p.Name == entry.Key).FirstOrDefault();
                if (matchingProperty != null)
                {
                    matchingProperty.Value.Load(entry.Value);
                }
            }

            IsExpanded = IsExpanded || Properties.Count <= ExpansionLimit;
        }

        public override object GetObject()
        {
            object instance = Activator.CreateInstance(_type);
            foreach (var property in Properties)
            {
                var matchingProperty = _type.GetProperty(property.Name);
                if (matchingProperty != null)
                {
                    matchingProperty.SetValue(instance, property.Value.GetObject());
                }
            }
            return instance;
        }

        public class Property : NotifyPropertyChanged
        {
            private ObjectItem _parent;

            public Property(ObjectItem parent)
            {
                _parent = parent;
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
        }
    }
}
