using PurchaseOrders.Application.Domain.PurchaseOrders.Enums;
using PurchaseOrders.Application.Domain.PurchaseOrders;
using PurchaseOrders.Application.Domain.Setup;

namespace PurchaseOrders.Application.Dtos.PurchaseOrders
{
    public class PurchaseOrderUpdateGetDto
    {
        public Guid Id { get; set; }
        public string Date { get; set; } = default!;
        public string Ref { get; set; } = default!;
        public int Number { get; set; }
        public string ProjectId { get; set; } = default!;
        public string SupplierId { get; set; } = default!;
        public string CurrencyId { get; set; } = default!;
        public decimal CurrencyFactor { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal RetentionPercent { get; set; }
        public decimal AdvancePayment { get; set; }
        public string? Remarks { get; set; }
        public short State { get; set; }
        public List<PurchaseOrderItemUpdateGetDto> Items { get; set; } = new List<PurchaseOrderItemUpdateGetDto>();
    }
}
