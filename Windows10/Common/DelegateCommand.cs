using System;
using System.Diagnostics;

namespace SimpleOcr10.Common
{
        public class DelegateCommand : System.Windows.Input.ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;
            public event EventHandler CanExecuteChanged;

            public DelegateCommand(Action execute, Func<bool> canExecute = null)
            {
                if (execute == null)
                {
                    throw new ArgumentNullException(nameof(execute));
                }
                _execute = execute;
                _canExecute = canExecute ?? (() => true);
            }
            
            public bool CanExecute(object p)
            {
                try { return _canExecute(); }
                catch { return false; }
            }

            public void Execute(object p)
            {
                if (!CanExecute(p))
                {
                    return;
                }
                try { _execute(); }
                catch { Debugger.Break(); }
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public class DelegateCommand<T> : System.Windows.Input.ICommand
        {
            private readonly Action<T> _execute;
            private readonly Func<T, bool> _canExecute;
            public event EventHandler CanExecuteChanged;

            public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
            {
                if (execute == null)
                {
                    throw new ArgumentNullException(nameof(execute));
                }
                _execute = execute;
                _canExecute = canExecute ?? (e => true);
            }
            
            public bool CanExecute(object p)
            {
                try
                {
                    var value = (T) p;
                    return _canExecute?.Invoke(value) ?? true;
                }
                catch { return false; }
            }

            public void Execute(object p)
            {
                if (!CanExecute(p))
                {
                    return;
                }
                var value = (T) p;
                _execute(value);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
