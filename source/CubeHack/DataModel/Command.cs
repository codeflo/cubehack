using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CubeHack.DataModel
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

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 0067 // The event is never used
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
