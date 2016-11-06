﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AutismCommunicationApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisplayPictures : Page
    {

        // A list that can store Bitmaps 
        List<BitmapImage> storeBitmaps = new List<BitmapImage>();

        public DisplayPictures()
        {
            this.InitializeComponent();
        }

        // Load an image
        async void loadImagesButton_Click(object sender, RoutedEventArgs e)
        {

            /*  =====  THIS BLOCK OF CODE GETS 1 FILE AND CONVERTS IT TO A BITMAP IMAGE  =====
            // Point to Local folder
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            // Point to pictures folder in the local folder
            StorageFolder picturesSubFolder = await folder.GetFolderAsync("Pictures");
            // Get the file called Test.jpg in the pictures subfolder
            StorageFile file = await picturesSubFolder.GetFileAsync("Test.jpg");
            // ********** adapted from  http://windowsapptutorials.com/tips/storagefile/convert-storagefile-to-a-bitmapimage-in-universal-windows-apps/  **********
            // Convert storage file to bitmap image
            BitmapImage bitmapImage = await ImageUtils.StorageFileToBitmapImage(file);
            firstImage.Source = bitmapImage;
            */

            // Point to Local folder
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            // Point to pictures folder in the local folder
            StorageFolder picturesSubFolder = await folder.GetFolderAsync("Pictures");

            // Read all the files in the Pictures folder and store them in a list
            IReadOnlyList<StorageFile> pictureList = await picturesSubFolder.GetFilesAsync();

            //Debug.WriteLine(pictureList.Count());

            
            // Loop through the list 
            foreach (StorageFile file in pictureList)
            {

                // Convert each Storage File into a bitmap
                BitmapImage bitmapImage = await ImageUtils.StorageFileToBitmapImage(file);

                Debug.WriteLine(bitmapImage);

                // Place bitmaps into a list
                storeBitmaps.Add(bitmapImage);

            }// End foreach loop

            firstImage.Source = storeBitmaps.ElementAt(0);
            secondImage.Source = storeBitmaps.ElementAt(1);
            
        }// End loadImagesButton_Click

    }// End class DisplayIamges


    /*
     * 
     *  Copyright http://windowsapptutorials.com/tips/storagefile/convert-storagefile-to-a-bitmapimage-in-universal-windows-apps/
     * 
    */
    public class ImageUtils
    {
        public static async Task<BitmapImage> StorageFileToBitmapImage(StorageFile savedStorageFile)
        {
            using (IRandomAccessStream fileStream = await savedStorageFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelHeight = 200;
                bitmapImage.DecodePixelWidth = 200;
                await bitmapImage.SetSourceAsync(fileStream);
                return bitmapImage;
            }
        }
    }

}// End namespace AutismCommunicationApp