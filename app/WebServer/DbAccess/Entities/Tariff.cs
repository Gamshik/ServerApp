namespace DbAccess.Entities;

public partial class Tariff
{
    public int Id { get; set; }

    public int MinWeight { get; set; }

    public int MaxWeight { get; set; }

    public int CostPerKm { get; set; }

    public virtual ICollection<CargosTransport> CargosTransports { get; set; } = new List<CargosTransport>();

    public override string ToString() => $"Tariff ID: {Id} | Min weight: {MinWeight} | Max weight: {MaxWeight} | Cost per km: {CostPerKm}";
}
