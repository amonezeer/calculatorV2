using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IOSCalculator
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private string display;
        private double currentValue;
        private string currentOperator;
        private bool isNewEntry;

        public string Display
        {
            get => display;
            set
            {
                display = value;
                OnPropertyChanged(nameof(Display));
            }
        }

        private string currentOperation;
        public string CurrentOperation
        {
            get => currentOperation;
            set
            {
                currentOperation = value;
                OnPropertyChanged(nameof(CurrentOperation));
            }
        }

        public ICommand NumberCommand { get; }
        public ICommand OperatorCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand NegateCommand { get; }
        public ICommand PercentCommand { get; }

        public CalculatorViewModel()
        {
            Display = "0";
            CurrentOperation = "";
            currentValue = 0;
            currentOperator = string.Empty;
            isNewEntry = true;

            NumberCommand = new RelayCommand(param => AddNumber(param?.ToString()));
            OperatorCommand = new RelayCommand(param => SetOperatorAsync(param?.ToString()));
            ClearCommand = new RelayCommand(_ => Clear());
            EqualsCommand = new RelayCommand(_ => CalculateAsync());
            NegateCommand = new RelayCommand(_ => Negate());
            PercentCommand = new RelayCommand(_ => Percent());
        }

        private void AddNumber(string? number)
        {
            if (string.IsNullOrEmpty(number)) return;

            if (isNewEntry || Display == "0")
            {
                Display = number;
                isNewEntry = false;
            }
            else
            {
                Display += number;
            }
            CurrentOperation += number;
        }

        private async void SetOperatorAsync(string? op)
        {
            if (string.IsNullOrEmpty(op)) return;

            if (!isNewEntry)
            {
                await CalculateAsync();
            }

            currentValue = double.TryParse(Display, out var value) ? value : 0;
            currentOperator = op;
            CurrentOperation += $" {currentOperator} ";
            isNewEntry = true;
        }

        private void Clear()
        {
            Display = "0";
            CurrentOperation = "";
            currentValue = 0;
            currentOperator = string.Empty;
            isNewEntry = true;
        }

        private async Task CalculateAsync()
        {
            if (string.IsNullOrEmpty(currentOperator)) return;

            var newValue = double.TryParse(Display, out var value) ? value : 0;

            await Task.Run(() =>
            {
                switch (currentOperator)
                {
                    case "+":
                        currentValue += newValue;
                        break;
                    case "-":
                        currentValue -= newValue;
                        break;
                    case "*":
                        currentValue *= newValue;
                        break;
                    case "/":
                        if (newValue != 0)
                        {
                            currentValue /= newValue;
                        }
                        else
                        {
                            Display = "Error";
                            Clear();
                            return;
                        }
                        break;
                }
            });

            Display = currentValue.ToString();
            CurrentOperation += $" = {currentValue}";
            currentOperator = string.Empty;
            isNewEntry = true;
        }

        private void Negate()
        {
            if (double.TryParse(Display, out var result))
            {
                Display = (-result).ToString();
                CurrentOperation += " (±)";
            }
        }

        private void Percent()
        {
            if (double.TryParse(Display, out var result))
            {
                Display = (result / 100).ToString();
                CurrentOperation += " (%)";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool>? canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
