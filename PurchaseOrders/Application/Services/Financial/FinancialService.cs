using Microsoft.EntityFrameworkCore;
using PurchaseOrders.Application.Domain.Payments.Enums;
using PurchaseOrders.Application.Domain.PurchaseOrders;
using PurchaseOrders.Application.Domain.PurchaseOrders.Enums;
using PurchaseOrders.Data;

namespace PurchaseOrders.Application.Services.Financial
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _context;
        public FinancialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal GetPurchaseOrderTotal(PurchaseOrder purchaseOrder)
        {
            //decimal currencyFactor = purchaseOrder.CurrencyFactor;
            decimal discountPercent = purchaseOrder.DiscountPercent;
            decimal total = 0;

            foreach (var item in purchaseOrder.Items)
            {
                total += (item.Quantity * item.UnitPrice) * (item.DiscountPercent ?? discountPercent);
            }

            return total;
        }

        public decimal GetPurchaseOrderRetentionAmount(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.RetentionPercent == 0)
                return 0;

            decimal total = GetPurchaseOrderTotal(purchaseOrder);
            return total * purchaseOrder.RetentionPercent;
        }

        public async Task<decimal> GetPaidPaymentsTotal(Guid purchaseOrderId)
        {
            return await _context.PurchaseOrderPayments
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .SumAsync(x => (x.Amount - x.AdvanceDeduction));
        }

        public async Task<decimal?> GetAdvancePaymentAmount(Guid purchaseOrderId)
        {
            return await _context.PurchaseOrderPayments
                .Where(x => x.PurchaseOrderId == purchaseOrderId && x.Type == PaymentType.Advance)
                .Select(x => (decimal?)x.Amount)
                .SingleOrDefaultAsync();
        }

        public async Task<decimal> GetAdvancePaymentDeductionsAmount(Guid purchaseOrderId)
        {
            return await _context.PurchaseOrderPayments
                .Where(x => x.PurchaseOrderId == purchaseOrderId && x.Type == PaymentType.Regular)
                .SumAsync(x => x.AdvanceDeduction);
        }

        public async Task<decimal> GetOriginalRegularPaymentsAmount(Guid purchaseOrderId)
        {
            return await _context.PurchaseOrderPayments
                .Where(x => x.PurchaseOrderId == purchaseOrderId && x.Type == PaymentType.Regular)
                .SumAsync(x => x.Amount);
        }

        public async Task<decimal?> GetApprovedStatementsTotalAmount(Guid purchaseOrderId)
        {
            var total = await
                _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrderId && x.State == PurchaseOrderStatementState.Approved)
                .SelectMany(x => x.Items, (s, si) => new { s, si })
                .Join(_context.PurchaseOrderItems,
                    c1 => c1.si.PurchaseOrderItemId,
                    c2 => c2.Id,
                    (c1, c2) => new { c1.s, c1.si, poi = c2 }
                )
                .Join(_context.PurchaseOrders,
                c1 => c1.poi.PurchaseOrderId,
                c2 => c2.Id,
                (c1, c2) => new { c1.s, c1.si, c1.poi, po = c2 })
                .SumAsync(x => x.si.Quantity * x.poi.UnitPrice * (1 - (x.poi.DiscountPercent ?? x.po.DiscountPercent)));

            var violations = await
                _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrderId && x.State == PurchaseOrderStatementState.Approved)
                .Join(_context.PurchaseOrderViolationDiscounts,
                c1 => c1.Id,
                c2 => c2.PurchaseOrderStatementId,
                (c1, c2) => c2.Amount)
                .SumAsync();

            return total - violations;
        }


        public async Task<decimal> GetViolationsTotal(PurchaseOrder purchaseOrder)
        {
            //var currencyFactor = purchaseOrder.CurrencyFactor;

            var statementsIds = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrder.Id)
                .Select(x => x.Id)
                .ToListAsync();

            var violationsOriginalCurrency = await _context.PurchaseOrderViolationDiscounts
                .Where(x => statementsIds.Contains(x.PurchaseOrderStatementId))
                .SumAsync(x => x.Amount);

            return violationsOriginalCurrency;
        }
    }
}
