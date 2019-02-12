using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Debugger
{
    public class LambdaCommand : ICommand
    {
        Action<object> OnExecute = (o) => { };
        Func<object, bool> OnCanExecute = (o) => { return true; };

        public LambdaCommand(Action<object> execute)
        {
            OnExecute = execute;
        }

        public LambdaCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            OnExecute = execute;
            OnCanExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return OnCanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            OnExecute(parameter);
        }
    }
}
