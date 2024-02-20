using Microsoft.EntityFrameworkCore;
using MagPro.Model;
using MagPro.Model.DB;
using MagPro.ViewModel.Wares;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MagPro.ViewModel
{
    public class UsersViewModel : BasicBundle
    {
        private MainWindowViewModel _mvm;
        private ObservableCollection<UserRekord> _users;
        private UserRekord _selectedUser;
        private UserSearcher _filter;
        private List<Rola> _roles;

		private List<UserRekord> _usersToFilter;
		private string _sortColumnName = "Id";
		private SortDirection _sortDirection = SortDirection.Descending;
		private bool _isSorted = false;

		public UsersViewModel(MainWindowViewModel mvm)
        {
            _mvm = mvm;
			this.Title = "Użytkownicy";
			CurrentPage = 1;
            Filter = new UserSearcher();
        }

        public override void DoubleClick()
        {
            Edit();
        }

        public override void Add()
        {
			if (!IsAuthorized(2))
				return;
			View.AddEditWindows.AddEditUserView addEditUserWindow = new View.AddEditWindows.AddEditUserView()
            {
                DataContext = new AddEditWindows.AddEditUserViewModel()
            };
            _mvm.AddMask();
            addEditUserWindow.ShowDialog();
            _mvm.RemoveMask();
            _isSorted = false;
			Load();
		}

        public override void Edit()
        {
			if (!IsAuthorized(2))
				return;
			if (SelectedUser != null)
            {
                if (CurrentPrincipal.Identity.RolaId < SelectedUser.User.RolaId)
                {
                    BasicError("Nie można edytować użytkowników o wyższym poziomie uprawnień.");
                    return;
                }


                View.AddEditWindows.AddEditUserView addEditUserWindow = new View.AddEditWindows.AddEditUserView()
                {
                    DataContext = new AddEditWindows.AddEditUserViewModel(SelectedUser.User)
                };
                _mvm.AddMask();
                addEditUserWindow.ShowDialog();
                _mvm.RemoveMask();
                _isSorted = false;
            }
            else
            {
                BasicError("Nie wybrano żadnego użytkownika");
            }
			Load();
		}

        public override void Delete()
        {
			if (!IsAuthorized(2))
				return;
			if (SelectedUser != null)
            {
                if (CurrentPrincipal.Identity.RolaId < SelectedUser.User.RolaId)
                {
                    BasicError("Nie można usuwać użytkowników o wyższym poziomie uprawnień.");
                    return;
                }

                if (!BasicWarning("Czy na pewno chcesz usunąć użytkownika:\n" + SelectedUser.User.Imie + " " + SelectedUser.User.Nazwisko))
                    return;
                using (var context = new ApplicationDbContext())
                {
                    if (!context.Uzytkownicy.Any(x => x.Id == SelectedUser.User.Id))
                    {
                        BasicError("Nie znaleziono wybranego użytkownika");
                        return;
                    }
                    var userToDelete = context.Uzytkownicy.FirstOrDefault(x => x.Id == SelectedUser.User.Id);
                    context.Uzytkownicy.Remove(userToDelete);
                    context.SaveChanges();
                }
            }
            else
            {
                BasicError("Nie wybrano żadnego użytkownika");
            }
			_isSorted = false;
			Load();
		}

		public override void Refresh()
		{
			SetBusyState();
			using (var context = new ApplicationDbContext())
			{
                _roles = context.Role.ToList();
			}
			_isSorted = false;
			Load();
		}

		public override void Load()
        {
			SetBusyState();

            if (_isSorted == false)
                Sort(_sortColumnName);
            else
            {
                using (var context = new ApplicationDbContext())
                {
                    bool filter = false;
                    if (Filter != null)
                    {
                        if (!string.IsNullOrWhiteSpace(Filter.Nazwa))
                            filter = true;
                        if (!string.IsNullOrWhiteSpace(Filter.Imie))
                            filter = true;
                        if (!string.IsNullOrWhiteSpace(Filter.Nazwisko))
                            filter = true;
                        if (!string.IsNullOrWhiteSpace(Filter.Email))
                            filter = true;
                    }

                    if (_usersToFilter == null)
                        return;

                    if (filter)
                    {
                        Filter.Nazwa = !string.IsNullOrEmpty(Filter.Nazwa) ? Filter.Nazwa : "";
                        Filter.Imie = !string.IsNullOrEmpty(Filter.Imie) ? Filter.Imie : "";
                        Filter.Nazwisko = !string.IsNullOrEmpty(Filter.Nazwisko) ? Filter.Nazwisko : "";
                        Filter.Email = !string.IsNullOrEmpty(Filter.Email) ? Filter.Email : "";
                        Users = new ObservableCollection<UserRekord>();

                        // Update
                        UpdateNumberOfPages(_usersToFilter.Where(x => x.User.Name.IndexOf(Filter.Nazwa, StringComparison.OrdinalIgnoreCase) >= 0 && x.User.Imie.IndexOf(Filter.Imie, StringComparison.OrdinalIgnoreCase) >= 0
                             && x.User.Name.IndexOf(Filter.Nazwisko, StringComparison.OrdinalIgnoreCase) >= 0 && x.User.Email.IndexOf(Filter.Email, StringComparison.OrdinalIgnoreCase) >= 0).Count());

                        _usersToFilter
                            .Where(x => x.User.Name.IndexOf(Filter.Nazwa, StringComparison.OrdinalIgnoreCase) >= 0 && x.User.Imie.IndexOf(Filter.Imie, StringComparison.OrdinalIgnoreCase) >= 0
                              && x.User.Name.IndexOf(Filter.Nazwisko, StringComparison.OrdinalIgnoreCase) >= 0 && x.User.Email.IndexOf(Filter.Email, StringComparison.OrdinalIgnoreCase) >= 0)
                            .Skip((CurrentPage - 1) * ElementsPerPage)
                            .Take(ElementsPerPage)
                            .ToList().ForEach(x => Users.Add(x));
                    }
                    else
                    {
                        UpdateNumberOfPages(_usersToFilter.Count());
                        Users = new ObservableCollection<UserRekord>();
                        _usersToFilter
                            .Skip((CurrentPage - 1) * ElementsPerPage)
                            .Take(ElementsPerPage)
                            .ToList().ForEach(x => Users.Add(x));
                    }
                }
            }
        }

        public override void ResetSearch()
        {
            Filter = new UserSearcher();
            CurrentPage = 1;
			Load();
		}


		public void Sort(string column = null)
		{
            if (_roles == null)
            {
                Refresh();
            }
            else
            {
                using (var context = new ApplicationDbContext())
                {
                    _usersToFilter = new List<UserRekord>();
                    context.Uzytkownicy.ToList().ForEach(x => _usersToFilter.Add(new UserRekord
                    {
                        User = x,
                        Role = _roles.FirstOrDefault(y => y.Id == x.RolaId)
                    }));
                    if (_usersToFilter == null)
                        return;
                    if (_usersToFilter.Count == 0)
                    {
                        //return;
                    }
                }
                if (column == null)
                    column = _sortColumnName;
                if (_sortColumnName != column)
                {
                    _sortColumnName = column;
                }
                switch (column)
                {
                    case "Id":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => x.User.Id.CompareTo(y.User.Id));
                        else
                            _usersToFilter.Sort((x, y) => y.User.Id.CompareTo(x.User.Id));
                        break;
					case "User.Id":
						if (_sortDirection == SortDirection.Ascending)
							_usersToFilter.Sort((x, y) => x.User.Id.CompareTo(y.User.Id));
						else
							_usersToFilter.Sort((x, y) => y.User.Id.CompareTo(x.User.Id));
						break;
					case "User.Name":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => string.Compare(x.User.Name, y.User.Name));
                        else
                            _usersToFilter.Sort((x, y) => string.Compare(y.User.Name, x.User.Name));
                        break;
                    case "User.Imie":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => string.Compare(x.User.Imie, y.User.Imie));
                        else
                            _usersToFilter.Sort((x, y) => string.Compare(y.User.Imie, x.User.Imie));
                        break;
                    case "User.Nazwisko":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => string.Compare(x.User.Nazwisko, y.User.Nazwisko));
                        else
                            _usersToFilter.Sort((x, y) => string.Compare(y.User.Nazwisko, x.User.Nazwisko));
                        break;
                    case "User.Email":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => string.Compare(x.User.Email, y.User.Email));
                        else
                            _usersToFilter.Sort((x, y) => string.Compare(y.User.Email, x.User.Email));
                        break;
                    case "User.Telefon":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => string.Compare(x.User.Telefon, y.User.Telefon));
                        else
                            _usersToFilter.Sort((x, y) => string.Compare(y.User.Telefon, x.User.Telefon));
                        break;
                    case "User.RolaId":
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => x.User.RolaId.CompareTo(y.User.RolaId));
                        else
                            _usersToFilter.Sort((x, y) => y.User.RolaId.CompareTo(x.User.RolaId));
                        break;
					case "Role.Nazwa":
						if (_sortDirection == SortDirection.Ascending)
							_usersToFilter.Sort((x, y) => string.Compare(x.Role.Nazwa, y.Role.Nazwa));
						else
							_usersToFilter.Sort((x, y) => string.Compare(y.Role.Nazwa, x.Role.Nazwa));
						break;
					default:
                        if (_sortDirection == SortDirection.Ascending)
                            _usersToFilter.Sort((x, y) => x.User.Id.CompareTo(y.User.Id));
                        else
                            _usersToFilter.Sort((x, y) => y.User.Id.CompareTo(x.User.Id));
                        break;
                }
                _isSorted = true;
                Load();
            }
		}


        public List<Rola> Roles
        {
            get
            {
                return _roles;
            }
            set
            {
                _roles = value;
                OnPropertyChanged("Roles");
            }
        }
		public bool IsSorted
		{
			get
			{
				return _isSorted;
			}
			set
			{
				_isSorted = value;
				OnPropertyChanged("IsSorted");
			}
		}
		public SortDirection SortDirection
		{
			get
			{
				return _sortDirection;
			}
			set
			{
				_sortDirection = value;
				OnPropertyChanged("SortDirection");
			}
		}
		public UserRekord SelectedUser
        {
            get
            {
                return _selectedUser;
            }
            set
            {
                _selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }
        public ObservableCollection<UserRekord> Users
        {
            get
            {
                return _users;
            }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }
        public UserSearcher Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                OnPropertyChanged("Filter");
            }
        }
    }

    public class UserRekord
    {
        public Uzytkownik User { get; set; }
        public Rola Role { get; set; }
    }

    public class UserSearcher
    {
        public string Nazwa { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Email { get; set; }
    }
}
