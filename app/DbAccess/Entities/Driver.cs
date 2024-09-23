namespace DbAccess.Entities;

public partial class Driver
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PassportNumber { get; set; } = null!;

    public virtual ICollection<CargosTransport> CargosTransports { get; set; } = new List<CargosTransport>();

    public override string ToString() => $"Driver ID: {Id} | FirstName: {FirstName} | LastName: {LastName} | Passport number: {PassportNumber}";
}
