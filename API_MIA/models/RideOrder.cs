
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ЛР3_МіА._АПІшка.models
{
    public class RideOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string PassengerId { get; set; }
        public string? DriverId { get; set; }

        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }

        public double DistanceKm { get; set; }
        public CarClass RequestedClass { get; set; }
        public RideStatus Status { get; set; } = RideStatus.Searching;

        public double BasePrice { get; set; }
        public double SurgeMultiplier { get; set; } = 1.0;
        public double FinalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

}