using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace Модуль_6_Воробьёв
{
    public partial class FinancialRecordForm : Window
    {
        private string connectionString = "Data Source=FinancialRecords.db;Version=3;"; // Путь к базе данных

        public FinancialRecordForm()
        {
            InitializeComponent();
            CreateDatabaseIfNotExists(); // Создание базы данных при необходимости
            LoadFinancialRecords(); // Загрузка данных при запуске формы
        }

        // Метод для создания базы данных, если она не существует
        private void CreateDatabaseIfNotExists()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS FinancialRecords (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT NOT NULL,
                            Type TEXT NOT NULL CHECK(Type IN ('Доход', 'Расход')),
                            Amount REAL NOT NULL,
                            Description TEXT
                        )";
                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Ошибка при создании базы данных: " + ex.Message);
                }
            }
        }

        // Метод для сохранения записи в базу данных
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранную дату, тип операции, сумму и описание
            string date = DatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            string type = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string amountText = AmountTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();

            // Проверка заполненности всех полей и корректности ввода
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(amountText) || !decimal.TryParse(amountText, out decimal amount))
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.");
                return;
            }

            // Проверка корректности типа операции (доход или расход)
            if (type != "Доход" && type != "Расход")
            {
                MessageBox.Show("Выберите корректный тип операции (Доход или Расход).");
                return;
            }

            try
            {
                // Добавляем запись в базу данных с русским типом операции
                AddRecordToDatabase(date, type, amount, description);
                MessageBox.Show("Запись сохранена.");
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Ошибка при сохранении записи: " + ex.Message);
            }

            // Очистка полей после сохранения
            DatePicker.SelectedDate = null;
            TypeComboBox.SelectedIndex = -1;
            AmountTextBox.Clear();
            DescriptionTextBox.Clear();

            // Перезагружаем данные после сохранения
            LoadFinancialRecords();
        }

        // Метод для добавления записи в базу данных
        private void AddRecordToDatabase(string date, string type, decimal amount, string description)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO FinancialRecords (Date, Type, Amount, Description) VALUES (@Date, @Type, @Amount, @Description)";
                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        // Добавляем параметры
                        command.Parameters.AddWithValue("@Date", date);
                        command.Parameters.AddWithValue("@Type", type);  // Оставляем тип как есть, на русском
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@Description", description);

                        // Выполняем запрос
                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Ошибка при добавлении записи в базу данных: " + ex.Message);
                }
            }
        }

        // Метод для загрузки данных из базы данных в DataGrid
        private void LoadFinancialRecords()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Id, Date, Type, Amount, Description FROM FinancialRecords";
                    using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, conn))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // Преобразуем столбец Type (тип операции) из англоязычных значений в русскоязычные
                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (row["Type"].ToString() == "Income")
                            {
                                row["Type"] = "Доход";
                            }
                            else if (row["Type"].ToString() == "Expense")
                            {
                                row["Type"] = "Расход";
                            }
                        }

                        // Отображаем данные в DataGrid
                        FinancialRecordsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }
    }
}