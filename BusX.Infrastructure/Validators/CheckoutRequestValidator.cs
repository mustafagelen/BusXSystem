using BusX.Domain.DTOs;
using FluentValidation;

namespace BusX.Infrastructure.Validators;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequestDto>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.JourneyId).GreaterThan(0).WithMessage("Geçersiz Sefer ID.");

        RuleFor(x => x.SeatIds)
            .NotEmpty().WithMessage("En az 1 koltuk seçmelisiniz.")
            .Must(ids => ids != null && ids.Count <= 4).WithMessage("Tek işlemde en fazla 4 koltuk satın alabilirsiniz.");

        RuleFor(x => x.PassengerName)
            .NotEmpty().WithMessage("Yolcu adı boş olamaz.")
            .MaximumLength(100).WithMessage("Yolcu adı çok uzun.");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().Length(11).WithMessage("TC Kimlik No 11 haneli olmalıdır.");

        RuleFor(x => x.Gender).IsInEnum().WithMessage("Geçersiz cinsiyet seçimi.");
    }
}