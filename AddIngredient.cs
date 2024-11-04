using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Windows.Forms;

namespace TEST
{
    public partial class AddIngredient : Form
    {
        private string connectionString = "provider=Microsoft.Jet.OLEDB.4.0;Data Source=breakfast.mdb"; //строка соединения с БД
        private DataGridView dataGridView2;

        public AddIngredient(DataGridView dgv)
        {
            InitializeComponent();
            dataGridView2 = dgv;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void AddIngredient_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e) //кнопка ОК
        {
            // Получение значений из ComboBox и TextBox
            string comboBoxValue = comboBox1.SelectedItem.ToString();
            string textBoxValue = textBox1.Text;

            // Выполнение запроса обновления
            UpdateDatabase(comboBoxValue, textBoxValue);

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    // SQL-запрос для извлечения данных из таблицы
                    string query = "SELECT * FROM fridge WHERE ingredient <> 'water'";

                    // Создаем команду для выполнения запроса
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Создаем адаптер для заполнения DataSet
                    OleDbDataAdapter adapter = new OleDbDataAdapter(command);

                    // Создаем DataSet для хранения данных
                    DataSet dataSet = new DataSet();

                    // Заполняем DataSet данными из базы данных
                    adapter.Fill(dataSet);

                    // Привязываем DataSet к DataGridView
                    dataGridView2.DataSource = dataSet.Tables[0];

                    dataGridView2.Columns[0].Width = 300;
                    dataGridView2.Columns[1].Width = 158;
                    dataGridView2.Columns[2].Width = 158;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private void UpdateDatabase(string comboBoxValue, string textBoxValue)
        {
            // Запрос обновления (пример)
            string query = "UPDATE [fridge] SET [count] = [@TextBoxValue] WHERE [ingredient] = [@ComboBoxValue]";

            // Использование конструкции using для автоматического закрытия подключения и команды
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Добавление параметров для предотвращения SQL-инъекций
                    command.Parameters.AddWithValue("@TextBoxValue", textBoxValue);
                    command.Parameters.AddWithValue("@ComboBoxValue", comboBoxValue);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        MessageBox.Show($"{rowsAffected} ингредиенты(-ов) были(-о) обновлены(-о)");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void AddIngredient_Load(object sender, EventArgs e)
        {

        }
    }
}
