using PurchaseOrders.Application.Domain.Base;

namespace PurchaseOrders.Application.Domain.Setup
{
    public class Currency : BaseEntity
    {
        public string Name { get; set; } = default!;
    }
}
