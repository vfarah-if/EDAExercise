using Domain.Contracts;
using FluentValidation;

namespace Domain.FluentValidators;

public class ManifestDetailValidator : AbstractValidator<ManifestDetail>
{
    public ManifestDetailValidator()
    {
        RuleFor(manifestDetail => manifestDetail.SourceSystemId).NotEmpty().MinimumLength(6);
        RuleFor(manifestDetail => manifestDetail.EventId).NotEmpty().MinimumLength(24);
        RuleFor(manifestDetail => manifestDetail.Timestamp).NotEmpty();
        RuleFor(manifestDetail => manifestDetail.PurchaseOrderNumber).NotEmpty().MinimumLength(6);
        RuleFor(manifestDetail => manifestDetail.DeliveryLocation).InclusiveBetween(1, 1000);
        RuleFor(manifestDetail => manifestDetail.Products)
            .NotEmpty()
            .Must(productDetail => productDetail?.Count >= 1)
            .WithMessage("At least one product must be provided");

        RuleForEach(manifestDetail => manifestDetail.Products)
            .SetValidator(new ProductDetailValidator());
    }
}
