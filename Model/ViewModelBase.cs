using MagPro.Interfaces;
using MagPro.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MagPro.Model
{
    /// <summary>
    /// Schemat obiektów modelu klasy
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged, IDisposable, IUpdate, INavigation
    {
		#region Navigation
		private ICommand _goBackCommand;
        public ViewModelBase PreviouseViewModelBase { get; set; }
        public bool CanGoBack()
        {
            if (PreviouseViewModelBase != null)
                return true;
            return false;
        }
        public virtual void GoBack() {  }
        public ICommand GoBackCommand { get { return _goBackCommand; } }
        #endregion

        public readonly INumberGenerator NumberGenerator;
        private bool _isEditable;
        private CustomPrincipal _currentPrincipal;

		public ViewModelBase()
        {
            NumberGenerator = new NumberGeneratorService();
            _goBackCommand = new RelayCommand(x => GoBack());
        }

        public string Title { get; set; }

        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                _isEditable = value;
                OnPropertyChanged("IsEditable");
            }
        }

		public CustomPrincipal CurrentPrincipal
		{
			get
			{
				return _currentPrincipal;
			}
			set
			{
				_currentPrincipal = value;
				OnPropertyChanged("CurrentPrincipal");
			}
		}


		/// <summary>
		/// Zdarzenie zmiany właściwości obiektu
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Funkcja wywoływana przy zmianie właściwości
        /// np. przy zmianie wartości zmiennej lub obiektu.
        /// Jako parametr przyjmowana jest nazwa obiektu
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Dispose() { }
        
        /// <summary>
        /// Pobiera i aktualizuje dane z bazy danych. 
        /// Przypisuje do list te elementy, które spełniają wybrane kryteria.
        /// </summary>
        public virtual void Refresh() { }

		/// <summary>
		/// Ładuje obiekty wykorzystywane przez daną strone.
		/// </summary>
		public virtual void Load() { }

		public virtual int CurrentPage { get; set; }

        /// <summary>
        /// Wartość określająca czy w danym momencie wykonywane są jakieś operacje czyli czy UI jest zajęte.
        /// </summary>
        private static bool IsBusy;

        /// <summary>
        /// Ustawia parametr busy.
        /// </summary>
        public static void SetBusyState()
        {
            SetBusyState(true);
        }

        /// <summary>
        /// Ustawia zmienną busy na zajętą lub nie.
        /// </summary>
        /// <param name="busy"></param>
        private static void SetBusyState(bool busy)
        {
            if(busy != IsBusy)
            {
                IsBusy = busy;
                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if(IsBusy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle, dispatcherTimer_Tick, System.Windows.Application.Current.Dispatcher);
                }
            }
        }

		/// <summary>
		/// Handles the Tick event of the dispatcherTimer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			var dispatcherTimer = sender as DispatcherTimer;
			if (dispatcherTimer != null)
			{
				SetBusyState(false);
				dispatcherTimer.Stop();
			}
		}

        public void RefreshCurrentPrincipal()
        {
			CurrentPrincipal = Thread.CurrentPrincipal as CustomPrincipal;
		}

		public bool IsAuthorized(int accessLevel)
		{
            if (CurrentPrincipal == null)
                RefreshCurrentPrincipal();
            if (CurrentPrincipal.Identity.RolaId >= accessLevel)
                return true;
            else
                return false;
		}

		/// <summary>
		/// Wyświetla standardowe okienko błędu, z przyciskiem 'OK'
		/// </summary>
		/// <param name="message"></param>
		public void BasicError(string message)
        {
            bool? Result = new CustomMessageBox(message, MessageType.Error, MessageButtons.Ok).ShowDialog();
        }

        /// <summary>
        /// Wyświetla standardowe okienko ostrzeżenia, z przyciskami Tak i Nie
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool BasicWarning(string message)
        {
            bool? Result = new CustomMessageBox(message, MessageType.Warning, MessageButtons.YesNo).ShowDialog();
            return (bool)Result;
        }

        /// <summary>
        /// Wyświetla standardowe okienko informacyjne, z przyciskiem 'OK'
        /// </summary>
        /// <param name="message"></param>
        public void BasicInformation(string message)
        {
            bool? Result = new CustomMessageBox(message, MessageType.Info, MessageButtons.Ok).ShowDialog();
        }

        /// <summary>
        /// Wyświetla standardowe okienko potwierdzenia, z przyciskami 'OK' i Anuluj
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool BasicConfirmation(string message)
        {
            bool? Result = new CustomMessageBox(message, MessageType.Confirmation, MessageButtons.OkCancel).ShowDialog();
            return (bool)Result;
        }

        /// <summary>
        /// Wyświetla standardowe okienko powodzenia/sukcesu, z przyciskiem 'OK'
        /// </summary>
        /// <param name="message"></param>
        public void BasicSuccess(string message)
        {
            bool? Result = new CustomMessageBox(message, MessageType.Success, MessageButtons.Ok).ShowDialog();
        }
    }
}
