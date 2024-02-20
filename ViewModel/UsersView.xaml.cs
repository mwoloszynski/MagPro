using MagPro.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MagPro.View
{
    /// <summary>
    /// Logika interakcji dla klasy UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
        }

		GridViewColumnHeader _lastHeaderClicked = null;
		ListSortDirection _lastDirection = ListSortDirection.Ascending;

		private void UsersListView_Click(object sender, RoutedEventArgs e)
		{
			var headerClicked = e.OriginalSource as GridViewColumnHeader;
			ListSortDirection direction;

			if (headerClicked != null)
			{
				var vm = (UsersViewModel)this.DataContext;
				if (vm == null)
					return;


				if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
				{
					if (headerClicked != _lastHeaderClicked)
					{
						direction = ListSortDirection.Descending;
					}
					else
					{
						if (_lastDirection == ListSortDirection.Ascending)
						{
							direction = ListSortDirection.Descending;
						}
						else
						{
							direction = ListSortDirection.Ascending;
						}
					}

                    var tagSetter = headerClicked.Column.HeaderContainerStyle.Setters.OfType<Setter>().FirstOrDefault(x => x.Property == TagProperty);
                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var columnHeader = headerClicked.Column.Header.ToString();
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

					if (direction == ListSortDirection.Ascending)
						vm.SortDirection = SortDirection.Ascending;
					else
						vm.SortDirection = SortDirection.Descending;
					vm.Sort(sortBy);


                    string[] notFilterHeaders = { "Id", "Telefon", "Uprawnienia" };
                    var newStyle = new Style(typeof(GridViewColumnHeader));
                    var actualSetters = headerClicked.Column.HeaderContainerStyle.Setters.OfType<Setter>().Where(x => x.Property == TagProperty || x.Property == TabIndexProperty).ToList();
                    if (direction == ListSortDirection.Ascending)
					{
                        if (notFilterHeaders.Contains(columnHeader))
                            newStyle.BasedOn = Application.Current.Resources["ListHeaderWithoutFilterAndUpArrow"] as Style;
                        else
                            newStyle.BasedOn = Application.Current.Resources["ListHeaderWithFilterAndUpArrow"] as Style;
                    }
					else
					{
                        if (notFilterHeaders.Contains(columnHeader))
                            newStyle.BasedOn = Application.Current.Resources["ListHeaderWithoutFilterAndDownArrow"] as Style;
                        else
                            newStyle.BasedOn = Application.Current.Resources["ListHeaderWithFilterAndDownArrow"] as Style;
                    }
                    foreach (var setter in actualSetters)
                        newStyle.Setters.Add(setter);
                    headerClicked.Column.HeaderContainerStyle = newStyle;

                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
					{
                        //_lastHeaderClicked.Column.HeaderTemplate = null;
                        var lastHeaderNewStyle = new Style(typeof(GridViewColumnHeader));
                        var lastSetter = _lastHeaderClicked.Column.HeaderContainerStyle.Setters.OfType<Setter>().Where(x => x.Property == TagProperty || x.Property == TabIndexProperty).ToList();
                        var lastColumnHeader = _lastHeaderClicked.Column.Header.ToString();
                        if (notFilterHeaders.Contains(lastColumnHeader))
                            lastHeaderNewStyle.BasedOn = Application.Current.Resources["ListHeaderWithoutFilter"] as Style;
                        else
                            lastHeaderNewStyle.BasedOn = Application.Current.Resources["ListHeaderWithFilter"] as Style;
                        foreach (var setter in lastSetter)
                            lastHeaderNewStyle.Setters.Add(setter);
                        _lastHeaderClicked.Column.HeaderContainerStyle = lastHeaderNewStyle;
                    }

					_lastHeaderClicked = headerClicked;
					_lastDirection = direction;
				}
			}
		}
    }
}
