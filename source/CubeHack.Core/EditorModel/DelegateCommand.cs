// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Windows.Input;

namespace CubeHack.EditorModel
{
    public class DelegateCommand : ICommand
    {
        private Action<object> _execute;

        public DelegateCommand(Action execute)
        {
            _execute = o => execute();
        }

        public DelegateCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
