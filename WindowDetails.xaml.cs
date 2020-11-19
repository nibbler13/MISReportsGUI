using MISReports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MISReportsGUI {
	/// <summary>
	/// Interaction logic for WindowDetails.xaml
	/// </summary>
	public partial class WindowDetails : Window {
		private BackgroundWorker bw;
		private ItemUser itemUser;

		public WindowDetails(string title, string text, ItemUser itemUser = null, ItemReport itemReport = null) {
			InitializeComponent();
			Title = title;
			TextBoxMain.Text = text;
			this.itemUser = itemUser;

			if (itemReport != null) {
				CreateReport(itemReport);
				ButtonClose.IsEnabled = false;
				Cursor = Cursors.Wait;
			}

			Closing += (s, e) => {
				if (bw != null && bw.IsBusy)
					bw.CancelAsync();
			};
		}

		private void ButtonClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void CreateReport(ItemReport itemReport) {
			if (itemUser != null) {
				if (!Directory.Exists(itemUser.FolderToSaveReports)) {
					try {
						Directory.CreateDirectory(itemUser.FolderToSaveReports);
					} catch (Exception) {
						MessageBox.Show(this, "Не удалось создать папку для сохранения отчетов: " + itemUser.FolderToSaveReports);

						CommonOpenFileDialog dialog = new CommonOpenFileDialog();
						dialog.IsFolderPicker = true;
						if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
							itemUser.FolderToSaveReports = dialog.FileName;
							UserList.SaveConfiguration();
						} else {
							MessageBox.Show(this, "Невозможно продолжить формирование т.к. не выбрана папка для сохранения", "", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
					}
				}
			}

			bw = new BackgroundWorker {
				WorkerReportsProgress = true
			};

			bw.ProgressChanged += (s, e) => {
				if (e.UserState != null) {
					TextBoxMain.Text += e.UserState.ToString() + Environment.NewLine;
					TextBoxMain.ScrollToEnd();
				}
			};

			bw.DoWork += (s, e) => {
				Program.CreateReport(itemReport, true);// itemUser != null ? itemUser.IsAdministrator : false);
			};

			bw.RunWorkerCompleted += (s, e) => {
				if (e.Error != null) {
					MessageBox.Show(this, e.Error.Message + Environment.NewLine + e.Error.StackTrace, "", 
						MessageBoxButton.OK, MessageBoxImage.Error);
				} else {
					MessageBox.Show(this, "Завершено", "",
						MessageBoxButton.OK, MessageBoxImage.Information);
				}

				ButtonClose.IsEnabled = true;
				Cursor = Cursors.Arrow;

				if (!string.IsNullOrEmpty(itemReport.FileResult)) {
					try {
						//string fileName = Path.GetFileName(itemReport.FileResult);
						//string newDestination = Path.Combine(itemUser.FolderToSaveReports, fileName);
						//TextBoxMain.Text += Environment.NewLine + "Перемещение файла в папку: " + itemUser.FolderToSaveReports + Environment.NewLine;
						//File.Move(itemReport.FileResult, newDestination);
						string argument = "/select, \"" + Path.GetDirectoryName(itemReport.FileResult) + "\"";
						Process.Start("explorer.exe", argument);
					} catch (Exception exc) {
						MessageBox.Show(this, exc.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			};

			Logging.bw = bw;
			bw.RunWorkerAsync();
		}
	}
}
