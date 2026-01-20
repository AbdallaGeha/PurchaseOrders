namespace PurchaseOrders.Application.Dtos.PurchaseOrderStatements
{
    public class StatementItemUpdateDto
    {
        public Guid? Id { get; set; }
        public int LineNo { get; set; }
        public decimal Quantity { get; set; }
        public Guid PurchaseOrderItemId { get; set; }
    }
}
