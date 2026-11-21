namespace ЛР3_МіА._АПІшка.models
{
    public class DataContainer
    {
        public List<Driver> Drivers { get; set; } = new();
        public List<Passenger> Passengers { get; set; } = new();
        public List<RideOrder> RideOrders { get; set; } = new();
    }
}
