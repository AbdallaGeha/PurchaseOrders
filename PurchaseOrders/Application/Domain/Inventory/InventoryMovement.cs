using PurchaseOrders.Application.Domain.Base;
using PurchaseOrders.Application.Domain.Inventory.Enums;
using System.Reflection.PortableExecutable;

namespace PurchaseOrders.Application.Domain.Inventory
{
    public class InventoryMovement : BaseEntity
    {
        public DateOnly Date { get; set; }
        public InventoryMovementKind Kind { get; set; }
        public Guid StoreId { get; set; }
        public Guid ItemId { get; set; }
        public Guid UnitId { get; set; }
        public decimal Quantity { get; set; }
        public Guid TransactionId { get; set; }
    }
}
