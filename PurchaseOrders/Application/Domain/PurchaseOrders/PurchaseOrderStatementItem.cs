using PurchaseOrders.Application.Domain.Base;

namespace PurchaseOrders.Application.Domain.PurchaseOrders
{
    public class PurchaseOrderStatementItem : BaseEntity
    {
        public int LineNo { get; set; }
        public decimal Quantity { get; set; }
        public Guid PurchaseOrderItemId { get; set; }
        public Guid PurchaseOrderStatementId { get; set; }
    }
}
