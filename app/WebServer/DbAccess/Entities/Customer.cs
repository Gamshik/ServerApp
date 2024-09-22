namespace DbAccess.Entities;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CargosTransport> CargosTransports { get; set; } = new List<CargosTransport>();

    public override string ToString() => $"Customer ID: {Id} | Name: {Name}";
}
