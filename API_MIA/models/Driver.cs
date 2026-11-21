using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ЛР3_МіА._АПІшка.models
{
    public class Driver
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "ПІБ водія є обов'язковим")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я має бути від 2 до 100 символів")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Номер телефону обов'язковий")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Номер телефону повинен відповідати формату +380XXXXXXXXX")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Державний номер авто обов'язковий")]
        [RegularExpression(@"^[A-Z]{2}\d{4}[A-Z]{2}$", ErrorMessage = "Номер авто повинен бути у форматі AA1234BB")]
        public string CarPlate { get; set; } = string.Empty;

        [Range(1, 3, ErrorMessage = "Оберіть коректний клас авто (1-Standard, 2-Comfort, 3-Business)")]
        public CarClass Class { get; set; } = CarClass.Standard;

        public DriverStatus Status { get; set; } = DriverStatus.Offline;

        [Range(-90.0, 90.0, ErrorMessage = "Широта має бути від -90 до 90")]
        public double CurrentLatitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Довгота має бути від -180 до 180")]
        public double CurrentLongitude { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Roles Role { get; set; } = Roles.Driver;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public class DriverDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CarPlate { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class DriverCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CarPlate { get; set; } = string.Empty;
        public CarClass Class { get; set; } = CarClass.Standard;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class DriverUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CarPlate { get; set; } = string.Empty;
        public CarClass Class { get; set; } = CarClass.Standard;
    }
}
