﻿using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        public int value = 0;
        public static int varNb = 0;
        public int offset = 0;
        public int limit = 2;
        public static string path = @"c:\Images\";
        DateTime time = new DateTime();
        Hebus item;
        WebClient ei = new WebClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                item = new Hebus();
                pagination();
                StreamReader read = new StreamReader(path + "log");
                textBoxDebut.Text = read.ReadLine();
                textBoxLimit.Text = read.ReadLine();
                read.Close();
            }
            catch (Exception)
            {
                textBoxDebut.Text = "0";
                textBoxLimit.Text = "10";
                textBlockInfos.Content = "Initialization";
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            readResult();
        }

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
                DateTime time = new DateTime();
                time = DateTime.Now;
                item.process();
                if (cbRewriteFile.IsChecked == false)
                {
                    MessageBoxResult result = MessageBox.Show("Souhaitez vous supprimer le dossier " + path + " ?", "Attention", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                        Directory.Delete(path, true);
                }
                for (value = int.Parse(textBoxDebut.Text); value <= int.Parse(textBoxLimit.Text); value++)
                {
                    operate();
                    progressBar1.Value = (100 / (int.Parse(textBoxLimit.Text)) * value);
                    //if (value%100==0) MessageBox.Show("Mise a jour", "Informations", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None);
                }

                textBlockVarNB.Text = (Convert.ToString(varNb));
                DateTime elapse = DateTime.Now;
                textBlockDuree.Text = elapse.Subtract(time).Hours.ToString() + "H -" + elapse.Subtract(time).Minutes.ToString() + "M -" + elapse.Subtract(time).Seconds.ToString() + 'S';
                try
                {
                    StreamWriter logger = new StreamWriter(path + "log", false);
                    logger.WriteLine(textBoxLimit.Text + "\n" + (int.Parse(textBoxLimit.Text) + 10));
                    logger.Close();
                }
                catch (Exception i)
                {
                    MainWindow.log(i);
                }
                textBlockInfos.Content = "La liste d'image est copiée dans: " + path;
                textBlockState.Text = "Etat: Terminé";
                textBoxDebut.Text = Convert.ToString(int.Parse(textBoxLimit.Text) + 1);
                textBoxLimit.Text = Convert.ToString(int.Parse(textBoxLimit.Text) + 11);
                readResult();
                pagination();
                ei.Dispose();
        }

        public void operate()
        {
            {
                String URL = item.getImageURL(value);
                writeImageURL(URL);
                getImage(URL);
                Console.WriteLine(progressBar1.Value);
            }
        }
        public bool getImage(String url)
        {
            if (url != "")
            {
                ei.DownloadFile(url,path+value+(url.Substring(url.LastIndexOf('.'))));
                return true;
            }
            else return false;
        }
        
        private void btPrec_Click(object sender, RoutedEventArgs e)
        {
            offset = (offset == 0) ? 0 : offset - limit;
            readResult();
            pagination();

        }

        private void btSuiv_Click(object sender, RoutedEventArgs e)
        {
            offset += limit;
            readResult();
            pagination();
        }

        private void btVoir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.Parse(tbPage.Text) <= (Hebus.countMyLink() / limit) && int.Parse(tbPage.Text) >= 0)
                {
                    offset = limit * int.Parse(tbPage.Text);
                    readResult();
                }
                else tbPage.Text = "?";
            }
            catch (Exception i)
            {
                tbPage.Text = "?";
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.Parse(tbImgPage.Text) > 0)
                {
                    limit = int.Parse(tbImgPage.Text);
                    pagination();
                    readResult();
                }
                else tbImgPage.Text = "?";
            }
            catch (Exception i)
            {
                tbImgPage.Text = "?";
            }
        }

        private void cbRewriteFile_Checked(object sender, RoutedEventArgs e)
        {
            cbRewriteFile.Content = "Ajouter";
        }

        private void cbRewriteFile_Unchecked(object sender, RoutedEventArgs e)
        {
            cbRewriteFile.Content = "Ré-ecrire";
        }

        protected String getSource(int limit, String source)
        {
            try
            {
                if (limit != 0)
                    source += limit + ".html";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(source);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                String Line = reader.ReadToEnd();
                response.Close();
                return Line;
            }
            catch (Exception e)
            {
                log(e);
                String Line = "";
                return Line;
            }
        }

        public void writeImageURL(string URL)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                StreamWriter stream = new StreamWriter(path + "List.txt", true);
                if (URL != "")
                    stream.WriteLine(value+" : "+URL);
                stream.Close();
                if (cbRewriteFile.IsChecked == false) cbRewriteFile.IsChecked = true;
            }
            catch (Exception e)
            {
                log(e);
            }
        }

        private void Window_gotFocus(object sender, RoutedEventArgs e)
        {
                textBlockState.Text = "Traitement...";
        }

        public static void log(Exception e)
        {
            if (!Directory.Exists(path))
            System.IO.Directory.CreateDirectory(path);
            StreamWriter stream = new StreamWriter(path + "errors", true);
            stream.WriteLine(">" + DateTime.Now + " : " + e.ToString());
            stream.Close();
        }

        public void readResult()
        {
            try
            {
                StreamReader str = new StreamReader(path + "List.txt");
                string line = "<html><body bgColor='black'><center>";
                int initCount = 0;
                double width=0;
                while (initCount <= offset)
                {
                    if (initCount != 0) str.ReadLine();
                    initCount++;
                }
                for (int count = 0; count < limit; count++)
                {
                    if (limit<=2) width = webBrowser1.Width / limit-1; else width = webBrowser1.Width / (Math.Sqrt(limit));
                    String itemURL = str.ReadLine();
                    line += "<a href=\"" + itemURL.Substring(itemURL.IndexOf(':')+1) + "\"><img style=\"border:0px\" "
                    +"width=\"" + width + "\""
                    +"src=\""+ path + itemURL.Substring(0, itemURL.IndexOf(':')-1) + itemURL.Substring(itemURL.LastIndexOf('.')) + "\"/></a>&nbsp;";
                }
                line = line + "</center><body></html>";
                webBrowser1.NavigateToString(line);
                str.Close();
            }
            catch (Exception e)
            {
                log(e);
            }
        }

        public void pagination()
        {
            if (File.Exists(path + "list.txt"))
            {
                if (offset> 0) btPrec.IsEnabled = true;
                button1.IsEnabled = true;
                StreamReader readMore = new StreamReader(path + "list.txt");
                for (int count = 0; count < limit + 1; count++)
                    if (readMore.ReadLine() != null && count >= limit)
                    {
                        btImgPage.IsEnabled = true;
                        btSuiv.IsEnabled = true;
                        btVoir.IsEnabled = true;
                        tbPage.IsEnabled = true;
                        if (offset == 0) btPrec.IsEnabled = false;
                        if ((offset / limit) + 1==((Hebus.countMyLink() / limit) + 1)) btSuiv.IsEnabled = false;
                    }
                int page = (offset / limit) + 1;
                tbPage.Text = Convert.ToString(page);
                lPage.Content = "/ " + Convert.ToString((Hebus.countMyLink()/limit)+1);
                readMore.Close();
            }
            else webBrowser1.NavigateToString("<center>Vous n'avez pas encore recupere d'imagess</center>");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            item.Close();
            this.Close();
        }

        private void progressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Console.WriteLine("ok");
        }
    }
}