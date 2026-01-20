using PurchaseOrders.Application.Domain.Setup;

namespace PurchaseOrders.Application.Dtos.PurchaseOrders
{
    public class PurchaseOrderItemCreationDto
    {
        public int LineNo { get; set; }
        public Guid ItemId { get; set; }
        public Guid UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
    }
}
