using PurchaseOrders.Application.Domain.Base;
using PurchaseOrders.Application.Domain.PurchaseOrders.Enums;
using PurchaseOrders.Application.Domain.Setup;

namespace PurchaseOrders.Application.Domain.PurchaseOrders
{
    public class PurchaseOrder : BaseEntity
    {
        public DateOnly Date { get; set; }
        public string Ref { get; set; } = default!;
        public int Number { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;
        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; } = default!;
        public Guid CurrencyId { get; set; }
        public Currency Currency { get; set; } = default!;
        public decimal CurrencyFactor { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal RetentionPercent { get; set; }
        public decimal AdvancePayment { get; set; }
        public string? Remarks { get; set; }
        public PurchaseOrderState State { get; set; }
        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
