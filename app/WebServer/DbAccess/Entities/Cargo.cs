namespace DbAccess.Entities;

public partial class Cargo
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int Weight { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public virtual ICollection<CargosTransport> CargosTransports { get; set; } = new List<CargosTransport>();

    public override string ToString() => $"Cargo ID: {Id} | Title: {Title} | Weight: {Weight} | Registration number: {RegistrationNumber}";
}
