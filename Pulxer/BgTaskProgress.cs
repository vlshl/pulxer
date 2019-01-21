using Common.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Pulxer
{
    /// <summary>
    /// Изменение состояния
    /// </summary>
    /// <param name="state">Новое состояние</param>
    public delegate void OnBgTaskStateChange(BgTaskState state);

    /// <summary>
    /// Обслуживание фоновых задач.
    /// Допускается произвольная иерархия из подзадач.
    /// </summary>
    public class BgTaskProgress : INotifyPropertyChanged
    {
        private string _name = "";
        private ISyncContext _syncContext = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="syncContext">Контекст синхронизации. События изменения состояний генерируются через него.</param>
        /// <param name="name">Наименование для показа</param>
        public BgTaskProgress(ISyncContext syncContext, string name = "")
        {
            _name = name ?? "";
            _syncContext = syncContext ?? throw new ArgumentNullException("syncContext");
        }

        /// <summary>
        /// Наименование фоновой задачи
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        #region Children
        /// <summary>
        /// Дочерние фоновые задачи
        /// </summary>
        public ObservableCollection<BgTaskProgress> Children
        {
            get
            {
                return _children;
            }
        }
        private ObservableCollection<BgTaskProgress> _children = new ObservableCollection<BgTaskProgress>();

        /// <summary>
        /// Добавить новую дочернюю фоновую задачу
        /// </summary>
        /// <param name="name">Наименование</param>
        /// <returns></returns>
        public BgTaskProgress AddChildProgress(string name)
        {
            BgTaskProgress item = new BgTaskProgress(_syncContext, name);
            _syncContext.RunAsync(() => { _children.Add(item); });

            return item;
        }
        #endregion

        /// <summary>
        /// Сообщить о запуске процесса
        /// </summary>
        /// <param name="isProgressBar">true - если требуется индикатор</param>
        public void OnStart(bool isProgressBar = false)
        {
            State = BgTaskState.Working;
            _isProgressBar = isProgressBar;
            OnPropertyChange("IsProgressBar");
        }

        /// <summary>
        /// Сообщить о стадии выполнении задачи
        /// </summary>
        /// <param name="percent">Процент выполнения</param>
        public void OnProgress(double percent)
        {
            this._percent = percent;
            OnPropertyChange("Percent");
            State = BgTaskState.Working;
        }

        /// <summary>
        /// Имеется ли индикатор
        /// </summary>
        public bool IsProgressBar
        {
            get
            {
                return _isProgressBar;
            }
        }
        private bool _isProgressBar = false;

        /// <summary>
        /// Добавить сообщение о стадии выполнения
        /// </summary>
        /// <param name="message"></param>
        public void OnProgress(string message)
        {
            _syncContext.RunAsync(() => { _progressMessages.Add(message); });
            State = BgTaskState.Working;
        }

        /// <summary>
        /// Список сообщений фоновой задачи
        /// </summary>
        public ObservableCollection<string> ProgressMessages
        {
            get
            {
                return _progressMessages;
            }
        }
        private ObservableCollection<string> _progressMessages = new ObservableCollection<string>();

        /// <summary>
        /// Информировать о завершении задачи
        /// </summary>
        public void OnComplete()
        {
            State = BgTaskState.Complete;
            OnPropertyChange("IsComplete");
        }

        /// <summary>
        /// Завершена задача или нет
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _state == BgTaskState.Complete;
            }
        }

        /// <summary>
        /// Информировать об исключении в ходе выполнения задачи
        /// </summary>
        /// <param name="ex"></param>
        public void OnFault(Exception ex)
        {
            _fault = ex;
            State = BgTaskState.Fault;
            OnPropertyChange("Fault");
            OnPropertyChange("IsFault");
        }

        /// <summary>
        /// Исключение, возникшее в ходе выполнения задачи
        /// </summary>
        public Exception Fault
        {
            get
            {
                return _fault;
            }
        }
        private Exception _fault = null;

        /// <summary>
        /// Завершилось ли выполнение исключением
        /// </summary>
        public bool IsFault
        {
            get
            {
                return _state == BgTaskState.Fault;
            }
        }

        /// <summary>
        /// Информировать о прерывании выполнения задачи
        /// </summary>
        public void OnAbort()
        {
            State = BgTaskState.Abort;
            OnPropertyChange("IsAbort");
        }

        /// <summary>
        /// Была ли прервана задача
        /// </summary>
        public bool IsAbort
        {
            get
            {
                return _state == BgTaskState.Abort;
            }
        }

        /// <summary>
        /// Текущее состояние фоновой задачи
        /// </summary>
        public BgTaskState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state == value) return;

                _state = value;
                RaiseBgTaskStateChange(_state);
                OnPropertyChange("State"); OnPropertyChange("StateStr");
                OnPropertyChange("IsWorking");
            }
        }
        private BgTaskState _state = BgTaskState.None;

        /// <summary>
        /// Событие изменения состояния фоновой задачи
        /// </summary>
        public event OnBgTaskStateChange OnStateChange;

        private void RaiseBgTaskStateChange(BgTaskState state)
        {
            if (OnStateChange != null)
                _syncContext.RunAsync(() => { OnStateChange(state); });
        }

        /// <summary>
        /// Строковое значение текущего состояния
        /// </summary>
        public string StateStr
        {
            get
            {
                string str = "";
                switch (_state)
                {
                    case BgTaskState.None: str = ""; break;
                    case BgTaskState.Working: str = "Работает"; break;
                    case BgTaskState.Complete: str = "Завершено"; break;
                    case BgTaskState.Fault: str = "Ошибка"; break;
                    case BgTaskState.Abort: str = "Прервано"; break;
                }
                return str;
            }
        }

        /// <summary>
        /// Текущее состояние "Работает"
        /// </summary>
        public bool IsWorking
        {
            get
            {
                return _state == BgTaskState.Working;
            }
        }

        /// <summary>
        /// Текущий процент выполнения
        /// </summary>
        public double Percent
        {
            get
            {
                return _percent;
            }
        }
        private double _percent = 0;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string property)
        {
            if (PropertyChanged != null)
                _syncContext.RunAsync(() => { PropertyChanged(this, new PropertyChangedEventArgs(property)); });
        }
        #endregion
    }

    public enum BgTaskState
    {
        None = 0, // начальное состояние
        Working = 1, // работает
        Complete = 2, // завершено
        Fault = 3, // ошибка
        Abort = 4 // прервано
    }
}
