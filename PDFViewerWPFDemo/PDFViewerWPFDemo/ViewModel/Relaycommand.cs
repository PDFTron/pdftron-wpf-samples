using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PDFViewerWPFDemo.ViewModel
{
    class Relaycommand : ICommand
    {
        // Delegate to be executed
        Action _execute;
        // Delegate to check if we can execute
        Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;

        public Relaycommand(Action execute) :this(execute, null) { }
        public Relaycommand(Action execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new NullReferenceException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        // If null always Can Execute, otherwise run delegate
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
