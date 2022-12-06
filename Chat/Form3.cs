using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Chat
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            textBox2.UseSystemPasswordChar = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(800, 100);
        }
        public bool Login()
        {
            bool f = false;
            string login = textBox1.Text;
            foreach (string str in System.IO.File.ReadAllLines(@"login.txt"))
            {
                if (login == str)
                {
                    f = true;
                    break;
                }
            }
            return f;
        }

        public int PositionOfLoginInDatabase()//определение строки, в которой нах-ся логин
        {
            int k = 0, ret = 0;
            string login = textBox1.Text;
            foreach (string str in System.IO.File.ReadAllLines(@"login.txt"))
            {
                if (login == str)
                {
                    ret = k;
                    break;
                }
                k++;
            }
            return ret;
        }
        public string GetHash(string input)//создание хеш-функции по алгоритму md5
        {
            var md5 = MD5.Create();
            byte[] textData = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();

        }

        public bool CheckPassword(int pos_log, string password_from_textbox, string salt)//сравнение пароля из текстбокса и базы данных
        {
            bool f = false;
            string file_name = @"hash_password.txt";
            string password_from_database = "";
            int k = 0;
            foreach (string s in System.IO.File.ReadAllLines(file_name))
            {
                if (k == pos_log)
                {
                    password_from_database = s;
                    break;
                }
                k++;
            }
            password_from_database = password_from_database + salt;
            password_from_database = GetHash(password_from_database);
            if (password_from_database == password_from_textbox)
            {
                f = true;
            }
            return f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Enter login and password", "Хеширование", MessageBoxButtons.OK);
            }
            else
            {
                if (Login() == false) { MessageBox.Show("Wrong password or login", "Хеширование", MessageBoxButtons.OK); }
                else
                {
                    Random rnd = new Random();
                    string salt = rnd.Next().ToString();//соль для паролей
                    string password = textBox2.Text;
                    password = GetHash(password);
                    password = GetHash(password + salt);
                    int position_of_login = 0;
                    position_of_login = PositionOfLoginInDatabase();
                    if (CheckPassword(position_of_login, password, salt) == false)
                    {
                        MessageBox.Show("Wrong password or login", "Хеширование", MessageBoxButtons.OK);
                    }
                    else
                    {
                        Form4 Form4 = new Form4();
                        Form4.Show();
                        this.Hide();
                    }
                }
            }
        }
    }
}
