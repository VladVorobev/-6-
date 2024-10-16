using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace Модуль_6_Воробьёв
{
    public partial class MainWindow : Window
    {
        private string dbConnectionString = "Data Source=employees.db;Version=3;";
        private List<Employee> employees = new List<Employee>();

        public MainWindow()
        {
            InitializeComponent();
            CreateDatabaseAndTable();
            LoadEmployees();
        }
        // Обработчик кнопки для перехода к TaskManagementForm
        private void GoToTaskFormButton_Click(object sender, RoutedEventArgs e)
        {
            // Создание и показ новой формы
            TaskManagementForm taskForm = new TaskManagementForm();
            taskForm.Show();
        }
        // Создание базы данных и таблицы сотрудников, если их еще нет
        private void CreateDatabaseAndTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(dbConnectionString))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Employees (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        Age INTEGER NOT NULL,
                        BirthDate TEXT NOT NULL,  -- Оставляем как TEXT для совместимости с SQLite, но будем конвертировать в DateTime
                        Position TEXT NOT NULL,
                        Address TEXT NOT NULL,
                        PhoneNumber TEXT NOT NULL)";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Загрузка сотрудников из базы данных
        private void LoadEmployees()
        {
            employees.Clear();
            using (SQLiteConnection conn = new SQLiteConnection(dbConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Employees";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FullName = reader["FullName"].ToString(),
                                Age = Convert.ToInt32(reader["Age"]),
                                BirthDate = reader["BirthDate"].ToString(),  // Сохраняем как строку, но конвертируем в DateTime при необходимости
                                Position = reader["Position"].ToString(),
                                Address = reader["Address"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString()
                            });
                        }
                    }
                }
            }

            EmployeesDataGrid.ItemsSource = null;
            EmployeesDataGrid.ItemsSource = employees;
        }

        // Добавление нового сотрудника
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(AgeTextBox.Text) ||
                string.IsNullOrWhiteSpace(BirthDatePicker.Text) ||
                string.IsNullOrWhiteSpace(PositionTextBox.Text) ||
                string.IsNullOrWhiteSpace(AddressTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (!int.TryParse(AgeTextBox.Text, out int age))
            {
                MessageBox.Show("Возраст должен быть числом.");
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection(dbConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO Employees (FullName, Age, BirthDate, Position, Address, PhoneNumber)
                    VALUES (@FullName, @Age, @BirthDate, @Position, @Address, @PhoneNumber)";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@FullName", FullNameTextBox.Text);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@BirthDate", BirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Position", PositionTextBox.Text);
                    command.Parameters.AddWithValue("@Address", AddressTextBox.Text);
                    command.Parameters.AddWithValue("@PhoneNumber", PhoneNumberTextBox.Text);
                    command.ExecuteNonQuery();
                }
            }

            LoadEmployees();
            ClearInputFields();
        }

        // Обработка изменения данных в DataGrid
        private void EmployeesDataGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == System.Windows.Controls.DataGridEditAction.Commit)
            {
                var editedEmployee = e.Row.Item as Employee;
                if (editedEmployee != null)
                {
                    // Отладочное сообщение для проверки изменений
                    MessageBox.Show($"Updating Employee with ID {editedEmployee.Id}: FullName: {editedEmployee.FullName}, Age: {editedEmployee.Age}");

                    using (SQLiteConnection conn = new SQLiteConnection(dbConnectionString))
                    {
                        conn.Open();
                        string sql = @"
                    UPDATE Employees 
                    SET FullName = @FullName, Age = @Age, BirthDate = @BirthDate, 
                        Position = @Position, Address = @Address, PhoneNumber = @PhoneNumber 
                    WHERE Id = @Id";

                        using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                        {
                            // Отладка: проверка значений перед обновлением
                            command.Parameters.AddWithValue("@FullName", editedEmployee.FullName);
                            command.Parameters.AddWithValue("@Age", editedEmployee.Age); // Убедитесь, что это значение правильное
                            command.Parameters.AddWithValue("@BirthDate", editedEmployee.BirthDate);
                            command.Parameters.AddWithValue("@Position", editedEmployee.Position);
                            command.Parameters.AddWithValue("@Address", editedEmployee.Address);
                            command.Parameters.AddWithValue("@PhoneNumber", editedEmployee.PhoneNumber);
                            command.Parameters.AddWithValue("@Id", editedEmployee.Id);

                            // Выполняем запрос на обновление
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                MessageBox.Show("Ошибка при обновлении записи.");
                            }
                            else
                            {
                                MessageBox.Show("Данные успешно обновлены.");
                            }
                        }
                    }

                    // Важно обновить только отредактированную запись в DataGrid
                    e.Row.Item = editedEmployee;
                }
            }
        }

        // Удаление выбранного сотрудника
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee selectedEmployee)
            {
                using (SQLiteConnection conn = new SQLiteConnection(dbConnectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Employees WHERE Id = @Id";
                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@Id", selectedEmployee.Id);
                        command.ExecuteNonQuery();
                    }
                }

                LoadEmployees();
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления.");
            }
        }

        // Очистка полей ввода
        private void ClearInputFields()
        {
            FullNameTextBox.Clear();
            AgeTextBox.Clear();
            BirthDatePicker.SelectedDate = null;
            PositionTextBox.Clear();
            AddressTextBox.Clear();
            PhoneNumberTextBox.Clear();
        }

        // Класс для сотрудника
        public class Employee
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public int Age { get; set; }
            public string BirthDate { get; set; }  // Храним как строку для совместимости с SQLite
            public string Position { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
        }
    }
}