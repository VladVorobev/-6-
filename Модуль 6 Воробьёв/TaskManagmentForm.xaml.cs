using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Модуль_6_Воробьёв
{
    public partial class TaskManagementForm : Window
    {
        private string taskDbConnectionString = "Data Source=tasks.db;Version=3;";
        private List<Task> tasks = new List<Task>();

        public TaskManagementForm()
        {
            InitializeComponent();
            CreateTaskTable();
            LoadTasks();
        }
        // Метод для перехода на форму с книгами
        private void OpenBookFormButton_Click(object sender, RoutedEventArgs e)
        {
            BookRentalForm bookForm = new BookRentalForm();
            bookForm.Show(); // Открываем форму с книгами
        }
        // Создание таблицы задач
        private void CreateTaskTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(taskDbConnectionString))
            {
                conn.Open();
                string sql = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TaskName TEXT NOT NULL,
                    DueDate TEXT NOT NULL,
                    Status TEXT NOT NULL)";

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Загрузка задач из базы данных
        private void LoadTasks()
        {
            tasks.Clear();
            using (SQLiteConnection conn = new SQLiteConnection(taskDbConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Tasks";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new Task
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                TaskName = reader["TaskName"].ToString(),
                                DueDate = DateTime.Parse(reader["DueDate"].ToString()), // Преобразуем строку в DateTime
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }

            TasksDataGrid.ItemsSource = null;
            TasksDataGrid.ItemsSource = tasks;
        }

        // Добавление новой задачи
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameTextBox.Text) ||
                DueDatePicker.SelectedDate == null ||
                StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection(taskDbConnectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO Tasks (TaskName, DueDate, Status)
                    VALUES (@TaskName, @DueDate, @Status)";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@TaskName", TaskNameTextBox.Text);
                    command.Parameters.AddWithValue("@DueDate", DueDatePicker.SelectedDate?.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Status", (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
                    command.ExecuteNonQuery();
                }
            }

            LoadTasks();
        }

        // Обновление задачи
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksDataGrid.SelectedItem is Task selectedTask)
            {
                string updatedTaskName = TaskNameTextBox.Text.Trim();
                DateTime? updatedDueDate = DueDatePicker.SelectedDate;
                string updatedStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

                if (string.IsNullOrWhiteSpace(updatedTaskName) || !updatedDueDate.HasValue || updatedStatus == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.");
                    return;
                }

                using (SQLiteConnection conn = new SQLiteConnection(taskDbConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        UPDATE Tasks
                        SET TaskName = @TaskName, DueDate = @DueDate, Status = @Status
                        WHERE Id = @Id";
                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@Id", selectedTask.Id);
                        command.Parameters.AddWithValue("@TaskName", updatedTaskName);
                        command.Parameters.AddWithValue("@DueDate", updatedDueDate.Value.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Status", updatedStatus);
                        command.ExecuteNonQuery();
                    }
                }

                LoadTasks();
            }
            else
            {
                MessageBox.Show("Выберите задачу для обновления.");
            }
        }

        // Удаление задачи
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksDataGrid.SelectedItem is Task selectedTask)
            {
                using (SQLiteConnection conn = new SQLiteConnection(taskDbConnectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Tasks WHERE Id = @Id";
                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@Id", selectedTask.Id);
                        command.ExecuteNonQuery();
                    }
                }

                LoadTasks();
            }
            else
            {
                MessageBox.Show("Выберите задачу для удаления.");
            }
        }

        // Метод для обработки события GotFocus (когда фокус на TextBox)
        private void TaskNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskNameTextBox.Text == "")
            {
                TaskNamePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        // Метод для обработки события LostFocus (когда фокус уходит от TextBox)
        private void TaskNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameTextBox.Text))
            {
                TaskNamePlaceholder.Visibility = Visibility.Visible;
            }
        }

        // Обработка выбора задачи в DataGrid
        private void TasksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksDataGrid.SelectedItem is Task selectedTask)
            {
                TaskNameTextBox.Text = selectedTask.TaskName;
                DueDatePicker.SelectedDate = selectedTask.DueDate;

                // Устанавливаем выбранный элемент в ComboBox на основе тега
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Tag.ToString() == selectedTask.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // Класс для задачи
        public class Task
        {
            public int Id { get; set; }
            public string TaskName { get; set; }
            public DateTime DueDate { get; set; }
            public string Status { get; set; }
        }
    }
}