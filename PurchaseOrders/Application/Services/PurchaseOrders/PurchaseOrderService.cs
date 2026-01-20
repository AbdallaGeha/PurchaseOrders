using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PurchaseOrders.Application.Common.Exceptions;
using PurchaseOrders.Application.Domain.Payments;
using PurchaseOrders.Application.Domain.Payments.Enums;
using PurchaseOrders.Application.Domain.PurchaseOrders;
using PurchaseOrders.Application.Domain.Setup;
using PurchaseOrders.Application.Dtos.PurchaseOrders;
using PurchaseOrders.Application.Services.Financial;
using PurchaseOrders.Data;

namespace PurchaseOrders.Application.Services.PurchaseOrders
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinancialService _financialService;
        private readonly IMapper _mapper;
        public PurchaseOrderService(ApplicationDbContext context, IFinancialService financialService, IMapper mapper)
        {
            _context = context;
            _financialService = financialService;
            _mapper = mapper;
        }
        public async Task AddPurchaseOrderAsync(PurchaseOrderCreationDto purchaseOrderDto)
        {
            var purchaseOrder = _mapper.Map<PurchaseOrder>(purchaseOrderDto);
            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanUpdateAsync(Guid id)
        {
            var purchaseOrder = await GetPurchaseOrderWithItemsAsync(id);
            if (purchaseOrder.State == Domain.PurchaseOrders.Enums.PurchaseOrderState.Created)
                return true;

            return false;
        }
        public async Task UpdatePurchaseOrderAsync(Guid id, PurchaseOrderUpdateDto purchaseOrderDto)
        {
            var noDuplication = purchaseOrderDto.Items
                .GroupBy(x => new { x.ItemId, x.UnitId })
                .All(g => g.Count() == 1);

            if (!noDuplication)
                throw new BusinessException("duplicate item and unit in more than one row"); 

            var purchaseOrder = await GetPurchaseOrderWithItemsAsync(id);
            _mapper.Map(purchaseOrderDto, purchaseOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanApproveAsync(Guid id)
        {
            var purchaseOrder = await GetPurchaseOrderWithItemsAsync(id);
            if (purchaseOrder.State != Domain.PurchaseOrders.Enums.PurchaseOrderState.Created)
                return false;

            return true;
        }

        public async Task ApproveAsync(Guid id)
        {
            var purchaseOrder = await GetPurchaseOrderWithItemsAsync(id);
            if (purchaseOrder.State != Domain.PurchaseOrders.Enums.PurchaseOrderState.Created)
                throw new BusinessException("Purchase Order should be in created state before approval");

            purchaseOrder.State = Domain.PurchaseOrders.Enums.PurchaseOrderState.Approved;

            var payment = new PurchaseOrderPayment
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Type = PaymentType.Advance,
                Amount = purchaseOrder.AdvancePayment,
                AdvanceDeduction = 0,
                CurrencyId = purchaseOrder.CurrencyId,
                CurrencyFactor = purchaseOrder.CurrencyFactor,
                PurchaseOrderId = purchaseOrder.Id,
                PurchaseOrderStatementId = null
            };

            _context.PurchaseOrderPayments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task CloseAsync(Guid id)
        {
            var purchaseOrder = await GetPurchaseOrderWithItemsAsync(id);
            if (purchaseOrder.State != Domain.PurchaseOrders.Enums.PurchaseOrderState.Approved)
                throw new BusinessException("Purchase Order should be in Approved state before close");

            var orderTotal = _financialService.GetPurchaseOrderTotal(purchaseOrder);
            var paidTotal = await _financialService.GetPaidPaymentsTotal(purchaseOrder.Id);
            var violationTotal = await _financialService.GetViolationsTotal(purchaseOrder);
            var retentionAmount = _financialService.GetPurchaseOrderRetentionAmount(purchaseOrder);

            if (orderTotal != paidTotal + violationTotal + retentionAmount)
                throw new BusinessException("Purchase Order is not ready to close");

            purchaseOrder.State = Domain.PurchaseOrders.Enums.PurchaseOrderState.Closed;
            
            var retentionPayment = new PurchaseOrderPayment
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Type = PaymentType.Retention,
                Amount = retentionAmount,
                AdvanceDeduction = 0,
                CurrencyId = purchaseOrder.CurrencyId,
                CurrencyFactor = purchaseOrder.CurrencyFactor,
                PurchaseOrderId = purchaseOrder.Id,
                PurchaseOrderStatementId = null
            };

            _context.PurchaseOrderPayments.Add(retentionPayment);

            await _context.SaveChangesAsync();
        }

        public async Task<PurchaseOrderUpdateGetDto> GetPurchaseOrderWithItemsByIdAsync(Guid id)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(x => x.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id);

            if (purchaseOrder == null)
                throw new NotFoundException("purchase order not found.");

            return _mapper.Map<PurchaseOrderUpdateGetDto>(purchaseOrder);
        }

        public async Task<int> GetNumber(int year)
        {
            var lastNumber = await _context.PurchaseOrders
                .Where(x => x.Date.Year == year)
                .Select(x => (int?)x.Number)
                .MaxAsync();

            return (lastNumber ?? 0) + 1;
        }

        // -----------------------------
        // Private helper methods
        // -----------------------------
        private async Task<PurchaseOrder> GetPurchaseOrderWithItemsAsync(Guid id)
        {
            var PurchaseOrder = await _context.PurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (PurchaseOrder == null)
                throw new NotFoundException("Purchase order not found.");

            return PurchaseOrder;
        }

    }
}
