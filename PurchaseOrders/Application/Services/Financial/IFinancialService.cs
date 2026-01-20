using PurchaseOrders.Application.Domain.PurchaseOrders;

namespace PurchaseOrders.Application.Services.Financial
{
    public interface IFinancialService
    {
        Task<decimal?> GetAdvancePaymentAmount(Guid purchaseOrderId);
        Task<decimal> GetAdvancePaymentDeductionsAmount(Guid purchaseOrderId);
        Task<decimal?> GetApprovedStatementsTotalAmount(Guid purchaseOrderId);
        Task<decimal> GetOriginalRegularPaymentsAmount(Guid purchaseOrderId);
        Task<decimal> GetPaidPaymentsTotal(Guid purchaseOrderId);
        decimal GetPurchaseOrderRetentionAmount(PurchaseOrder purchaseOrder);
        decimal GetPurchaseOrderTotal(PurchaseOrder purchaseOrder);
        Task<decimal> GetViolationsTotal(PurchaseOrder purchaseOrder);
    }
}