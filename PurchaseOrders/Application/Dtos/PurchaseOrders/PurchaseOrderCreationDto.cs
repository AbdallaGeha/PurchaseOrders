using PurchaseOrders.Application.Domain.PurchaseOrders.Enums;
using PurchaseOrders.Application.Domain.PurchaseOrders;
using PurchaseOrders.Application.Domain.Setup;

namespace PurchaseOrders.Application.Dtos.PurchaseOrders
{
    public class PurchaseOrderCreationDto
    {
        public string Date { get; set; } = default!;
        public string Ref { get; set; } = default!;
        public int Number { get; set; }
        public Guid ProjectId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid CurrencyId { get; set; }
        public decimal CurrencyFactor { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal RetentionPercent { get; set; }
        public decimal AdvancePayment { get; set; }
        public string? Remarks { get; set; }
        public List<PurchaseOrderItemCreationDto> Items { get; set; } = new List<PurchaseOrderItemCreationDto>();
    }
}
