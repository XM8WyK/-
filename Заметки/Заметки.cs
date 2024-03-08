using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace Заметки
{
    /*
   ______          __         __   __             _  __ __  _______ _       __      __ __
  / ____/___  ____/ /__  ____/ /  / /_  __  __   | |/ //  |/  ( __ ) |     / /_  __/ //_/
 / /   / __ \/ __  / _ \/ __  /  / __ \/ / / /   |   // /|_/ / __  | | /| / / / / / ,<   
/ /___/ /_/ / /_/ /  __/ /_/ /  / /_/ / /_/ /   /   |/ /  / / /_/ /| |/ |/ / /_/ / /| |  
\____/\____/\__,_/\___/\__,_/  /_.___/\__, /   /_/|_/_/  /_/\____/ |__/|__/\__, /_/ |_|  
                                     /____/                               /____/         
     */
    public partial class Заметки : Form
    {
        private string connectionString = "Data Source=notes.db;Version=3;"; // Конектимся к базе данных SQLite
        public Заметки()
        {
            InitializeComponent();
            CreateTable(); // Создаем таблицу при запуске формы           
            UpdateNoteList(); // Обновляем список заметок при запуске формы
            TextBoxSearch.TextChanged += TextBoxSearch_TextChanged;
        }
        // При нажатие кнопки добавляет определеную заметку
        private void Add_Click(object sender, EventArgs e)
        {
            string title = TextBoxTitle.Text.Trim();
            string noteText = TextBoxContent.Text.Trim();
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(noteText))
            {
                CreateNote(title, noteText);
                UpdateNoteList();
                TextBoxTitle.Clear();
                TextBoxContent.Clear();
            }
            else
            {
                MessageBox.Show("Введите заголовок и текст заметки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Поиск заметок по ключевому слову из TextBox
        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = TextBoxSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                SearchNotes(keyword);
            }
            else
            {
                UpdateNoteList();
            }
        }
        // Создания таблицы в базе данных
        private void CreateTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "CREATE TABLE IF NOT EXISTS Notes (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Text TEXT)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        // Метод Поиска заметок по ключевому слову
        private void SearchNotes(string keyword)
        {
            listView1.Items.Clear();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Notes WHERE Title COLLATE UTF8_GENERAL_CI LIKE @Keyword OR Text COLLATE UTF8_GENERAL_CI LIKE @Keyword";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["Title"].ToString());
                            item.SubItems.Add(reader["Text"].ToString());
                            listView1.Items.Add(item);
                        }
                    }
                }
            }
        }
        // Обновления списка заметок на форме
        private void UpdateNoteList()
        {
            listView1.Items.Clear();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Notes";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["Title"].ToString());
                            item.SubItems.Add(reader["Text"].ToString());
                            listView1.Items.Add(item);
                        }
                    }
                }
            }
        }
        // Метод создания новой заметки в базе данных
        private void CreateNote(string title, string text)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO Notes (Title, Text) VALUES (@Title, @Text)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Text", text);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Заметка успешно создана.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        // Метод удаления заметки из базы данных
        private void DeleteNoteByTitle(string title)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM Notes WHERE Title = @Title";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("Заметка с указанным заголовком не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Заметка успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        // Удаляем при нажатие на кнопку заметку из базы данных
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string noteTitleToDelete = listView1.SelectedItems[0].Text;
                DeleteNoteByTitle(noteTitleToDelete);
                UpdateNoteList();
            }
            else
            {
                MessageBox.Show("Выберите заметку для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
