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
		public ObservableCollection<ItemReport> ReportsList { get; set; } = new ObservableCollection<ItemReport>();

		public MainWindow() {
			InitializeComponent();
			DataGridReports.DataContext = this;

			foreach (ReportsInfo.Type type in Enum.GetValues(typeof(ReportsInfo.Type))) {
				ReportsList.Add(new ItemReport(type.ToString()));
			}
		}

		private void ButtonMailTo_Click(object sender, RoutedEventArgs e) {
			ItemReport itemReport = (sender as Button).DataContext as ItemReport;
			if (itemReport == null)
				return;

			string title = itemReport.Name + ", Получатели:";
			string text = itemReport.MailTo.Replace(";", Environment.NewLine);
			WindowDetails windowDetails = new WindowDetails(title, text, this);
			windowDetails.ShowDialog();
		}

		private void ButtonSqlQuery_Click(object sender, RoutedEventArgs e) {
			ItemReport itemReport = (sender as Button).DataContext as ItemReport;
			if (itemReport == null)
				return;

			string title = itemReport.Name + ", Запрос:";
			string text = itemReport.SqlQuery;
			WindowDetails windowDetails = new WindowDetails(title, text, this);
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
			WindowDetails windowDetails = new WindowDetails(title, string.Empty, this, itemReport);
			windowDetails.ShowDialog();
		}
	}
}
