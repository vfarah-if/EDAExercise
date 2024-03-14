using Domain.Contracts;
using FluentValidation;

namespace Domain.FluentValidators;

public class ProductDetailValidator : AbstractValidator<ProductDetail>
{
    public ProductDetailValidator()
    {
        RuleFor(productDetail => productDetail.Sku).NotEmpty().MinimumLength(6);
        RuleFor(productDetail => productDetail.SerialNumber).NotEmpty().MinimumLength(6);
    }
}
