using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PAENN2
{
    /// <summary>
    /// Abstract class implementing the INotifyPropertyChanged interface.
    /// </summary>
    public abstract class Observable : INotifyPropertyChanged
    {
        // Broadcasts the change in property so it can be picked up by the WPF controls.
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Broadcasts the change in a given property.
        /// </summary>
        /// <param name="Property">The property's name.</param>
        public void NotifyPropertyChanged(string Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }

        /// <summary>
        /// Notifies the change in a property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="field">The private backing field.</param>
        /// <param name="value">The newly assigned value.</param>
        protected void ChangeProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                NotifyPropertyChanged(propertyName);
            }
        }
    }

    /// <summary>
    /// Basic command delegate class, implementing the ICommand interface.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> action)
        {
            execute = action;
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

    }
}
