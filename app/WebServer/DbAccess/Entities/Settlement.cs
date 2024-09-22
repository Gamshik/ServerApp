namespace DbAccess.Entities;

public partial class Settlement
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<Route> RouteEndSettlements { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteStartSettlements { get; set; } = new List<Route>();

    public override string ToString() => $"Settlement ID: {Id} | Title: {Title}";
}
