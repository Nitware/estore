using FluentValidation;
using SmartStore.Admin.Models.Promotions;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Promotions
{
    public partial class PromotionProductsValidator : AbstractValidator<PromotionProductsModel>
    {
        public PromotionProductsValidator(ILocalizationService localizationService)
        {
        }
    }
}