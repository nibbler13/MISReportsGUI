using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class WindowAccessRights : Window {
		public WindowAccessRights() {
			InitializeComponent();
			DataGridUsers.DataContext = UserList.Instance;
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e) {
			UserList.SaveConfiguration();
			DialogResult = true;
			Close();
		}

		private void ButtonDeleteUser_Click(object sender, RoutedEventArgs e) {
			UserList.Instance.Users.Remove(DataGridUsers.SelectedItem as ItemUser);
		}

		private void ButtonAddUser_Click(object sender, RoutedEventArgs e) {
			WindowSelectUser windowSelectUser = new WindowSelectUser();
			windowSelectUser.Owner = this;
			if (windowSelectUser.ShowDialog() == true) {
				if (windowSelectUser.SelectedUser != null)
					UserList.Instance.Users.Add(windowSelectUser.SelectedUser);
			}
		}

		private void DataGridUsers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonDeleteUser.IsEnabled = DataGridUsers.SelectedItems.Count > 0;
		}

		private void ButtonSelectReports_Click(object sender, RoutedEventArgs e) {
			Button button = sender as Button;
			ItemUser user = button.DataContext as ItemUser;
			WindowSelectReports windowSelectReports = new WindowSelectReports(user);
			windowSelectReports.Owner = this;
			windowSelectReports.ShowDialog();
		}
	}
}
