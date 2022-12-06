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
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Chat
{
    public partial class Form2 : Form
    {
        
        
        public Form2()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 100);
            RSAHash.Data_From_User2ToUser1.EventHandler = new RSAHash.Data_From_User2ToUser1.MyEvent(RSATxtChange);
            TripleDESHash.Data_From_User2ToUser1.EventHandler = new TripleDESHash.Data_From_User2ToUser1.MyEvent(TripleDESTxtChange);
        }


        private void RSATxtChange(byte[][] data, RSAParameters key)
        {
            RSAHashDecrypt(data, key);
        }
        private void TripleDESTxtChange(byte[] data, byte[] key, byte[] vector)
        {
            TripleDESHashDecrypt(data, key, vector);
        }
        public void TripleDESHashEncrypt(string txt)
        {
            using (TripleDES TDAlg = TripleDES.Create())
            {
                byte[] output;
                byte[] key = TDAlg.Key;
                byte[] vector = TDAlg.IV; //вектор инициализации
                ICryptoTransform encryptor = TDAlg.CreateEncryptor(key, vector);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {   
                            swEncrypt.Write(txt);
                        }
                        output = msEncrypt.ToArray();
                    }
                }
                TripleDESHash.Data_From_User1ToUser2.EventHandler(output, key, vector);
            }
        }
        public void TripleDESHashDecrypt(byte[] data, byte[] key, byte[] vector)
        {
            using (TripleDES TDAlg = TripleDES.Create())
            {
                TDAlg.Key = key;
                TDAlg.IV = vector;

                ICryptoTransform decryptor = TDAlg.CreateDecryptor(TDAlg.Key, TDAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(data))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            richTextBox3.Text = richTextBox3.Text + BitConverter.ToString(data).Replace("-", String.Empty).ToLower()+Environment.NewLine;
                            richTextBox2.Text = srDecrypt.ReadToEnd() + richTextBox2.Text;
                        }
                    }
                }
            }
        }
        public void RSAHashEncrypt(string txt)
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSAParameters public_key = RSA.ExportParameters(false);
            RSAParameters private_key = RSA.ExportParameters(true);
            RSA.ImportParameters(public_key);
            byte[] textData = ByteConverter.GetBytes(txt);
            int len = textData.Length/100+1;
            byte[][] text_array = new byte[len][];
            int m = 0;
            if (len == 1)//делю массив массив текста по 128 битов
            {
                text_array[0] = new byte[textData.Length];
            }
            else
            {
                int k = 0;
                for (int i = 0; i < textData.Length; i++)
                {
                    m++;
                    if (m == 100)
                    {
                        text_array[k] = new byte[m];
                        k++;
                        m = 0;
                    }
                }
                if (m != 0)
                {
                    text_array[k] = new byte[m];
                }
            }
            int index = 0;
            m = 0;
            for (int i = 0; i < textData.Length; i++)
            {
                if (m == 100)
                {
                    index++;
                    m = 0;
                }
                Array.Copy(textData, i, text_array[index], m, 1);
                m++;
            }
            byte[][] output = new byte[len][];
            for (int i=0; i<len; i++)
            {
                output[i] = new byte[128];
            }
            output[0] = RSA.Encrypt(text_array[0], false);
            for (int i = 1; i < len; i++)//складываю
            {
                for(int j=0; j<text_array[i].Length; j++)
                {
                    int o = output[i-1][j];
                    int t = text_array[i][j];
                    int p = o^t;
                    text_array[i][j] = Convert.ToByte(p);
                }
                RSA.ImportParameters(public_key);
                byte[] data = RSA.Encrypt(text_array[i], false);

                output[i] = data;
            }
            RSAHash.Data_From_User1ToUser2.EventHandler(output, private_key);


        }
        public void RSAHashDecrypt(byte[][] data, RSAParameters key)
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            int len = data.Length;
            byte[][] text_array = new byte[len][];
            int k = 0;
            byte[][] output = new byte[len][];
            RSA.ImportParameters(key);
            text_array[0] = RSA.Decrypt(data[0], false);
            string txt = "";
            for (int i = 1; i < len; i++)//складываю
            {
                text_array[i] = new byte[data[i].Length];
                RSA.ImportParameters(key);
                richTextBox3.Text = richTextBox3.Text+BitConverter.ToString(data[i]);
                text_array[i] = RSA.Decrypt(data[i], false);
                
                for (int j = 0; j < text_array[i].Length; j++)
                {
                    int o = data[i - 1][j];
                    int t = text_array[i][j];
                    int p = o ^ t;
                    text_array[i][j] = Convert.ToByte(p);
                }

            }
            for (int i = 0; i < len; i++)
            {
                txt = txt + ByteConverter.GetString(text_array[i]);
            }
            richTextBox2.Text = txt + richTextBox2.Text;
        }
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form Form1 = Application.OpenForms[0];
            Form1.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string msg= DateTime.Now.ToLongTimeString() + ": " + richTextBox1.Text + "\r\n";
            if (radioButton1.Checked == true)
            {
                RSAHashEncrypt(msg);
            }
            else if (radioButton2.Checked == true){
                TripleDESHashEncrypt(msg);
            }
            else
            {
                MessageBox.Show("Выберите алгоритм шифрования", "Chat", MessageBoxButtons.OK);
            }
            richTextBox1.Text = null;
        }



        public void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
