using GalaSoft.MvvmLight.Ioc;
using GoogleDrive.Services.Services;

namespace GoogleDrive.ViewModels
{
    public class ViewModelLocator
    {
        public MainPageViewModel MainPageViewModel => SimpleIoc.Default.GetInstance<MainPageViewModel>();

        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainPageViewModel>();

            // services
            SimpleIoc.Default.Register<IGoogleDriveService, GoogleDriveService>(createInstanceImmediately: false);
        }
    }
}