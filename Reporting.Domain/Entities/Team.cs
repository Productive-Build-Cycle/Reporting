using System.Collections.ObjectModel;

namespace Reporting.Domain.Entities;

public sealed class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<User> Users { get; set; } = new List<User>();
}
