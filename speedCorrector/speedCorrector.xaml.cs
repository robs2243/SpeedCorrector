using System;
using System.Collections.Generic;
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
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Media;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;



namespace speedCorrector
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SQLiteConnection dbConnection;
        private int counter = 0;
        private int counter2 = 0;
        IEnumerable<String> pfade_fragen;
        IEnumerable<String> pfade_antworten;

        public MainWindow()
        {
            InitializeComponent();

        }

        private BitmapImage holeLösungsBild()
        {

            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("bild2.bmp", UriKind.Absolute);
            bi3.EndInit();

            return bi3;
        }

        private BitmapImage holeAufgabenBild()
        {

            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(Directory.GetCurrentDirectory() + "/bild1.bmp", UriKind.Absolute);
            bi3.EndInit();

            return bi3;
        }
                
  
        private static byte[] converterDemo(BitmapImage x)
        {
            byte[] data;
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(x));
            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);
            data = ms.ToArray();
            return data;
        }
       
        private String returnhHashValue(String Bild)
        {
            byte[] hashValue;

            SHA256 mySHA256 = SHA256Managed.Create();

            FileStream fRead = new FileStream(Bild, FileMode.Open, FileAccess.Read);

            // Be sure it's positioned to the beginning of the stream.
            fRead.Position = 0;

            // Compute the hash of the fileStream.
            hashValue = mySHA256.ComputeHash(fRead);

            // Close the stream.
            fRead.Close();
                        
            String hashValueString = PrintByteArray(hashValue);

            return hashValueString;

        }

        private String PrintByteArray(byte[] array)
        {
            String ausgabe = "";
            int i;
            for (i = 0; i < array.Length; i++)
            {
                ausgabe = ausgabe + String.Format("{0:X2}", array[i]);
                //Console.Write(String.Format("{0:X2}", array[i]));
                //if ((i % 4) == 3) Console.Write(" ");
            }
            //Console.WriteLine();
            return ausgabe;
        }

        private void B_letztes_Click(object sender, RoutedEventArgs e)
        { 
            
        }


        string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                folder += System.IO.Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }

        private void LstAnzeige_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        void schreibehashWerteInDBTab(String DBTab, String hash)
        {
            dbConnection = new SQLiteConnection("Data Source = db.sqlite; Version = 3");
            //dbConnection.SetPassword("geheim");
            try
            {
                dbConnection.Open();
            }
            catch (SQLiteException myException)
            {
                MessageBox.Show("Message: " + myException.Message + "\n");
                TextBox_System_Nachricht.Text = "Fehler beim Öffnen der DB.";
                
            }

            TextBox_System_Nachricht.Text = "DB erfolgreich geöffnet.\n";

            try
            {
                string sql = "REPLACE INTO " + DBTab + "(hash) VALUES(@hash)";
                SQLiteCommand Command = new SQLiteCommand(sql, dbConnection);
                Command.Parameters.Add("@hash", DbType.String).Value = hash;
                Command.ExecuteNonQuery();
                Command.Parameters.Clear();
                dbConnection.Close();
                TextBox_System_Nachricht.Text = TextBox_System_Nachricht.Text + "In DB erfolgreich geschrieben.\n";
            }

            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Schreiben in die DB\n" + ex.Message);
                TextBox_System_Nachricht.Text = "Fehler beim Schreiben in die DB";
            }

        }
    
                       
        private BitmapImage sendeBildAnImage(String Bild)
        {
            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(Bild, UriKind.Absolute);
            bi3.EndInit();

            return bi3;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            counter = 0;
            counter2 = 0;

            PictureBox_Fragen.Stretch = Stretch.Fill;
            try
            {
                PictureBox_Fragen.Source = sendeBildAnImage(Directory.GetCurrentDirectory() + "/fragen/bild1.bmp");
                InkCanvas_Antwort.Background = new ImageBrush(sendeBildAnImage(Directory.GetCurrentDirectory() + "/antworten/bild2.bmp"));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Startbilder.\n" + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
                         
        }

        private void bilderBlättern(String richtung, bool InkCanvas)
        {

            //var pfade = Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/" + relativerOrdner);

            if (!InkCanvas)
            {
                if (richtung == "vor")
                {
                    if (counter == pfade_fragen.Count<String>())
                    {

                        counter = pfade_fragen.Count<String>();

                    }
                    else
                    {

                        counter = counter + 1;

                    }

                }
                else if (richtung == "zurück")
                {
                    if (counter == 0)
                    {

                        counter = 0;

                    }
                    else
                    {

                        counter = counter - 1;

                    }

                }
                else
                {

                    counter = 0;

                }

                if (counter > 0 && counter < pfade_fragen.Count<String>())
                {

                    PictureBox_Fragen.Source = sendeBildAnImage(pfade_fragen.ElementAt<String>(counter));

                }

            }

            else
            {
                if (richtung == "vor")
                {
                    if (counter2 == pfade_antworten.Count<String>())
                    {

                        counter2 = pfade_antworten.Count<String>();

                    }
                    else
                    {

                        counter2 = counter2 + 1;

                    }

                }
                else if (richtung == "zurück")
                {
                    if (counter2 == 0)
                    {

                        counter2 = 0;

                    }
                    else
                    {

                        counter2 = counter2 - 1;

                    }

                }
                else
                {

                    counter = 0;

                }

                if (counter2 > 0 && counter2 < pfade_antworten.Count<String>())
                {

                    InkCanvas_Antwort.Background = new ImageBrush(sendeBildAnImage(pfade_antworten.ElementAt<String>(counter2)));

                }

                /*
                catch (Exception ex)
                {
                    MessageBox.Show("Blättern der Bilder fehlgeschlagen.\n" + ex.Message);
                }
                */
            }
            
        }

        private void Button_Lösung_naechstes_Bild_Click(object sender, RoutedEventArgs e)
        {
            //PictureBox_Lösung.Source = 
            //TextBox_Lösung_Punke.Text = counter.ToString();

            bilderBlättern("vor", false);

        }

        private void Button_Lösung_letztes_Bild_Click(object sender, RoutedEventArgs e)
        {

            //TextBox_Lösung_Punke.Text = counter.ToString();

            bilderBlättern("zurück", false);

        }

        private void Button_Antwort_naechstes_Bild_Click(object sender, RoutedEventArgs e)
        {
            bilderBlättern("vor", true);
        }

        private void Button_Antwort_letztes_Bild_Click(object sender, RoutedEventArgs e)
        {
            bilderBlättern("zurück", true);
        }

        private void Button_Lösung_speichern_Click(object sender, RoutedEventArgs e)
        {         
            //schreibe_DB_Fragen(returnhHashValue(pfade_fragen.ElementAt<String>(counter)), Int16.Parse(TextBox_Lösung_Punke.Text));
            TextBox_System_Nachricht.Text = TextBox_System_Nachricht.Text + returnhHashValue(pfade_fragen.ElementAt<String>(counter)) + "\n";
        }

        private void Button_DB_erstellen_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                pfade_fragen = Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/fragen");
                pfade_antworten = Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/antworten");
                //MessageBox.Show("Es gibt " + pfade.Count<String>().ToString() + " Bilder.");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Datenbank bauen
            for (int i = 0; i < pfade_fragen.Count<String>(); i++)
            {
                schreibehashWerteInDBTab("fragen", returnhHashValue(pfade_fragen.ElementAt<String>(i)));

            }

            for (int i = 0; i < pfade_antworten.Count<String>(); i++)
            {
                schreibehashWerteInDBTab("antworten", returnhHashValue(pfade_antworten.ElementAt<String>(i)));
            }
            TextBox_System_Nachricht.Text = "Dantenbank erfolgreich aufgebaut.";

        }

        private void Button_Antwort_Punkte_speichern_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_fragen_wählen_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if( !VistaFolderBrowserDialog.IsVistaFolderDialogSupported )
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if ((bool)dialog.ShowDialog(this))
                TextBox_pfad_fragen.Text = dialog.SelectedPath;
                //MessageBox.Show(this, "The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog");                
        }

        private void Button_antworten_wählen_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if ((bool)dialog.ShowDialog(this))
                TextBox_pfad_antworten.Text = dialog.SelectedPath;
            //MessageBox.Show(this, "The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog"); 
        }
    }
} 
