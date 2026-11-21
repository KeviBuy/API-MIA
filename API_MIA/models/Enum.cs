namespace ЛР3_МіА._АПІшка.models
{
    public enum CarClass
    {
        Standard = 1,
        Comfort = 2,
        Business = 3
    }

    public enum DriverStatus
    {
        Offline = 0,
        Online = 1,
        Busy = 2
    }

    public enum RideStatus
    {
        Searching = 1,
        DriverAssigned = 2,
        Arrived = 3,
        InProgress = 4,
        Completed = 5,
        Cancelled = 6
    }

    [Flags]
public enum Roles
{
    None = 0,
    Passenger = 1 << 0,
    Driver = 1 << 1,
    Admin = 1 << 2
}
}
