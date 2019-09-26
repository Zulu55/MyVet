using Prism.Navigation;

namespace MyVet.Prism.ViewModels
{
    public class ProfilePageViewModel : ViewModelBase
    {
        public ProfilePageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Title = "Modify Profile";
        }
    }
}
