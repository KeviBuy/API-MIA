using FluentValidation;
using ЛР3_МіА._АПІшка.models;

namespace ЛР3_МіА._АПІшка.Validators
{
    public class PassengerValidator : AbstractValidator<Passenger>
    {
        public PassengerValidator()
        {
            RuleFor(p => p.FullName)
                .NotEmpty().WithMessage("ПІБ пасажира не може бути порожнім")
                .Length(2, 100).WithMessage("ПІБ має містити від 2 до 100 символів");

            RuleFor(p => p.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефону обов'язковий")
                .Matches(@"^\+380\d{9}$").WithMessage("Номер телефону повинен відповідати формату +380XXXXXXXXX (Regex)");

            RuleFor(p => p.Balance)
                .GreaterThanOrEqualTo(0).WithMessage("Баланс не може бути від'ємним");
        }
    }

    public class RideOrderValidator : AbstractValidator<RideOrder>
    {
        public RideOrderValidator()
        {
            RuleFor(r => r.PassengerId).NotEmpty().WithMessage("PassengerId є обов'язковим");

            RuleFor(r => r.PickupLatitude).InclusiveBetween(-90.0, 90.0);
            RuleFor(r => r.PickupLongitude).InclusiveBetween(-180.0, 180.0);
            RuleFor(r => r.DropoffLatitude).InclusiveBetween(-90.0, 90.0);
            RuleFor(r => r.DropoffLongitude).InclusiveBetween(-180.0, 180.0);

            RuleFor(r => r.DistanceKm)
                .GreaterThanOrEqualTo(0.5).WithMessage("Мінімальна дистанція поїздки — 0.5 км (500 метрів)");

            RuleFor(r => r.SurgeMultiplier)
                .InclusiveBetween(1.0, 2.0).WithMessage("Динамічний множник має бути у межах від 1.0x до 2.0x");
        }
    }
}
