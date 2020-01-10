using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MISReports;

namespace MISReportsGUI {
	[Serializable]
	public class ItemUser : INotifyPropertyChanged {
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string Name { get; private set; }

		public string ReportsAvailable {
			get {
				if (IsAdministrator)
					return "Все";
				else
					return ReportsTypesAvailable.Count.ToString() + " шт.";
			}
		}

		public ObservableCollection<ReportsInfo.Type> ReportsTypesAvailable { get; set; } = new ObservableCollection<ReportsInfo.Type>();
		public string UserName { get; private set; }

		public bool IsEditingEnabled { get { return !IsAdministrator; } }

		private bool isAdministrator = false;
		public bool IsAdministrator {
			get { return isAdministrator; }
			set {
				if (value != isAdministrator) {
					isAdministrator = value;
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(ReportsAvailable));
					NotifyPropertyChanged(nameof(IsEditingEnabled));
				}
			}
		}
		public string Company { get; set; }
		public string Department { get; set; }
		public string Title { get; set; }

		public ItemUser(string name, string userName) {
			Name = name;
			UserName = userName;
			ReportsTypesAvailable.CollectionChanged += (s, e) => {
				NotifyPropertyChanged(nameof(ReportsAvailable));
			};
		}

		public string FolderToSaveReports { get; set; } = @"C:\Temp\MisReports\";
	}
}
