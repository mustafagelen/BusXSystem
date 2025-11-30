using BusX.Domain.Common;

namespace BusX.Domain.Entities;

public class Station : BaseEntity
{
    public string City { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}