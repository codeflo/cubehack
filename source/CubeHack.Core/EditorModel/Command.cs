// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Windows.Input;

namespace CubeHack.EditorModel
{
    public class Command : ICommand
    {
        private Action<object> _execute;

        public Command(Action execute)
        {
            _execute = o => execute();
        }

        public Command(Action<object> execute)
        {
            _execute = execute;
        }

#pragma warning disable 0067 // The event is never used

        public event EventHandler CanExecuteChanged;

#pragma warning restore 0067

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
