using Microsoft.EntityFrameworkCore;
using PurchaseOrders.Application.Common.Exceptions;
using PurchaseOrders.Application.Domain.Payments;
using PurchaseOrders.Application.Domain.Payments.Enums;
using PurchaseOrders.Application.Domain.PurchaseOrders.Enums;
using PurchaseOrders.Application.Dtos.Payments;
using PurchaseOrders.Application.Services.Financial;
using PurchaseOrders.Data;

namespace PurchaseOrders.Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinancialService _financialService;
        public PaymentService(ApplicationDbContext context, IFinancialService financialService)
        {
            _context = context;
            _financialService = financialService;
        }

        public async Task<bool> CanInsertPayment(Guid statementId)
        {
            var purchaseOrderId = await GetPurchaseOrderIdByStatementId(statementId);

            if (!(await IsLastApprovedStatement(statementId, purchaseOrderId)))
                return false;

            var total = await _financialService.GetApprovedStatementsTotalAmount(purchaseOrderId);
            if (!total.HasValue)
                return false;

            var payments = await _financialService.GetOriginalRegularPaymentsAmount(purchaseOrderId);

            return total.Value - payments > 0.1m;
        }

        public async Task InsertPayment(RegularPaymentCreationDto paymentDto)
        {
            if (paymentDto.AdvanceDeduction >= paymentDto.Amount)
            {
                throw new BusinessException("Payment amount should be greater than Advance deduction.");
            }

            var purchaseOrderId = await GetPurchaseOrderIdByStatementId(paymentDto.PurchaseOrderStatementId);

            var purchaseOrder = await _context
                .PurchaseOrders.SingleOrDefaultAsync(x => x.Id == purchaseOrderId);

            if (purchaseOrder == null)
                throw new NotFoundException("purchase order not found.");

            var allowedDetails = await GetInsertPaymentDetails(paymentDto.PurchaseOrderStatementId, purchaseOrderId);

            if (paymentDto.Amount <= 0)
            {
                throw new BusinessException("Payment amount should be greater than 0.");
            }

            if (paymentDto.Amount - allowedDetails.AllowedPayment > 0.01m)
            {
                throw new BusinessException("Payment amount exceeds allowed payment.");
            }

            if (paymentDto.AdvanceDeduction - allowedDetails.AllowedAdvanceDeduction > 0.01m)
            {
                throw new BusinessException("Advance Payment deduction amount exceeds allowed.");
            }

            var payment = new PurchaseOrderPayment
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Type = PaymentType.Regular,
                Amount = paymentDto.Amount,
                AdvanceDeduction = paymentDto.AdvanceDeduction,
                CurrencyId = purchaseOrder.CurrencyId,
                CurrencyFactor = purchaseOrder.CurrencyFactor,
                PurchaseOrderId = purchaseOrderId,
                PurchaseOrderStatementId = paymentDto.PurchaseOrderStatementId
            };

            _context.PurchaseOrderPayments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<InsertPaymentGetDetailsDto> GetInsertPaymentDetails(Guid statementId)
        {
            var purchaseOrderId = await GetPurchaseOrderIdByStatementId(statementId);
            return await GetInsertPaymentDetails(statementId, purchaseOrderId);

        }

        public async Task<InsertPaymentGetDetailsDto> GetInsertPaymentDetails(Guid statementId, Guid purchaseOrderId)
        {
            var total = await _financialService.GetApprovedStatementsTotalAmount(purchaseOrderId);
            if (!total.HasValue)
                throw new BusinessException("approved statements amount is zero");

            var payments = await _financialService.GetOriginalRegularPaymentsAmount(purchaseOrderId);

            var advancePayment = await _financialService.GetAdvancePaymentAmount(purchaseOrderId);
            var advanceDeductions = await _financialService.GetAdvancePaymentDeductionsAmount(purchaseOrderId);

            return new InsertPaymentGetDetailsDto
            {
                AllowedPayment = total.Value - payments,
                AllowedAdvanceDeduction = advancePayment.HasValue ? advancePayment.Value - advanceDeductions : 0,
            };
        }

        //-----------------------------------------------
        //Private helper methods
        //-----------------------------------------------
        private async Task<bool> IsLastApprovedStatement(Guid statementId, Guid purchaseOrderId)
        {
            var lastApprovedStatement = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrderId && x.State == PurchaseOrderStatementState.Approved)
                .OrderByDescending(x => x.Number)
                .SingleOrDefaultAsync();

            if (lastApprovedStatement == null)
                return false;

            if (!lastApprovedStatement.Id.Equals(statementId))
                return false;

            return true;
        }

        private async Task<Guid> GetPurchaseOrderIdByStatementId(Guid statementId)
        {
            return await _context.PurchaseOrderStatements
                .Where(x => x.Id == statementId)
                .Select(x => x.PurchaseOrderId)
                .SingleAsync();
        }
    }
}
