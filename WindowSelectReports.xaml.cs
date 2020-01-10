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
using MISReports;

namespace MISReportsGUI {
	/// <summary>
	/// Interaction logic for WindowSelectReports.xaml
	/// </summary>
	public partial class WindowSelectReports : Window {
		public ObservableCollection<string> ReportsAvailable { get; set; } = new ObservableCollection<string>();
		public ObservableCollection<string> ReportsSelected { get; set; } = new ObservableCollection<string>();
		private readonly ItemUser user;

		public WindowSelectReports(ItemUser user) {
			InitializeComponent();
			DataContext = this;
			this.user = user;

			foreach (ReportsInfo.Type type in Enum.GetValues(typeof(ReportsInfo.Type))) {
				string report = ReportsInfo.AcceptedParameters[type] + " (" + type.ToString() + ")";

				if (user.ReportsTypesAvailable.Contains(type))
					ReportsSelected.Add(report);
				else
					ReportsAvailable.Add(report);
			}

			ButtonAllToAvailable.IsEnabled = ReportsSelected.Count > 0;
			ButtonAllToSelected.IsEnabled = ReportsAvailable.Count > 0;
			ReportsAvailable.CollectionChanged += Reports_CollectionChanged;
			ReportsSelected.CollectionChanged += Reports_CollectionChanged;

			ListBoxReportsAvailable.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
			ListBoxReportsSelected.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
		}

		private void Reports_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			ButtonAllToAvailable.IsEnabled = ReportsSelected.Count > 0;
			ButtonAllToSelected.IsEnabled = ReportsAvailable.Count > 0;
		}

		private void ButtonMoveReports_Click(object sender, RoutedEventArgs e) {
			List<string> items = new List<string>();
			ObservableCollection<string> source;
			ObservableCollection<string> destination;

			if (sender == ButtonOneToSelected) {
				source = ReportsAvailable;
				destination = ReportsSelected;
				foreach (string item in ListBoxReportsAvailable.SelectedItems) 
					items.Add(item);
				
			} else if (sender == ButtonAllToSelected) {
				source = ReportsAvailable;
				destination = ReportsSelected;
				foreach (string item in ReportsAvailable) 
					items.Add(item);

			} else if (sender == ButtonAllToAvailable) {
				source = ReportsSelected;
				destination = ReportsAvailable;
				foreach (string item in ReportsSelected)
					items.Add(item);

			} else /*(sender == ButtonOneToAvailable)*/ {
				source = ReportsSelected;
				destination = ReportsAvailable;
				foreach (string item in ListBoxReportsSelected.SelectedItems)
					items.Add(item);
			}

			foreach (string item in items) {
				source.Remove(item);
				destination.Add(item);
			}
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e) {
			user.ReportsTypesAvailable.Clear();
			foreach (ReportsInfo.Type type in Enum.GetValues(typeof(ReportsInfo.Type))) 
				foreach (string item in ReportsSelected) {
					if (item.Contains(type.ToString())) {
						user.ReportsTypesAvailable.Add(type);
						break;
					}
				}
			
			Close();
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonOneToAvailable.IsEnabled = ListBoxReportsSelected.SelectedItems.Count > 0;
			ButtonOneToSelected.IsEnabled = ListBoxReportsAvailable.SelectedItems.Count > 0;
		}

		private void ListBoxReportsAvailable_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (sender == ListBoxReportsAvailable) {
				string selected = ListBoxReportsAvailable.SelectedItem as string;
				ReportsAvailable.Remove(selected);
				ReportsSelected.Add(selected);
			} else {
				string selected = ListBoxReportsSelected.SelectedItem as string;
				ReportsSelected.Remove(selected);
				ReportsAvailable.Add(selected);
			}
		}
	}
}
