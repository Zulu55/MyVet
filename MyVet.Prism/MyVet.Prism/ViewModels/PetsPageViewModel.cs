using MyVet.Common.Models;
using Prism.Navigation;

namespace MyVet.Prism.ViewModels
{
    public class PetsPageViewModel : ViewModelBase
    {
        private OwnerResponse _owner;

        public PetsPageViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            Title = "Pets";
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);

            if (parameters.ContainsKey("owner"))
            {
                _owner = parameters.GetValue<OwnerResponse>("owner");
            }
        }
    }
}
