// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Windows;
using System.Windows.Input;

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
