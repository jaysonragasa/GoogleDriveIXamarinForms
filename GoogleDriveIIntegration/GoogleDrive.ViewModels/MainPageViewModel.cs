using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GoogleDrive.Services.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoogleDrive.ViewModels
{
    public partial class MainPageViewModel : ViewModelBase
    {
        #region events

        #endregion

        #region vars
        IGoogleDriveService _gdriveSvc;
        #endregion

        #region properties

        #endregion

        #region commands
        public ICommand Command_WebLogin { get; set; }
        #endregion

        #region ctors
        public MainPageViewModel()
        {
            InitCommands();
            _gdriveSvc = SimpleIoc.Default.GetInstance<IGoogleDriveService>();
        }
        #endregion

        #region command methods
        void Command_WebLogin_Click()
        {
            _gdriveSvc.WebLogin();
        }
        #endregion

        #region methods
        void InitCommands()
        {
            if (Command_WebLogin == null) Command_WebLogin = new RelayCommand(Command_WebLogin_Click);
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
