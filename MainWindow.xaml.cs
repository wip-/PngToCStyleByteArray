using System;
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

            BitmapInfo bitmapInfo = new BitmapInfo(bitmap);

            String arrayString = String.Format("int {0}[] =\n{{", Path.GetFileNameWithoutExtension(ImageSourceFileName));
            for (int y = 0; y < bitmapInfo.Height; ++y)
            for (int x = 0; x < bitmapInfo.Width; ++x )
            {
                int index = bitmapInfo.Width * y + x;
                if (index % 26 == 0)
                    arrayString += "\n   ";
                Color color = bitmapInfo.GetPixelColor(x, y);
                arrayString += String.Format(" 0x{0:X}{1:X}{2:X}{3:X},", color.A, color.R, color.G, color.B);
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
