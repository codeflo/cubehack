using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CubeHack.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new ViewModel();
            viewModel.TriggerFocusChange += () =>
                {
                    FocusManager.SetFocusedElement(this, null);
                };

            DataContext = viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            var viewModel = DataContext as ViewModel;
            if (viewModel != null)
            {
                viewModel.SaveCommand.Execute(null);
            }
        }
    }
}
