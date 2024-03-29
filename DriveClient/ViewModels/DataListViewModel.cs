﻿using DriveClient.Models;
using DriveClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DriveClient.ViewModels
{
    internal class DataListViewModel : BaseViewModel
    {
        /// <summary>
        /// Contains the file and directory objects listed in the grid.
        /// </summary>
        public ObservableCollection<BasicItem> BasicItems{ get; set; }
        /// <summary>
        /// Displays the actual path of the current directory.
        /// </summary>
        public string FullURLposition { get { return "Your current position: " + BasicItemService.Instance.actualPath; } }

        /// <summary>
        /// ICommand objects for the button controls.
        /// </summary>
        public ICommand AddCommand { get; set; }
        public ICommand BackCommand { get; set; }

        /// <summary>
        /// Constructor initializes the ICommand attributes with delegates.
        /// </summary>
        /// <param name="navigation">Xamarin specific thing</param>
        public DataListViewModel(INavigation navigation) : base(navigation)
        {
            LoadData(string.Empty);

            AddCommand      = new Command(AddCommandExecute);
            BackCommand     = new Command(BackCommandExecute);
        }

        /// <summary>
        /// Initializes the Basicitems container and the actual path.
        /// </summary>
        /// <param name="path">Loads files from this path.</param>
        /// <returns>-</returns>
        public async Task LoadData(string path)
        {
            var data = await BasicItemService.Instance.InitList(path);
            BasicItems = new ObservableCollection<BasicItem>(data);

            this.OnAppearing();
        }

        /// <summary>
        /// Responsible for the Back button, reinitializes the containers and the actual path variable from the previous actual path.
        /// </summary>
        /// <param name="obj"></param>
        private async void BackCommandExecute(object obj)
        {
            string path = BasicItemService.Instance.actualPath;

            int freq = path.Count(p => (p == '/'));
            if (freq > 0)
            {
                if(freq == 1)
                    await LoadData(string.Empty);
                else {
                    int index = path.LastIndexOf('/');
                    path = path.Substring(0, index);

                    await LoadData(path);
                }
            }
        }

        /// <summary>
        /// Responsible for uploading a file to the actual path.
        /// Opens Filepicker and the user must pick the file he wants to upload.
        /// Calls the DropBoxService's UploadFile function.
        /// </summary>
        /// <param name="obj"></param>
        private async void AddCommandExecute(object obj)
        {
            try
            {
                var result = await FilePicker.PickAsync();
                if(result != null)
                {
                    var stream = await result.OpenReadAsync();

                    await DropBoxService.Instance.UploadFile(result.FileName, stream);

                    await App.Current.MainPage.DisplayAlert("Success!", $"File: {result.FileName} uploaded!", "OK");
                }
            }catch(Exception){
                await App.Current.MainPage.DisplayAlert("Error", "There was an error with: opening file!", "OK");
            }
            finally {
                await LoadData(BasicItemService.Instance.actualPath);
            }
        }

        /// <summary>
        /// Refreshes the BasicItem list and the url position when it's called
        /// </summary>
        public override void OnAppearing()
        {
            base.OnAppearing();
            OnPropertyChanged(nameof(BasicItems));
            OnPropertyChanged(nameof(FullURLposition));
        }
    }
}
