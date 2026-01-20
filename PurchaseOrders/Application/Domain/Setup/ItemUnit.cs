using PurchaseOrders.Application.Domain.Base;
using System.Reflection.PortableExecutable;

namespace PurchaseOrders.Application.Domain.Setup
{
    public class ItemUnit : BaseEntity
    {
        public Guid ItemId { get; set; }
        public Guid UnitId { get; set; }
        public Unit Unit { get; set; } = default!;
    }
}
