using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MISReports;

namespace MISReportsGUI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private ItemUser currentUser = null;
		public ObservableCollection<ItemReport> ReportsList { get; set; } = new ObservableCollection<ItemReport>();

		public MainWindow() {
			InitializeComponent();

			Logging.ToLog(new string('-', 40));
			Logging.ToLog("Запуск, пользователь: " + Environment.UserName + ", система: " + Environment.MachineName);

			DataGridReports.DataContext = this;
			DataGridReports.Items.SortDescriptions.Add(
				new System.ComponentModel.SortDescription(
					nameof(MISReports.ItemReport.Name), System.ComponentModel.ListSortDirection.Ascending));
			DatePickerBegin.SelectedDate = DateTime.Now;
			DatePickerEnd.SelectedDate = DateTime.Now;

			Loaded += (s, e) => { UpdateReportList(); };
		}

		private void UpdateReportList() {
			ReportsList.Clear();
			currentUser = null;

			string currentUserName = Environment.UserName.ToLower();
			foreach (ItemUser item in UserList.Instance.Users) {
				if (item.UserName.ToLower().StartsWith(currentUserName)) {
					currentUser = item;
					break;
				}
			}

			if (currentUser == null) {
				MessageBox.Show(this, "Для Вас не заданы права доступа к отчетам. Пожалуйста, обратитесь в службу технической поддержки.",
					"", MessageBoxButton.OK, MessageBoxImage.Information);
			} else {
				if (currentUser.IsAdministrator)
					foreach (ReportsInfo.Type type in Enum.GetValues(typeof(ReportsInfo.Type)))
						ReportsList.Add(new ItemReport(type.ToString()));
				else
					foreach (ReportsInfo.Type type in currentUser.ReportsTypesAvailable)
						ReportsList.Add(new ItemReport(type.ToString()));
			}
		}

		private void ButtonMailTo_Click(object sender, RoutedEventArgs e) {
			ItemReport itemReport = (sender as Button).DataContext as ItemReport;
			if (itemReport == null)
				return;

			string title = itemReport.Name + ", Получатели:";
			string text = itemReport.MailTo != null ? itemReport.MailTo.Replace(";", Environment.NewLine) : string.Empty;
			WindowDetails windowDetails = new WindowDetails(title, text);
			windowDetails.Owner = this;
			windowDetails.ShowDialog();
		}

		private void ButtonSqlQuery_Click(object sender, RoutedEventArgs e) {
			ItemReport itemReport = (sender as Button).DataContext as ItemReport;
			if (itemReport == null)
				return;

			string title = itemReport.Name + ", Запрос:";
			string text = itemReport.SqlQuery;
			WindowDetails windowDetails = new WindowDetails(title, text);
			windowDetails.Owner = this;
			windowDetails.ShowDialog();
		}

		private void ButtonTemplate_Click(object sender, RoutedEventArgs e) {
			ItemReport itemReport = (sender as Button).DataContext as ItemReport;
			if (itemReport == null)
				return;

			try {
				if (!string.IsNullOrEmpty(itemReport.TemplateFileName))
					Process.Start(
						Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Templates"),
						itemReport.TemplateFileName));
			} catch (Exception exc) {
				MessageBox.Show(this, itemReport.TemplateFileName + Environment.NewLine +
					exc.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ButtonCreate_Click(object sender, RoutedEventArgs e) {
			string errorMessage = string.Empty;

			if (DataGridReports.SelectedItem == null)
				errorMessage = "Не выбран отчет для формирования" + Environment.NewLine;

			if (!DatePickerBegin.SelectedDate.HasValue)
				errorMessage += "Не выбрана дата начала" + Environment.NewLine;

			if (!DatePickerEnd.SelectedDate.HasValue)
				errorMessage += "Не выбрана дата окончания" + Environment.NewLine;

			if (DatePickerBegin.SelectedDate.HasValue &&
				DatePickerEnd.SelectedDate.HasValue &&
				DatePickerBegin.SelectedDate.Value > DatePickerEnd.SelectedDate.Value)
				errorMessage += "Дата окончания не быть может быть меньше даты начала";

			if (!string.IsNullOrEmpty(errorMessage)) {
				MessageBox.Show(this, errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			ItemReport itemReport = DataGridReports.SelectedItem as ItemReport;
			itemReport.SetPeriod(DatePickerBegin.SelectedDate.Value, DatePickerEnd.SelectedDate.Value);
			string title = itemReport.Name + ", формирование за период с " + 
				itemReport.DateBegin.ToShortDateString() + " по " + itemReport.DateEnd.ToShortDateString();
			WindowDetails windowDetails = new WindowDetails(title, string.Empty, currentUser, itemReport);
			windowDetails.Owner = this;
			windowDetails.ShowDialog();
		}

		private void ButtonAccessRights_Click(object sender, RoutedEventArgs e) {
			bool? isAccesGrunted = true;
			bool? dialogResult = false;

			if (currentUser == null || !currentUser.IsAdministrator) {
				WindowEnterPassword windowEnterPassword = new WindowEnterPassword();
				windowEnterPassword.Owner = this;
				isAccesGrunted = windowEnterPassword.ShowDialog();
			}

			if (isAccesGrunted == true) {
				WindowAccessRights windowAccessRights = new WindowAccessRights();
				windowAccessRights.Owner = this;
				dialogResult = windowAccessRights.ShowDialog();
			}

			if (dialogResult == true)
				UpdateReportList();
		}

		private void ButtonDateSelect_Click(object sender, RoutedEventArgs e) {
			string param = (sender as Button).Tag.ToString();
			DateTime dateBegin = DatePickerBegin.SelectedDate.Value;
			DateTime dateEnd = DatePickerEnd.SelectedDate.Value;

			if (param.Equals("EquateEndDateToBeginDate")) {
				dateEnd = dateBegin;
			} else if (param.Equals("SetDatesToCurrentDay")) {
				dateBegin = DateTime.Now;
				dateEnd = dateBegin;
			} else if (param.Equals("SetDatesToCurrentWeek")) {
				dateEnd = DateTime.Now;
				int dayOfWeek = (int)dateEnd.DayOfWeek;
				if (dayOfWeek == 0)
					dayOfWeek = 7;
				dateBegin = dateEnd.AddDays(-1 * (dayOfWeek - 1));
			} else if (param.Equals("SetDatesToCurrentMonth")) {
				dateBegin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				dateEnd = dateBegin.AddDays(DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) - 1);
			} else if (param.Equals("SetDatesToCurrentYear")) {
				dateBegin = new DateTime(DateTime.Now.Year, 1, 1);
				dateEnd = new DateTime(DateTime.Now.Year, 12, DateTime.DaysInMonth(DateTime.Now.Year, 12));
			} else if (param.Equals("GoToPreviousMonth")) {
				dateEnd = new DateTime(dateBegin.Year, dateBegin.Month, 1).AddDays(-1);
				dateBegin = dateEnd.AddDays(-1 * (DateTime.DaysInMonth(dateEnd.Year, dateEnd.Month) - 1));
			} else if (param.Equals("GoToPreviousDay")) {
				dateBegin = dateBegin.AddDays(-1);
				dateEnd = dateBegin;
			} else if (param.Equals("GoToNextDay")) {
				dateBegin = dateBegin.AddDays(1);
				dateEnd = dateBegin;
			} else if (param.Equals("GoToNextMonth")) {
				dateBegin = new DateTime(dateBegin.Year, dateBegin.Month, DateTime.DaysInMonth(dateBegin.Year, dateBegin.Month)).AddDays(1);
				dateEnd = dateBegin.AddDays((DateTime.DaysInMonth(dateBegin.Year, dateBegin.Month) - 1));
			}

			DatePickerBegin.SelectedDate = dateBegin;
			DatePickerEnd.SelectedDate = dateEnd;
		}
	}
}
