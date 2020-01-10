using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MISReportsGUI {
	[Serializable]
	class UserList {
		public bool IsConfigReadedSuccessfull { get; set; } = false;
		private static UserList instance = null;
		private static readonly object padlock = new object();

		private string configFilePath;

		public static UserList Instance {
			get {
				lock (padlock) {
					if (instance == null)
						instance = LoadConfiguration();

					return instance;
				}
			}
		}

		public ObservableCollection<ItemUser> Users { get; set; } = new ObservableCollection<ItemUser>();

		private static UserList LoadConfiguration() {
			UserList configuration = new UserList();

			if (!File.Exists(configuration.configFilePath)) {
				return configuration;
			}

			try {

				byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 };
				byte[] iv = { 1, 2, 3, 4, 5, 6, 7, 8 };

				using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
				using (var fs = new FileStream(configuration.configFilePath, FileMode.Open, FileAccess.Read))
				using (var cryptoStream = new CryptoStream(fs, des.CreateDecryptor(key, iv), CryptoStreamMode.Read)) {
					BinaryFormatter formatter = new BinaryFormatter();
					configuration = (UserList)formatter.Deserialize(cryptoStream);
					configuration.IsConfigReadedSuccessfull = true;
				};
			} catch (Exception e) {
				MessageBox.Show("Configuration - !!! " + e.Message + Environment.NewLine + e.StackTrace, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return configuration;
		}

		public static bool SaveConfiguration() {
			byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8 };
			byte[] iv = { 1, 2, 3, 4, 5, 6, 7, 8 };
			try {
				using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
				using (var fs = new FileStream(Instance.configFilePath, FileMode.OpenOrCreate, FileAccess.Write))
				using (var cryptoStream = new CryptoStream(fs, des.CreateEncryptor(key, iv), CryptoStreamMode.Write)) {
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(cryptoStream, Instance);
				};

				MessageBox.Show("Изменения сохранены", string.Empty,
					MessageBoxButton.OK, MessageBoxImage.Information);

				return true;
			} catch (Exception e) {
				MessageBox.Show("Не удалось сохранить конфигурацию: " + e.Message,
					"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

				return false;
			}
		}

		private UserList() {
			configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "UserList.obj");
		}
	}
}
