using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
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
using System.Windows.Shapes;

namespace MISReportsGUI {
	/// <summary>
	/// Interaction logic for WindowSelectUser.xaml
	/// </summary>
	public partial class WindowSelectUser : Window {
		public ObservableCollection<ItemUser> UserList { get; set; } = new ObservableCollection<ItemUser>();
		public ItemUser SelectedUser { get; set; }
		public WindowSelectUser() {
			InitializeComponent();
			DataContext = this;
			TextBoxSearch.Focus();
		}

		private void ButtonSelect_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}

		private async void ButtonSearch_Click(object sender, RoutedEventArgs e) {
			UserList.Clear();

			if (TextBoxSearch.Text.Length == 0)
				return;

			Cursor = Cursors.Wait;
			List<ItemUser> users = new List<ItemUser>();
			string userToSearch = TextBoxSearch.Text;

			await Task.Run(() => {
				using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + Domain.GetComputerDomain().ToString()))
				using (DirectorySearcher searcher = new DirectorySearcher(entry)) {
					searcher.SearchScope = SearchScope.Subtree;
					searcher.Filter = "(&(objectClass=User) (name=*" + userToSearch + "*))";
					searcher.SizeLimit = int.MaxValue;
					searcher.PageSize = int.MaxValue;

					foreach (SearchResult result in searcher.FindAll()) {
						try {
							string name = result.Properties["name"][0].ToString();
							string userName = result.Properties["userPrincipalName"][0].ToString();

							string company = string.Empty;
							try {
								if (result.Properties.Contains("company"))
									company = result.Properties["company"][0].ToString();
							} catch (Exception) {}

							string department = string.Empty;
							try {
								if (result.Properties.Contains("department"))
									department = result.Properties["department"][0].ToString();
							} catch (Exception) {}

							string title = string.Empty;
							try {
								if (result.Properties.Contains("title"))
									title = result.Properties["title"][0].ToString();
							} catch (Exception) {}
							
							ItemUser itemUser = new ItemUser(name, userName) {
								Company = company,
								Department = department,
								Title = title
							};

							users.Add(itemUser);
						} catch (Exception) {}
					}
				}
			});

			users.OrderBy(x => x.Name).ToList().ForEach(UserList.Add);
			Cursor = Cursors.Arrow;
		}

		private void TextBoxSearch_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter)
				ButtonSearch_Click(null, null);
		}

		private void DataGridUsers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonSelect.IsEnabled = DataGridUsers.SelectedItems.Count > 0;
		}

		private void DataGridUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ButtonSelect_Click(null, null);
		}
	}
}
