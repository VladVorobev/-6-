using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace Модуль_6_Воробьёв
{
    public partial class BookRentalForm : Window
    {
        private string connectionString = "Data Source=Books.db;Version=3;"; // Путь к базе данных

        public BookRentalForm()
        {
            InitializeComponent();
            CreateDatabaseIfNotExists(); // Проверка и создание базы данных при необходимости
        }
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Создание и показ новой формы
            FinancialRecordForm taskForm = new FinancialRecordForm();
            taskForm.Show();
        }
        // Метод для создания базы данных, если она не существует
        private void CreateDatabaseIfNotExists()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Books (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Genre TEXT NOT NULL,
                        Available INTEGER NOT NULL DEFAULT 1
                    )";
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод для поиска книг
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Введите название, автора или жанр для поиска.");
                return;
            }

            List<Book> books = SearchBooks(searchTerm);
            BooksDataGrid.ItemsSource = books;
        }

        // Метод для поиска книг в базе данных
        private List<Book> SearchBooks(string searchTerm)
        {
            List<Book> books = new List<Book>();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Books WHERE (Title LIKE @SearchTerm OR Author LIKE @SearchTerm OR Genre LIKE @SearchTerm)";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(new Book
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                Genre = reader["Genre"].ToString(),
                                Status = reader["Available"].ToString() == "1" ? "Доступна" : "Арендована"
                            });
                        }
                    }
                }
            }

            return books;
        }

        // Метод для аренды книги
        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book selectedBook)
            {
                if (selectedBook.Status == "Доступна")
                {
                    RentBook(selectedBook.Id);  // Обновляем статус книги в базе данных
                    selectedBook.Status = "Арендована";  // Обновляем статус книги в DataGrid

                    // Обновляем отображаемую информацию на экране
                    BooksDataGrid.Items.Refresh();  // Обновляем отображение в DataGrid
                    MessageBox.Show("Книга успешно арендована.");
                }
                else
                {
                    MessageBox.Show("Эта книга уже арендована.");
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для аренды.");
            }
        }

        // Метод для изменения статуса книги в базе данных
        private void RentBook(int bookId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE Books SET Available = 0 WHERE Id = @Id";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Id", bookId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод для возврата книги
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book selectedBook)
            {
                if (selectedBook.Status == "Арендована")
                {
                    ReturnBook(selectedBook.Id);  // Обновляем статус книги в базе данных
                    selectedBook.Status = "Доступна";  // Обновляем статус книги в DataGrid

                    // Обновляем отображение в DataGrid
                    BooksDataGrid.Items.Refresh();
                    MessageBox.Show("Книга успешно возвращена.");
                }
                else
                {
                    MessageBox.Show("Эта книга уже доступна.");
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для возврата.");
            }
        }

        // Метод для изменения статуса книги на "доступна" (возврат книги)
        private void ReturnBook(int bookId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE Books SET Available = 1 WHERE Id = @Id";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Id", bookId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод для добавления книги в базу данных
        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string genre = GenreTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(genre))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            AddBookToDatabase(title, author, genre);
            MessageBox.Show("Книга добавлена успешно.");

            // Обновление списка книг
            List<Book> books = SearchBooks(""); // Показать все книги
            BooksDataGrid.ItemsSource = books;
        }

        // Метод для добавления книги в базу данных
        private void AddBookToDatabase(string title, string author, string genre)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Books (Title, Author, Genre, Available) VALUES (@Title, @Author, @Genre, 1)";
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Author", author);
                    command.Parameters.AddWithValue("@Genre", genre);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Класс для хранения информации о книге
        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Genre { get; set; }
            public string Status { get; set; }
        }
    }
}