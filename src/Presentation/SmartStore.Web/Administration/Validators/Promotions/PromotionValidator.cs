using FluentValidation;
using SmartStore.Admin.Models.Promotions;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Promotions
{
    public partial class PromotionValidator : AbstractValidator<PromotionModel>
    {
        public PromotionValidator(ILocalizationService localizationService)
        {
        }
    }
}