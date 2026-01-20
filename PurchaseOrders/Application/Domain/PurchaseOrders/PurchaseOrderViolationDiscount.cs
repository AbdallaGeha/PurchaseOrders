using PurchaseOrders.Application.Domain.Base;

namespace PurchaseOrders.Application.Domain.PurchaseOrders
{
    public class PurchaseOrderViolationDiscount : BaseEntity
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = default!;
        public Guid PurchaseOrderStatementId { get; set; }
    }
}
