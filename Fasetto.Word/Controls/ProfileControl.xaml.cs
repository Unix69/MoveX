using System.Windows.Controls;
using System.IO;
using Movex.View.Core;
using System.Windows;
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Movex.View
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class ProfileControl : UserControl
    {
        public ProfileControl()
        {
            InitializeComponent();
            DataContext = IoC.Profile;
        }

    private void Button1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create OpenFileDialog
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                // Set filter for file extension and default file extension
                DefaultExt = ".jpg",
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"

            };

            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();
            if (result == true)
            {
                // Get the filepath of the picture
                var sourceFilepath = dlg.FileName;

                // Create the default directory for profile pictures if it does not exists
                var currWorkingDirectory = Directory.GetCurrentDirectory();
                var defaultDir = @"ProfilePictures";
                var defaultDirPath = Path.Combine(currWorkingDirectory, defaultDir);
                if (!Directory.Exists(defaultDirPath))
                {
                    try
                    {
                        Directory.CreateDirectory(defaultDir);
                    }
                    catch (Exception excep)
                    {
                        MessageBox.Show(excep.Message);
                    }
                }

                // Set the tmp filename
                var extension = Path.GetExtension(sourceFilepath);
                var filename = RandomString(6);
                var tmpFilePath = Path.Combine(defaultDirPath, filename + extension);

                // Set the final filename (if not exist yet)
                var finalFilename = RandomString(12) + extension;
                var db = new Database();
                var dictionary = db.GetValues();
                var originalFilenamePath = dictionary["ProfilePicture"];
                var originalFilename = Path.GetFileName(originalFilenamePath);
                
                // Save the cropped image
                try
                {
                    File.Copy(sourceFilepath, tmpFilePath, true);
                    var i = System.Drawing.Image.FromFile(tmpFilePath);
                    var cropped = CropImage(i);
                    var resized = ResizeImage(cropped, new System.Drawing.Size(200, 200));
                    var memStream = new MemoryStream();
                    resized.Save(memStream, ImageFormat.Jpeg);
                    
                    SaveImageToFile(memStream, Path.Combine(defaultDirPath, finalFilename));
                    memStream.Dispose();
                    i.Dispose();
                    File.Delete(tmpFilePath);
                }
                catch (Exception excep)
                {
                    MessageBox.Show(excep.Message);
                }
                
                // Store the path of the profilePicture into the User object
                // TODO
                IoC.User.SetProfilePicture(Path.Combine(defaultDirPath, finalFilename));
                /*
                if (!string.Equals(originalFilename, "profile.png"))
                    File.Delete(originalFilenamePath);
                    */
            }

            
            string RandomString(int length)
            {
                var random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            System.Drawing.Image CropImage(System.Drawing.Image image)
            {
                if (image.Width == image.Height) return image;

                var endingSize = image.Width < image.Height ? image.Width : image.Height;
                System.Drawing.Image newImage = new Bitmap(endingSize, endingSize);

                // Crop considering if the image is vertical or horizontal
                if (image.Width > endingSize) { 
                    using (var graphicsHandle = Graphics.FromImage(newImage))
                    {
                        graphicsHandle.SmoothingMode = SmoothingMode.AntiAlias;
                        graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphicsHandle.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphicsHandle.DrawImage(image, new System.Drawing.Rectangle(0, 0, endingSize, endingSize), new System.Drawing.Rectangle((image.Width-endingSize)/2, 0, endingSize, endingSize), GraphicsUnit.Pixel);
                    }
                } else
                {
                    using (var graphicsHandle = Graphics.FromImage(newImage))
                    {
                        graphicsHandle.SmoothingMode = SmoothingMode.AntiAlias;
                        graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphicsHandle.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphicsHandle.DrawImage(image, new System.Drawing.Rectangle(0, 0, endingSize, endingSize), new System.Drawing.Rectangle(0, (image.Height-endingSize)/2, endingSize, endingSize), GraphicsUnit.Pixel);
                    }
                }

                return newImage;
            }

            System.Drawing.Image ResizeImage(System.Drawing.Image image, System.Drawing.Size size, bool preserveAspectRatio = true)
            {
                if (image.Width < size.Width || image.Height < size.Height) return image;

                int newWidth;
                int newHeight;
                if (preserveAspectRatio)
                {
                    var originalWidth = image.Width;
                    var originalHeight = image.Height;
                    var percentWidth = (float)size.Width / (float)originalWidth;
                    var percentHeight = (float)size.Height / (float)originalHeight;
                    var percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                    newWidth = (int)(originalWidth * percent);
                    newHeight = (int)(originalHeight * percent);
                }
                else
                {
                    newWidth = size.Width;
                    newHeight = size.Height;
                }

                System.Drawing.Image newImage = new Bitmap(newWidth, newHeight);
                using (var graphicsHandle = Graphics.FromImage(newImage))
                {
                    graphicsHandle.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicsHandle.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphicsHandle.DrawImage(image, new System.Drawing.Rectangle(0, 0, newWidth, newWidth), new System.Drawing.Rectangle(0, 0, newWidth, newHeight), GraphicsUnit.Pixel);
                }
                return newImage;
            }

            void SaveImageToFile(MemoryStream ms, string destinationPath)
            {
               var data = ms.ToArray();

                using (var fs = new FileStream(destinationPath, FileMode.Create))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
        }


    }
}