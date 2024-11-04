using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace TEST
{
    public partial class Form1 : Form
    {
        private string connectionString = "provider=Microsoft.Jet.OLEDB.4.0;Data Source=breakfast.mdb"; //строка соединения с БД
        private List<string> outputTrace = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }
            
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            ShowRecipes();
        }
        private void LoadData()
        {
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
        private void ShowRecipes()
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();                  

                    // Заполнение трассы вывода
                    outputTrace.Clear();
                    outputTrace.Add($"Правило 1: Количество каждого ингредиента (сахар, яйца и т.д.) больше 0.");
                    outputTrace.Add($"Правило 2: Количество ингредиентов для каждого рецепта больше или равно необходимому.");
                    // SQL-запрос для извлечения данных из таблицы
                    string query_2 = "SELECT * FROM recipes r WHERE (r.rice IS NULL OR r.rice <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'rice')) AND (r.milk IS NULL OR r.milk <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'milk')) AND (r.sugar IS NULL OR r.sugar <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'sugar')) AND (r.apple IS NULL OR r.apple <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'apple')) AND (r.egg IS NULL OR r.egg <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'egg')) AND (r.oil IS NULL OR r.oil <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'oil')) AND (r.bread IS NULL OR r.bread <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'bread')) AND (r.sausage IS NULL OR r.sausage <= (SELECT f.count FROM fridge f WHERE f.ingredient <> 'water' AND f.ingredient = 'sausage'))";
                    // Создаем команду для выполнения запроса
                    OleDbCommand command_2 = new OleDbCommand(query_2, connection);

                    // Создаем адаптер для заполнения DataSet
                    OleDbDataAdapter adapter_2 = new OleDbDataAdapter(command_2);

                    // Создаем DataSet для хранения данных
                    DataSet dataSet_2 = new DataSet();

                    // Заполняем DataSet данными из базы данных
                    adapter_2.Fill(dataSet_2);

                    // Привязываем DataSet к DataGridView
                    dataGridView1.DataSource = dataSet_2.Tables[0];
                    label3.Text = $"Количество рецептов: {dataGridView1.Rows.Count - 1}";

                    // Добавляем рецепты в трассу вывода
                    foreach (DataRow row in dataSet_2.Tables[0].Rows)
                    {
                        string recipeName = row["recipe"].ToString();
                        StringBuilder ingredients = new StringBuilder();

                        foreach (DataColumn column in dataSet_2.Tables[0].Columns)
                        {
                            if (column.ColumnName != "recipe" && row[column] != DBNull.Value)
                            {
                                ingredients.Append($"{column.ColumnName}: {row[column]}, ");
                            }
                        }

                        string recipeDetails = $"Рецепт, который можно приготовить: {recipeName} ({ingredients.ToString().TrimEnd(',', ' ')})";
                        outputTrace.Add(recipeDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                label3.Text = $"Количество рецептов: 0";
                outputTrace.Add($"Ошибка: {ex.Message}");
            }
            listBoxTrace.Items.Clear();
            foreach (string trace in outputTrace)
            {
                listBoxTrace.Items.Add(trace);
            }
        }
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            about about = new about();
            about.ShowDialog();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Как! Уже уходите? Так быстро?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        }
        private void button1_Click(object sender, EventArgs e) // кнопка приготовления рецепта
        {
            // 1. Получить выбранный рецепт
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите рецепт для приготовления");
                return;
            }

            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            string selectedRecipeName = selectedRow.Cells["recipe"].Value.ToString();

            // 2. Получить необходимые ингредиенты
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM recipes WHERE recipe = @recipeName";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@recipeName", selectedRecipeName);

                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                if (dataSet.Tables.Count == 0)
                {
                    MessageBox.Show($"Рецепт '{selectedRecipeName}' не найден.");
                    return;
                }
                DataRow recipeRow = dataSet.Tables[0].Rows[0];

                // 3. Обновить таблицу `fridge`
                foreach (DataColumn column in dataSet.Tables[0].Columns)
                {
                    if (column.ColumnName != "recipe" && recipeRow[column] != DBNull.Value)
                    {
                        string ingredientName = column.ColumnName;
                        int requiredQuantity = (int)recipeRow[column];
                        // Получить текущее количество ингредиента
                        string fridgeQuery = "SELECT count FROM fridge WHERE ingredient = @ingredient";
                        OleDbCommand fridgeCommand = new OleDbCommand(fridgeQuery, connection);
                        fridgeCommand.Parameters.AddWithValue("@ingredient", ingredientName);

                        int currentQuantity;
                        object result = fridgeCommand.ExecuteScalar();
                        if (result != null)
                        {
                            currentQuantity = (int)result;
                        }
                        else
                        {
                            MessageBox.Show($"Не удалось найти '{ingredientName}' в таблице 'fridge'");
                            return;
                        }

                        if (currentQuantity >= requiredQuantity)
                        {
                            // Вычесть необходимое количество
                            string updateQuery = "UPDATE [fridge] SET [count] = [count] - [@requiredQuantity] WHERE [ingredient] = [@ingredientName]";
                            OleDbCommand updateCommand = new OleDbCommand(updateQuery, connection);
                            updateCommand.Parameters.AddWithValue("@requiredQuantity", requiredQuantity);
                            updateCommand.Parameters.AddWithValue("@ingredientName", ingredientName);
                            updateCommand.ExecuteNonQuery();
                        }
                        else
                        {
                            MessageBox.Show($"Недостаточно '{ingredientName}' для приготовления рецепта '{selectedRecipeName}'.");
                            return;
                        }
                    }
                }
                MessageBox.Show($"Рецепт '{selectedRecipeName}' успешно приготовлен!");
                LoadData(); 
                ShowRecipes();
            }
        }
        private void button2_Click(object sender, EventArgs e) //кнопка добавления ингредиентов
        {
            AddIngredient add = new AddIngredient(dataGridView2);
            add.Show();

            LoadData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowRecipes();
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pomogaika Pomogaika = new Pomogaika();
            Pomogaika.Show();
        }
    }
}