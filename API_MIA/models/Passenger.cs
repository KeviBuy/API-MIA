
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ЛР3_МіА._АПІшка.models
{
    public class Passenger
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
        public double BlockedBalance { get; set; }
        public bool HasUnpaidDebts { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Roles Role { get; set; } = Roles.Passenger;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public class PassengerDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
        public double BlockedBalance { get; set; }
        public bool HasUnpaidDebts { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class PassengerCreateDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public double Balance { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class PassengerUpdateDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public double Balance { get; set; }
    }
}

