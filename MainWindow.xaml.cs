using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PngToCStyleByteArray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String ImageSourceFileName;

        public MainWindow()
        {
            //BatchMode();
            InitializeComponent();
        }

        private void ImageDrop(object sender, DragEventArgs e)
        {
            String infoMsg = ImageDrop_Sub(e);
            if (infoMsg != null)
            {
                TextBox.Text = infoMsg;
            }
        }

        // generate .h/.cpp
        private void BatchMode()
        {
            String directory = @"F:\myDirectory";
            String[] files = Directory.GetFiles(directory, "*.png");

            Dictionary<String, List<String>> dictionary = new Dictionary<String, List<String>>();
            foreach (string filename in files)
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                var i = name.IndexOf("_");
                String prefix = (i==-1)? name : name.Substring(0, i);

                if (!dictionary.ContainsKey(prefix))
                    dictionary.Add(prefix, new List<String>());

                dictionary[prefix].Add(filename);
            }

            using (StreamWriter headerFile = File.AppendText(directory + @"\header.h"))
            {
                foreach (String prefix in dictionary.Keys)
                {
                    using (StreamWriter sourceFile = File.AppendText(directory + @"\bitmap_" + prefix + ".cpp"))
                    {
                        foreach (String filename in dictionary[prefix])
                        {
                            Bitmap bitmap = new Bitmap(filename);
                            String name = Path.GetFileNameWithoutExtension(filename);
                            String array = BitmapToArray(bitmap, name);

                            headerFile.WriteLine(String.Format("int {0}[];", name));
                            sourceFile.WriteLine(array);
                        }
                    }
                }
            }
        }


        // sanity check
        private string ImageDrop_Sub(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return "Not a file!";

            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 1)
                return "Too many files!";

            ImageSourceFileName = files[0];

            if (!File.Exists(ImageSourceFileName))
                return "Not a file!";

            FileStream fs = null;
            try
            {
                fs = File.Open(ImageSourceFileName, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                if (fs != null)
                    fs.Close();
                return "File already in use!";
            }


            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(fs);
            }
            catch (System.Exception /*ex*/)
            {
                bitmap.Dispose();
                return "Not an image!";
            }

            return BitmapToArray(bitmap, Path.GetFileNameWithoutExtension(ImageSourceFileName));
        }

        private String BitmapToArray(Bitmap bitmap, String name)
        {
            BitmapInfo bitmapInfo = new BitmapInfo(bitmap);

            String arrayString = String.Format("int {0}[] =\n{{", name);
            for (int y = 0; y < bitmapInfo.Height; ++y)
                for (int x = 0; x < bitmapInfo.Width; ++x)
                {
                    int index = bitmapInfo.Width * y + x;
                    if (index % 32 == 0)
                        arrayString += "\n   ";
                    Color color = bitmapInfo.GetPixelColor(x, y);
                    arrayString += String.Format(" 0x{0:X2}{1:X2}{2:X2}{3:X2},", color.R, color.G, color.B, color.A);
                }
            arrayString += "\n};";

            return arrayString;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
    }
}
