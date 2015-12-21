using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Programm_for_tests
{
    public partial class Form2 : Form
    {
        [SerializableAttribute]
        public struct Question
        {
            public string question;
            public string[] variant;
            public int correct;
        };

        Form1 f1;
        List<RadioButton> Radio = new List<RadioButton>();
        List<TextBox> BOX = new List<TextBox>();
        List<Question> Test = new List<Question>();
        int currQue = 0;
        int[] sequence;
        int[] answers;
        int correct = 0;

        public Form2(Form1 f, bool create)
        {
            InitializeComponent();
            f1 = f;
            Radio.Add(radioButton1);
            Radio.Add(radioButton2);
            Radio.Add(radioButton3);
            Radio.Add(radioButton4);
            BOX.Add(textBox1);
            BOX.Add(textBox2);
            BOX.Add(textBox3);
            BOX.Add(textBox4);
            string[] dirs = Directory.GetFiles((Application.StartupPath.ToString() + "\\Tests\\"), "*");
            
            foreach (string dir in dirs)
            {
                comboBox1.Items.Add(Path.GetFileName(dir));
            }
            if (!create)
            {
                for (int i = 0; i < 4; i++)
                {
                    BOX[i].Visible = false;
                }
                button1.Visible = false;
                button2.Visible = false;
                button3.Enabled = false;
                button5.Click -= button5_Click;
                button5.Click += button1_test;
                button5.Text = "Пройти тест";
                button4.Click -= button4_Click;
                button4.Click += button2_test;
                this.Text = "Прохождение теста";
                button4.Enabled = false;
                button6.Visible = false;
                button3.Click -= button3_Click;
                button3.Click += button3_test;
            }
        }

        //добавить вопрос
        private void button1_Click(object sender, EventArgs e)
        {
            bool flag = false;
            int corr = 0;

            if (richTextBox1.Text == "")
            {
                MessageBox.Show("Введите вопрос!!!", "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                if ((BOX[i].Text == ""))
                {
                    string str = "Введите " + (i+1) + " ответ";
                    MessageBox.Show("Введите " + (i + 1) + " ответ", "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Radio[i].Checked == true)
                {
                    flag = true;
                    corr = i;
                }
            }

            if (!flag)
            {
                MessageBox.Show("Ввеберите правильный вариант ответа!!!", "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Question vopr = new Question();
            vopr.question = richTextBox1.Text;
            vopr.variant = new string[4];
            for (int i = 0; i < 4; i++)
            {
                vopr.variant[i] = BOX[i].Text;
            }
            vopr.correct = corr;

            if (currQue == Test.Count)
            {
                Test.Add(vopr);
                currQue++;
                //totalQue++;
                richTextBox1.Clear();
                BOX[0].Clear();
                BOX[1].Clear();
                BOX[2].Clear();
                BOX[3].Clear();
                Radio[corr].Checked = false;
            }
            else
            {
                Test.RemoveAt(currQue);
                Test.Insert(currQue, vopr);
            }
        }

        //предыдущий вопрос
        private void button3_Click(object sender, EventArgs e) 
        {
            if (currQue > 0)
            {
                currQue--;
                richTextBox1.Text = Test[currQue].question;
                BOX[0].Text = Test[currQue].variant[0];
                BOX[1].Text = Test[currQue].variant[1];
                BOX[2].Text = Test[currQue].variant[2];
                BOX[3].Text = Test[currQue].variant[3];
                Radio[Test[currQue].correct].Checked = true;
                button1.Text = "Изменить вопрос";
            }
        }

        //следующий вопрос при создании теста
        private void button4_Click(object sender, EventArgs e) 
        {
            if (currQue < Test.Count)
            {
                currQue++;
                if (currQue < Test.Count)
                {
                    richTextBox1.Text = Test[currQue].question;
                    BOX[0].Text = Test[currQue].variant[0];
                    BOX[1].Text = Test[currQue].variant[1];
                    BOX[2].Text = Test[currQue].variant[2];
                    BOX[3].Text = Test[currQue].variant[3];
                    Radio[Test[currQue].correct].Checked = true;
                }
                else 
                {   
                    richTextBox1.Clear();
                    BOX[0].Clear();
                    BOX[1].Clear();
                    BOX[2].Clear();
                    BOX[3].Clear();
                    Radio[Test[currQue - 1].correct].Checked = false;
                    button1.Text = "Добавить вопрос";
                }
            }
        }

        //удалить вопрос
        private void button2_Click(object sender, EventArgs e)
        {
            if (currQue < Test.Count)
            {
                Radio[Test[currQue].correct].Checked = false;
                Test.RemoveAt(currQue);
                //totalQue--;
                currQue = Test.Count;
                richTextBox1.Clear();
                BOX[0].Clear();
                BOX[1].Clear();
                BOX[2].Clear();
                BOX[3].Clear();
                button1.Text = "Добавить вопрос";
            }
        }

        //редактирование теста
        private void button5_Click(object sender, EventArgs e)
        {
            //Вытаскиваем файлы уже существующих тестов и проверяем на существование
            string filename = Application.StartupPath.ToString() + "\\Tests\\" + comboBox1.Text;
            bool exist = false;
            string[] dirs = Directory.GetFiles((Application.StartupPath.ToString() + "\\Tests\\"), "*");
            foreach (string dir in dirs)
            {
                if (dir == filename) exist = true;
            }
            if (exist)
            {
                var result = MessageBox.Show("Загрузить тест для редактирования?", "Запрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    FileStream fin = File.OpenRead(filename);
                    BinaryFormatter buf = new BinaryFormatter();
                    Question quest = new Question();
                    quest.variant = new string[4];

                    while (fin.Length != fin.Position)
                    {
                        quest = (Question)buf.Deserialize(fin);
                        Test.Add(quest);
                    }
                    fin.Close();
                    currQue = 0;
                    richTextBox1.Text = Test[currQue].question;
                    BOX[0].Text = Test[currQue].variant[0];
                    BOX[1].Text = Test[currQue].variant[1];
                    BOX[2].Text = Test[currQue].variant[2];
                    BOX[3].Text = Test[currQue].variant[3];
                    Radio[Test[currQue].correct].Checked = true;
                }
                else
                { return; }
            }
            else
            {
                MessageBox.Show("Теста с заданным название не существует!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

        }

        //запись в файл теста
        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("Введите название теста!", "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Test.Count == 0)
            {
                MessageBox.Show("Не задан ни один вопрос!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //Вытаскиваем файлы уже существующих тестов и проверяем на существование
            string filename = Application.StartupPath.ToString() + "\\Tests\\" + comboBox1.Text;
            bool exist = false;
            string[] dirs = Directory.GetFiles((Application.StartupPath.ToString() + "\\Tests\\"), "*");
            foreach (string dir in dirs)
            {
                if (dir == filename) exist = true;
            }
            if (exist)
            {
                var result = MessageBox.Show("Файл с указанным файлом существует.\nПерезаписать его?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    File.Delete(filename);
                }
                else
                { return; }

            }
            //запись вопросов в файл
            FileStream fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);//File.OpenWrite(filename);
            BinaryFormatter buf = new BinaryFormatter();
            for (int i = 0; i < Test.Count; i++)
            {
                buf.Serialize(fout, Test[i]);
            }
            fout.Close();
            Test.Clear();
            currQue = 0;
            richTextBox1.Clear();
            comboBox1.Items.Add(comboBox1.Text);
            comboBox1.Text = "";
            for (int i = 0; i < 4; i++)
            {
                BOX[i].Clear();
                Radio[i].Checked = false;
            }
            button1.Text = "Добавить вопрос";
        }

        //кнопка для начала прохождения теста
        private void button1_test(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("Ввеберите тест!", "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filename = Application.StartupPath.ToString() + "\\Tests\\" + comboBox1.Text;
            FileStream fin = File.OpenRead(filename);

            BinaryFormatter buf = new BinaryFormatter();
            Question quest = new Question();
            quest.variant = new string[4];

            while (fin.Length != fin.Position)
            {
                quest = (Question)buf.Deserialize(fin);
                Test.Add(quest);
            }
            fin.Close();

            //для массива рандомных чисел
            int pos;
            sequence = new int[Test.Count];
            answers = new int[Test.Count];
            int[] b = new int[Test.Count];
            Random rand = new Random();
            for (int i = 0; i < Test.Count; i++)
                b[i] = i;
            for (int i = 0; i < Test.Count; i++)
            {
                pos = rand.Next() % (Test.Count - i);
                sequence[i] = b[pos];
                b[pos] = b[Test.Count - i - 1];
            }

            //первый вопрос
            richTextBox1.Text = Test[sequence[currQue]].question;
            for (int i = 0; i < 4; i++)
            {
                Radio[i].Text = Test[sequence[currQue]].variant[i];
            }
            button4.Enabled = true;
            button5.Enabled = false;
        }

        //следующий вопрос при прохождении теста
        private void button2_test(object sender, EventArgs e)
        {
            bool check = false;
            for (int i = 0; i < 4; i++)
            {
                if (Radio[i].Checked == true) 
                { 
                    check = true;
                    answers[currQue] = i;
                    break;
                }
            }
            if (!check)
            {
                MessageBox.Show("Ввеберите вариант ответа!", "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currQue++;
            if (currQue != 0)
            { button3.Enabled = true; }

            if (currQue != Test.Count)
            {
                richTextBox1.Text = Test[sequence[currQue]].question;
                for (int i = 0; i < 4; i++)
                {
                    Radio[i].Text = Test[sequence[currQue]].variant[i];
                    Radio[i].Checked = false;
                }
                if ((currQue+1) == Test.Count) button4.Text = "Завершить тест";
            }
            else
            {
                var result = MessageBox.Show("Хотите завершить тест?", "Запрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) { currQue--; return; }

                for (int i = 0; i < Test.Count; i++)
                {
                    if (answers[i] == Test[sequence[i]].correct) { correct++; }
                }

                    MessageBox.Show("Вы выполнили тест!\nИтог: " + correct.ToString() + "/" + Test.Count.ToString(), "Запрос", MessageBoxButtons.OK, MessageBoxIcon.Information);
                correct = currQue = 0;
                Test.Clear();
                richTextBox1.Text = "";
                for (int i = 0; i < 4; i++)
                {
                    Radio[i].Text = "";
                    Radio[i].Checked = false;
                    check = false;
                }
                button4.Enabled = false;
                button4.Text = "Следующий вопрос ->";
                button5.Enabled = true;
            }

        }

        //предыдущий вопрос
        private void button3_test(object sender, EventArgs e)
        {
            currQue--;
            button4.Text = "Следующий вопрос ->";
            richTextBox1.Text = Test[sequence[currQue]].question;
            for (int i = 0; i < 4; i++)
            {
                Radio[i].Text = Test[sequence[currQue]].variant[i];
                Radio[i].Checked = false;
            }
            if (currQue == 0)
            {
                button3.Enabled = false;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы хотите выйти?", "Запрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                f1.Show();
            }
            else
            { e.Cancel = true; }
        }
    }
}
