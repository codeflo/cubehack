using CubeHack.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CubeHack.Editor
{
    public class ItemEditor : Control
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
            "Item",
            typeof(Item),
            typeof(ItemEditor));

        public ItemEditor()
        {
        }

        public Item Item
        {
            get
            {
                return (Item)GetValue(ItemProperty);
            }

            set
            {
                SetValue(ItemProperty, value);
            }
        }
    }
}
