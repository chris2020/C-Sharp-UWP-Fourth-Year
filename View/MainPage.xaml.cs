﻿using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using AutismCommunicationApp.DataModel;
using ViewModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;
using Windows.Media.Capture;
using Windows.Foundation;
using AutismCommunicationApp.View;

namespace AutismCommunicationApp
{
    public sealed partial class MainPage : Page
    {

        // Private list used to bind to MainPage view
        private ObservableCollection<Picture> Pictures;
        private ObservableCollection<Picture> communicationBar;
        private bool editEnabled;
        private string pin;
        private string sentenceSize;

        public object SupportedOrientations { get; private set; }

        public MainPage()
        {

            this.InitializeComponent();

            // Save data to this list
            this.Pictures = PictureManager.loadData();

            this.communicationBar = new ObservableCollection<Picture>();

            editEnabled = false;

        }// End Constructor

        // Code for humburger button click
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {

            //  ====================  LOCKS AND UNLOCKS THE MENU  ====================

            // If User is in edit mode and they click the hamburger mode then it locks the menu  
            if (MySplitView.IsPaneOpen == true)
            {

                if (PinCodeStackPanel.Visibility == Visibility.Collapsed)
                {

                    // Keep the pin code stackpanel closed
                    PinCodeStackPanel.Visibility = Visibility.Collapsed;

                    // Close the splitview which it turn locks the menu
                    MySplitView.IsPaneOpen = false;

                }// End nested if

            }// End if
            else {

                // If panel is closed when button is pressed then open it otherwise if it is open then close it
                PinCodeStackPanel.Visibility = (PinCodeStackPanel.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;

            }

            //  ====================  LOADS THE PIN AND PROMPTS THE USER  ====================

            var pin = loadPinCode();

            if (String.IsNullOrEmpty(pin))
            {

                // Empty out the pin code text box when hamburger button is clicked
                PinCodeTextBox.Text = "";

            }
            else {

                // Show pin to user
                PinCodeTextBlock.Text = pin.ToString();

            }

        }// End HamburgerButton_Click

        // ====================  FILE EXPLORER BUTTON  ====================

        // This button opens file explorer, copies the selected file and navigates to the ImageDetails page
        private async void MenuButton1_Click(object sender, RoutedEventArgs e)
        {

            // If pin code has been entered correctly and menu has been unlocked
            if (MySplitView.IsPaneOpen == true)
            {

                //  ----------  adapted from http://stackoverflow.com/questions/39111925/uwp-copy-file-from-fileopenpicker-to-localstorage  ----------

                // Open file explorer
                var pickerOpen = new FileOpenPicker();

                // Have a filter for png files
                pickerOpen.FileTypeFilter.Add(".png");
                pickerOpen.FileTypeFilter.Add(".jpg");
                pickerOpen.FileTypeFilter.Add(".jpeg");

                // pick file and store it in a storage file
                StorageFile storageFile = await pickerOpen.PickSingleFileAsync();

                // Check if file is selected
                if (storageFile != null)
                {
                    // Copy the file into the devices local storage
                    // If file alredy exists replace the copy
                    await storageFile.CopyAsync(ApplicationData.Current.LocalFolder, storageFile.Name, NameCollisionOption.ReplaceExisting);
                }

                // If a file has been selected
                if (storageFile != null)
                {

                    // Navigate to page where image is displayed and label can be set
                    // Pass the file name along
                    this.Frame.Navigate(typeof(ImageDetails), storageFile.Name);

                }// End if

            }// End if

        }// End MenuButton1_Click

        //  ====================  DRAG AND DROP FROM DISPLAY TO COMMUNICATION BAR  ====================

        //  ----------  Adapted from http://www.shenchauhan.com/blog/2015/8/23/drag-and-drop-in-uwp  ----------

        // Code for gridview displaying pictures
        private void DisplayPictures_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
           
            // Get ID of card and set as a string
            var card = string.Concat(e.Items.Cast<Picture>().Select(i => i.pictureId));
            e.Data.SetText(card);
            e.Data.RequestedOperation = DataPackageOperation.Move;

        }

        // Code for communication bar
        private void CommunicationBar_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }

        // Moves the item being dragged into an observable collection and moves it into a different 
        // gridview on the screen
        private async void CommunicationBar_Drop(object sender, DragEventArgs e)
        {

            var localSettings = ApplicationData.Current.LocalSettings;

            // Load value of sentence size into Object
            Object size = localSettings.Values["sentenceSize"];

            // Store value of object in  a string
            sentenceSize = size.ToString();

            // Convert string value to integer
            int maxPictures = Int32.Parse(sentenceSize);

            // If a string is being passed over 
            if (e.DataView.Contains(StandardDataFormats.Text))
            {

                // Retrieve the pictures id and store in string
                var id = await e.DataView.GetTextAsync();

                var itemIdsToMove = id.Split(',');

                var destinationListView = sender as GridView;
                var listViewItemsSource = destinationListView.ItemsSource as ObservableCollection<Picture>;

                if (listViewItemsSource != null)
                {

                    // Loop through list containing picture
                    foreach (var itemId in itemIdsToMove)
                    {

                        // Catch exception that won't let you drop an image in the same list it is already in
                        try
                        {

                            // Find picture in list 
                            var itemToMove = this.Pictures.First(i => i.pictureId.ToString() == itemId);

                            // If communication bar has no more than 2 pictures in it
                            if (listViewItemsSource.Count() < maxPictures)
                            {

                                // Move picture to communication bar
                                listViewItemsSource.Add(itemToMove);

                                // Remove picture from display 
                                this.Pictures.Remove(itemToMove);

                            }// End if
                        }
                        catch (InvalidOperationException){ }
                        
                    }// End foreach
                    
                }// End if

            }// End if

        }// End CommunicationBar_Drop

        //  ====================  DRAG AND DROP FROM COMMUNICATION BAR TO DISPLAY  ====================

        private void CommunicationBar_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {

            // Get ID of card and set as a string
            var card = string.Concat(e.Items.Cast<Picture>().Select(i => i.pictureId));
            e.Data.SetText(card);
            e.Data.RequestedOperation = DataPackageOperation.Move;

        }

        private void DisplayPictures_DragOver(object sender, DragEventArgs e)
        {

            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }

        }

        private async void DisplayPictures_Drop(object sender, DragEventArgs e) 
        {
            // If a string is being passed over 
            if (e.DataView.Contains(StandardDataFormats.Text))
            {

                // Retrieve the pictures id and store in string
                var id = await e.DataView.GetTextAsync();

                // Created an array with picture ID
                var itemIdsToMove = id.Split(',');

                var destinationListView = sender as GridView;
                var listViewItemsSource = destinationListView.ItemsSource as ObservableCollection<Picture>;

                if (listViewItemsSource != null)
                {

                    // Loop through list containing picture
                    foreach (var itemId in itemIdsToMove) 
                    {

                        // Catch exception that won't let you drop an image in the same list it is already in
                        try
                        {

                            // Find picture in communication bar list 
                            var itemToMove = this.communicationBar.First(i => i.pictureId.ToString() == itemId);

                            // Move picture to communication bar
                            listViewItemsSource.Add(itemToMove);

                            // Remove picture from communication bar 
                            this.communicationBar.Remove(itemToMove);

                        }catch (InvalidOperationException) { }

                    }// End foreach

                }// End nested if

            }// End if

        }// End DisplayPictures_Drop

        // ====================  EDIT MODE FUNCTIONALITY  ====================

        // Button will enable edit mode
        private void Edit_Click(object sender, RoutedEventArgs e)
        {

            // Get access to resources for localisation
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            // Retrieve reqired strings
            var editEnabledLocalisation = loader.GetString("editEnabled");
            var editDisabledLocalisation = loader.GetString("editDisabled");

            // Switch the value of editEnabled to the opposite of what is currently is 
            editEnabled = (editEnabled == true) ? false : true;

            // Change the content of the edit textblock to show if it's enabled or disabled
            EditTextBlock.Text = (editEnabled == true) ? editEnabledLocalisation : editDisabledLocalisation;

        }// End Edit_Click

        private void DisplayPictures_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {

            // Adapted from http://stackoverflow.com/questions/13696210/how-to-get-tapped-item-in-gridview

            // Get the details of the selected Card
            var card = (Picture)(sender as GridView).SelectedItem;

            // If edit mode is enabled navigate to the edit page
            // Pass over the card with the details
            if (editEnabled == true)
                this.Frame.Navigate(typeof(EditCardPage), card);

        }// DisplayPictures_DoubleTapped

        //  ----------  Adapted from https://msdn.microsoft.com/en-us/windows/uwp/launch-resume/launch-default-app  ----------
        // Button to lauch the web browser
        private async void WebBrowser_Click(object sender, RoutedEventArgs e)
        {

            // If pin code has been entered correctly and menu has been unlocked
            if (MySplitView.IsPaneOpen == true)
            {
                // The URI to launch
                var google = new Uri(@"http://www.google.com");

                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(google);

                // If it fails go back to main page
                if (!success)
                {

                    // Return to main page if browser can't open
                    this.Frame.Navigate(typeof(MainPage));

                }// End if
            }
          
        }// End WebBrowser_Click

        // Method for taking a photo with the devices camera
        private async void Camera_Click(object sender, RoutedEventArgs e)
        {

            // If pin code has been entered correctly and menu has been unlocked
            if (MySplitView.IsPaneOpen == true)
            {

                CameraCaptureUI captureUI = new CameraCaptureUI();
                captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
                captureUI.PhotoSettings.CroppedSizeInPixels = new Size(300, 300);

                StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

                if (photo == null)
                {
                    // User cancelled photo capture
                    return;
                }
                else
                {
                    // Save file to applicaitons local folder
                    await photo.CopyAsync(ApplicationData.Current.LocalFolder, photo.Name, NameCollisionOption.ReplaceExisting);
                }

                // If a file has been selected
                if (photo != null)
                {

                    // Navigate to page where image is displayed and label can be set
                    // Pass the file name along
                    this.Frame.Navigate(typeof(ImageDetails), photo.Name);

                }// End if

            }// End if

        }// End Camera_Click

        //  ====================  PIN CODE FUNCTIONS  ====================

        private string loadPinCode()
        {

            // Access Local Settings
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Load pin code
            Object value = localSettings.Values["pinCode"];

            // Convert object to a string
            pin = value.ToString();

            if (value == null)
            {
                return null;
            }
            else
            {
                return pin;
            }

        }// End loadPinCode

        // OK Button 
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            // If code is correct unlock menu
            if (string.Equals(pin, PinCodeTextBox.Text))
            {

                // Close pin code stackpanel
                PinCodeStackPanel.Visibility = Visibility.Collapsed;

                // Open the pane
                MySplitView.IsPaneOpen = true;

            }
            else {

                // Clear pin code text box
                PinCodeTextBox.Text = "";

            }

        }// End  OKButton_Click

        // Cancel Button
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {

            // Clear pin code text box
            PinCodeTextBox.Text = "";

            // Close pin code stackpanel
            PinCodeStackPanel.Visibility = Visibility.Collapsed;

        }// End cancelButton_Click

        // Button click for settings
        private void Settings_Click(object sender, RoutedEventArgs e)
        {

            // If pin code has been entered correctly and menu has been unlocked
            if (MySplitView.IsPaneOpen == true)
            {
                this.Frame.Navigate(typeof(UserSettingsPage));
            }

        }
    }// End class MainPage

}// End namespace AutismCommunicationApp


