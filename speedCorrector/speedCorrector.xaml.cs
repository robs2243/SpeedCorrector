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
using System.Drawing.Imaging;
using System.Drawing;
//using ExifLibrary;
using System.ComponentModel;
using ExifUtils.Exif;
using ExifUtils.Exif.IO;
using ExifUtils.Exif.TagValues;


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
            System.Windows.Controls.Image myImage3 = new System.Windows.Controls.Image();
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
                    //Console.WriteLine(pfade_antworten.ElementAt<String>(counter2));
                    metadata_reader(pfade_antworten.ElementAt<String>(counter2));

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
                pfade_fragen = Directory.EnumerateFiles(TextBox_pfad_fragen.Text);
                pfade_antworten = Directory.EnumerateFiles(TextBox_pfad_antworten.Text);
                //pfade_fragen = Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/fragen");
                //pfade_antworten = Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/antworten");
                //MessageBox.Show("Es gibt " + pfade.Count<String>().ToString() + " Bilder.");



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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        private void Button_Antwort_Punkte_speichern_Click(object sender, RoutedEventArgs e)
        {

        }

        private String pfad_auswählen()
        {
            String pfad;
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Wählen Sie bitte den entsprechenden Ordner.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if ((bool)dialog.ShowDialog(this))
            {
                pfad = dialog.SelectedPath;
                return pfad;
                //MessageBox.Show(this, "The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog"); 
            }
            else
            {
                return "";
            }
        }

        private void Button_fragen_wählen_Click(object sender, RoutedEventArgs e)
        {
            TextBox_pfad_fragen.Text = pfad_auswählen();
        }

        private void Button_antworten_wählen_Click(object sender, RoutedEventArgs e)
        {
            TextBox_pfad_antworten.Text = pfad_auswählen();
        }

        private void TextBox_pfad_fragen_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox_pfad_fragen.Text = pfad_auswählen();
        }

        private void TextBox_pfad_antworten_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox_pfad_antworten.Text = pfad_auswählen();
        }



        private void metadatei()
        {
            // Create an Image object. 
            System.Drawing.Image theImage = new Bitmap("c:\\fotos\\2.jpg");

            // Get the PropertyItems property from image.
            PropertyItem[] propItems = theImage.PropertyItems;

            int count = 0;
            foreach (PropertyItem propItem in propItems)
            {
                Console.WriteLine("Id: " + propItem.Id.ToString("x"));
                Console.WriteLine("Type: " + propItem.Type.ToString());
                Console.WriteLine("Länge: " + propItem.Len.ToString());
                Console.WriteLine("Wert: " + System.Text.Encoding.UTF8.GetString(propItem.Value));
            }


        }
        /*
        private void metadatei2()
        {
            ImageFile data = ImageFile.FromFile("metadata.jpg");
            ExifProperty item;
            
            //ListViewItem lvitem = new ListViewItem();
                       
            //data.Properties
            //GetName(typeof(IFD), ExifTagFactory.GetTagIFD(item.Tag));
            //ExifTagFactory.GetTagIFD()
            //ExifTagFactory.GetExifTag()
            
            item = data.Properties[0];
            item.Value = "C91361CAEC37DD3FA018FE1CF6A2EAB28F0BC04898ADAE473EFD3D8665ADFC80";
            item = data.Properties[1];
            item.Value = "hss-06641";
            item = data.Properties[28];
            item.Value = "lsg-0123456789";
            Console.WriteLine(item.ToString());
            Console.WriteLine(item.Interoperability.TypeID);
            data.Save("c:\\fotos\\2_ver.jpg");
            

            //ExifProperty item;
            //item = data.Properties[ExifTagFactory.GetTagID( y.Get()]
            int stelle = 0;
            //int[] werte = { 0, 1, 28, 19};
            //ExifProperty asfd = new ExifProperty(ExifTag bla);
            //IFD.EXIF
            //xifTag.GPSAltitude;

            //var bla = new ExifProperty(ExifTag.GPSAltitude)

            //ExifProperty copyright = new ExifProperty();
            //copyright.Tag = ExifTag.Copyright;

            //ExifTag wert = 12;
            //ExifProperty item2;
            //data.Properties.Add(ExifTagFactory.GetExifTag(ExifTag.GPSAltitude),)

            //ExifTagFactory tag;

            //data.Properties.Add(ExifTagFactory.)

            Console.WriteLine("Anzahl Eingenschaften: " + data.Properties.Count);
            try
            {
                for (int i = 0; i < data.Properties.Count; i++)
                {
                    
                    item = data.Properties[i];
                    if (item.Interoperability.TagID == 33432)
                    {
                        item.Value = "FICK DICH!";
                        Console.WriteLine("Geändert!");
                    }

                    //item.Value = "C91361CAEC37DD3FA018FE1CF6A2EAB28F0BC04898ADAE473EFD3D8665ADFC80";
                    //Console.WriteLine("TagID: " + item.Interoperability.TagID);
                    //Console.WriteLine("Name: " + item.Name);
                    stelle++;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler bei: " + stelle + " " + ex.Message);
            }
            data.Save("metadata.jpg");
            
            int zähler = 0;
            foreach (ExifProperty item in data.Properties)
            {
                
                //ListViewItem lvitem = new ListViewItem(item.Name);
                Console.WriteLine("Nummer: " + zähler +" Item: " + item.ToString() + " Typ: " + item.Interoperability.TypeID);
                //lvitem.SubItems.Add(Enum.GetName(typeof(IFD), ExifTagFactory.GetTagIFD(item.Tag)));
                //Console.WriteLine(IFD.EXIF);
                //Console.WriteLine(ExifTagFactory.GetTagIFD(item.Tag));

                //lvitem.Tag = item;
                //lvExif.Items.Add(lvitem);
                zähler++;

            }
            

        }
        */
        private void Button_meta_Click(object sender, RoutedEventArgs e)
        {
            //metadatei2();
            metadata_writer("Klaus Petersen", "E1BT1", "21.02.2018", "1b", "10", "5", "LBTE", "C91361CAEC37DD3FA018FE1CF6A2EAB28F0BC04898ADAE473EFD3D8665ADFC80",
                @"metadata.jpg");
        }

        private void metadata_writer(String Schuler, String Klasse, String Datum, String Aufgabe, String maxPunkte, String erreichtePunkte, 
            String Klassenarbeit, String HashWert, String pfadBilddatei)
        {
            ExifProperty copyright = new ExifProperty();
            ExifProperty ImageDescription = new ExifProperty();
            ExifProperty Make = new ExifProperty();
            ExifProperty Model = new ExifProperty();
            ExifProperty GPSLatitudeRef = new ExifProperty();
            ExifProperty Software = new ExifProperty();
            ExifProperty DateTime = new ExifProperty(); //nur zahl
            ExifProperty Artist = new ExifProperty(); //nur zahl

            copyright.Tag = ExifTag.Copyright;
            ImageDescription.Tag = ExifTag.ImageDescription;
            Make.Tag = ExifTag.Make;
            Model.Tag = ExifTag.Model;
            GPSLatitudeRef.Tag = ExifTag.GpsLatitudeRef;
            Software.Tag = ExifTag.Software;
            DateTime.Tag = ExifTag.DateTime;
            Artist.Tag = ExifTag.Artist;

            copyright.Value = String.Format(Schuler);
            ImageDescription.Value = String.Format(Klasse);
            Make.Value = String.Format(Datum);
            Model.Value = String.Format(Aufgabe);
            DateTime.Value = String.Format(maxPunkte);
            Artist.Value = String.Format(erreichtePunkte);
            GPSLatitudeRef.Value = String.Format(Klassenarbeit);
            Software.Value = String.Format(HashWert);
                        
            ExifPropertyCollection properties = ExifReader.GetExifData(pfadBilddatei);     
            
            FileStream datenstrom = File.OpenRead(pfadBilddatei);                
            System.Drawing.Image bilddatei = System.Drawing.Image.FromStream(datenstrom);

            try { ExifWriter.AddExifData(bilddatei, copyright);} catch { MessageBox.Show("Fehler: copyright"); } 
            try { ExifWriter.AddExifData(bilddatei, ImageDescription); } catch { MessageBox.Show("Fehler: ImageDescription"); }
            try { ExifWriter.AddExifData(bilddatei, Make); } catch { MessageBox.Show("Fehler: Make"); }
            try { ExifWriter.AddExifData(bilddatei, Model); } catch { MessageBox.Show("Fehler: Model"); }
            try { ExifWriter.AddExifData(bilddatei, GPSLatitudeRef); } catch { MessageBox.Show("Fehler: GPSLatitudeRef"); }
            try { ExifWriter.AddExifData(bilddatei, Software); } catch { MessageBox.Show("Fehler: Software"); }
            try { ExifWriter.AddExifData(bilddatei, DateTime); } catch { MessageBox.Show("Fehler: DateTime"); }
            try { ExifWriter.AddExifData(bilddatei, Artist); } catch { MessageBox.Show("Fehler: ISOSpeedRatings"); }

            bilddatei.Save("dummy.jpg");
            datenstrom.Close();
            kopieren(pfadBilddatei);

        }

        private void metadata_reader(String pfadBilddatei)
        {
            Console.WriteLine(pfadBilddatei.ToString());
            ExifPropertyCollection properties = ExifReader.GetExifData(@"dummy.jpg");

            foreach (ExifProperty property in properties)
            {
                // Console.WriteLine("{0}.{1}: \"{2}\"",
                //  property.Tag.GetType().Name,
                //   property.Tag,
                //   property.DisplayName);
                //Console.WriteLine("{0}: {1}",
                //    GetPropertyTypeName(property.Value),
                //    property.Value);
                Console.WriteLine(property.DisplayValue);
                //Console.WriteLine();
            }
        }
        
       private void kopieren(String zieldatei)
        {
            System.Drawing.Image img;
            var bmpTemp = new Bitmap("dummy.jpg");
            img = new Bitmap(bmpTemp);            
            File.Copy("dummy.jpg", zieldatei, true);
        }

       private static string GetPropertyTypeName(object value)
		{
			if (value == null)
			{
				return "null";
			}

			Type type = value.GetType();

			return GetPropertyTypeName(type, type.IsArray ? ((Array)value).Length : 0);
		}

       private static string GetPropertyTypeName(Type type, int length)
        {
            if (type == null)
            {
                return "null";
            }

            if (type.IsArray || type.HasElementType)
            {
                return GetPropertyTypeName(type.GetElementType(), 0) + '[' + length + ']';
            }

            if (type.IsGenericType)
            {
                string name = type.Name;
                if (name.IndexOf('`') >= 0)
                {
                    name = name.Substring(0, name.IndexOf('`'));
                }
                name += '<';
                Type[] args = type.GetGenericArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                    {
                        name += ',';
                    }
                    name += args[i].Name;
                }
                name += '>';
                return name;
            }

            if (type.IsEnum)
            {
                return type.Name + ':' + Enum.GetUnderlyingType(type).Name;
            }

            return type.Name;
        }

       private void Button_meta_reader_Click(object sender, RoutedEventArgs e)
        {
           // metadata_reader(InkCanvas_Antwort.Background);
        }

        private void TextBox_Schuler_Copy1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
} 
