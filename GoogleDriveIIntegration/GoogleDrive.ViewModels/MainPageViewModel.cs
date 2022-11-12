using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GoogleDrive.Services.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace GoogleDrive.ViewModels
{
    public partial class MainPageViewModel : ViewModelBase
    {
        #region events

        #endregion

        #region vars
        IGoogleDriveService _gdriveSvc;
        string _homeapp_folderId;
        #endregion

        #region properties
        private bool _EnableControls = false;
        public bool EnableControls
        {
            get => _EnableControls;
            set { Set(ref _EnableControls, value); }
        }

        private string _Message = string.Empty;
        public string Message
        {
            get => _Message;
            set { Set(ref _Message, value); }
        }
        #endregion

        #region commands
        public ICommand Command_WebLogin { get; set; }
        public ICommand Command_PickFileAndUpload { get; set; }
        public ICommand Command_GetFiles { get; set; }
        #endregion

        #region ctors
        public MainPageViewModel()
        {
            InitCommands();
            _gdriveSvc = SimpleIoc.Default.GetInstance<IGoogleDriveService>();
            Message = "Tap Web Login";
        }
        #endregion

        #region command methods
        async void Command_WebLogin_Click()
        {
            var res = await _gdriveSvc.WebLogin();

            if (res)
            {
                EnableControls = res;
                Message = "Now try Pick File and Upload";
            }
            else
            {
                Message = "Failed to login";
            }
        }

        async void Command_PickFileAndUpload_Click()
        {
            var pickOptions = new PickOptions()
            {
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new string[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
                        { DevicePlatform.iOS, new string[] { "com.adobe.pdf", "com.microsoft.word.doc", "org.openxmlformats.wordprocessingml.document" } }
                    }),
                PickerTitle = "Open Doc"
            };

            try
            {
                var result = await FilePicker.PickAsync(pickOptions);

                // get "Digital Top Drawer" folder
                _homeapp_folderId = await _gdriveSvc.GetFolderIdByNameAsync("Digital Top Drawer");

                // if that didn't exists, then create a new one
                if (string.IsNullOrEmpty(_homeapp_folderId))
                    _homeapp_folderId = await _gdriveSvc.CreateFolderAsync("Digital Top Drawer");

                // just make sure we have a folderID
                if (!string.IsNullOrEmpty(_homeapp_folderId))
                {
                    Message = "Uploading ...";

                    var fileid = await _gdriveSvc.SaveFile(
                        Path.GetFileName(result.FullPath),
                        File.ReadAllBytes(result.FullPath),
                        _homeapp_folderId
                    );

                    if (!string.IsNullOrWhiteSpace(fileid))
                        Message = "File uploaded!";
                }
                else
                {
                    Message = "Digital Top Drawer folder not found";
                }
            }
            catch (Exception ex)
            {
                Message = "ERROR : " + ex.Message;
            }
        }

        async void Command_GetFiles_Click()
        {
            string[] files = null;

            _homeapp_folderId = !string.IsNullOrEmpty(_homeapp_folderId) ? _homeapp_folderId : await _gdriveSvc.GetFolderIdByNameAsync("Digital Top Drawer");
            if (!string.IsNullOrEmpty(_homeapp_folderId))
            {
                Message = "Fetching ...";
                var fs = await _gdriveSvc.GetFiles(_homeapp_folderId);
                files = fs.Select(x => x.Name).ToArray();

                if(files.Any())
                {
                    Message = "Files uploaded\r\n" + string.Join("\r\n", files);
                }
                else
                {
                    Message = "No files uploaded";
                }
            }
        }
        #endregion

        #region methods
        void InitCommands()
        {
            if (Command_WebLogin == null) Command_WebLogin = new RelayCommand(Command_WebLogin_Click);
            if (Command_PickFileAndUpload == null) Command_PickFileAndUpload = new RelayCommand(Command_PickFileAndUpload_Click);
            if (Command_GetFiles == null) Command_GetFiles = new RelayCommand(Command_GetFiles_Click);
        }

        void DesignData()
        {

        }

        void RuntimeData()
        {

        }

        public async Task RefreshData()
        {

        }
        #endregion
    }
}
