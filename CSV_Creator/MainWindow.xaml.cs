﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data;
using System.Threading;
using System.Windows.Threading;

namespace CSV_Creator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    public partial class MainWindow : System.Windows.Window
    {

        bool _timer = false;
        private bool _isAnalyzing = false;
        private Thread _thread;
        string _path;


        public MainWindow()
        {
            InitializeComponent();            
            this.Loaded += MainWindow_Loaded;            
        }

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_timer)
            {
                txtLoading.Visibility = Visibility.Visible;
            }
            else
            {
                txtLoading.Visibility = Visibility.Hidden;
            }
            _timer = !_timer;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAnalyzing)
            {
                OpenFileDialog openfile = new OpenFileDialog();
                openfile.DefaultExt = ".xls";
                openfile.Filter = "Excel Files| *.xlsx; *.xls";

                var browsefile = openfile.ShowDialog();
                
                if (browsefile == true)
                {
                    _path = openfile.FileName;
                    string[] arr = _path.Split('\\');
                    txtFilePath.Text = arr[(arr.Length - 1)];

                    Dispatcher.BeginInvoke(new ThreadStart(delegate
                    {
                        //stopAnalyze();
                        
                        dispatcherTimer.Start();
                        btnOpen.Content = "Stop loading";
                    }));

                    
                    

                   // _thread = new Thread(LoadFile);
                   // _thread.Start();


                    Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                   


                    //????????? Why so many parameters???
                    //Microsoft.Office.Interop.Excel.Workbook excelBook = excelApp.Workbooks.Open(txtFilePath.Text.ToString(), 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                    Microsoft.Office.Interop.Excel.Workbook excelBook = excelApp.Workbooks.Open(_path);
                    Microsoft.Office.Interop.Excel.Worksheet excelSheet = (Microsoft.Office.Interop.Excel.Worksheet)excelBook.Worksheets.get_Item(1); ;
                    Microsoft.Office.Interop.Excel.Range excelRange = excelSheet.UsedRange;

                    string strCellData = "";
                    double douCellData;
                    int rowCnt = 0;
                    int colCnt = 0;

                    int countToName = 0; // will use to create new name of collun if sach name is exist already

                    DataTable dt = new DataTable();

                    for (colCnt = 1; colCnt <= excelRange.Columns.Count; colCnt++)
                    {

                        string strColumn = "";
                        strColumn = (string)(excelRange.Cells[1, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                        try
                        {
                            dt.Columns.Add(strColumn, typeof(string));
                        }
                        catch (DuplicateNameException)
                        {
                            strColumn = strColumn + "_" + (++countToName).ToString();
                            dt.Columns.Add(strColumn, typeof(string));
                        }

                    }

                    /////
                    int countOfRows = 3;
                    /////
                    if (excelRange.Rows.Count < countOfRows) countOfRows = excelRange.Rows.Count;

                    for (rowCnt = 2; rowCnt <= countOfRows; rowCnt++)
                    {

                        string strData = "";
                        for (colCnt = 1; colCnt <= excelRange.Columns.Count; colCnt++)
                        {
                            try
                            {
                                strCellData = (string)(excelRange.Cells[rowCnt, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                                strData += strCellData + "|";
                            }
                            catch (Exception ex)
                            {
                                douCellData = (excelRange.Cells[rowCnt, colCnt] as Microsoft.Office.Interop.Excel.Range).Value2;
                                strData += douCellData.ToString() + "|";
                            }
                        }
                        strData = strData.Remove(strData.Length - 1, 1);
                        dt.Rows.Add(strData.Split('|'));
                    }

                    dtGrid.ItemsSource = dt.DefaultView;

                    try
                    {
                        excelBook.Close(true, null, null);
                        excelApp.Quit();
                    }
                    catch (Exception)
                    {
                        throw;
                    }


                }                
            }
            else
            {
                
            }
        }

        private void stopAnalyze()
        {
            _isAnalyzing = false;
            dispatcherTimer.Stop();
            btnOpen.Content = "Select excel file";
                        
        }



        void LoadFile()
        {
            _isAnalyzing = true;
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        
    }
}
