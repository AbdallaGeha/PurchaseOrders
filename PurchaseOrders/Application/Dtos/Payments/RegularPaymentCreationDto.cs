namespace PurchaseOrders.Application.Dtos.Payments
{
    public class RegularPaymentCreationDto
    {
        public decimal Amount { get; set; }
        public decimal AdvanceDeduction { get; set; }
        public Guid PurchaseOrderStatementId { get; set; }
    }
}
