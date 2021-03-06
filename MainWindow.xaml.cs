using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace WpfMSSQLtoSQLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_ClickDatabaseInfo(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbxConnectionStringMSSQL.Text))
            {
                MessageBox.Show("Введите имя базы данных MS SQL");
                return;
            }

            if (string.IsNullOrWhiteSpace(TbxConnectionStringSQLite.Text))
            {
                MessageBox.Show("Введите имя базы данных SQLite");
                return;
            }

            ImportDBInfo(TbxConnectionStringMSSQL.Text, TbxConnectionStringSQLite.Text);
        }

        private void Btn_ClickCreateTableScript(object sender, RoutedEventArgs e)
        {
            CreateTableScript();
        }

        private void Btn_ClickCreateTable(object sender, RoutedEventArgs e)
        {
            CreateTable();
        }

        private void Btn_ClickCopyTable(object sender, RoutedEventArgs e)
        {
            if ((bool)ChbxIsLoremIpsum.IsChecked)
            {
                ChangeData cd = new ChangeData();
                
                CopyData(cd.GetNewData);
            }
            else
            {
                CopyData((cn, d) => d);
            }
        }

        private void TbxDbName_LostFocus(object sender, RoutedEventArgs e)
        {
            EnableBtnLoadAndCreate();
        }

        private void EnableBtnLoadAndCreate()
        {
            if (string.IsNullOrWhiteSpace(TbxConnectionStringMSSQL.Text) == false && string.IsNullOrWhiteSpace(TbxConnectionStringSQLite.Text) == false)
            {
                BtnLoadDb.IsEnabled = true;
            }
            else
            {
                BtnLoadDb.IsEnabled = false;
            }
        }

        private void BtnExitProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TbxConnectionStringMSSQL.LostFocus += new(TbxDbName_LostFocus);
            TbxConnectionStringSQLite.LostFocus += new(TbxDbName_LostFocus);

            EnableBtnLoadAndCreate();
        }

        private void TbxConnectionStringSQLite_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "sqlite|*.sqlite;*.db;*.sqlite3;*.db3|All files *.*|*.*",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string fname = saveFileDialog.FileName.Trim();

                if (fname.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase) == false
                    && fname.EndsWith(".db", StringComparison.OrdinalIgnoreCase) == false
                    && fname.EndsWith(".sqlite3", StringComparison.OrdinalIgnoreCase) == false
                    && fname.EndsWith(".db3", StringComparison.OrdinalIgnoreCase) == false)
                {
                    fname += ".sqlite";
                }

                TbxConnectionStringSQLite.Text = fname;

                BtnLoadDb.IsEnabled = true;
            }
        }
    }
}
