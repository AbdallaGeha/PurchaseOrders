using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PurchaseOrders.Application.Common.Exceptions;
using PurchaseOrders.Application.Domain.Inventory;
using PurchaseOrders.Application.Domain.Inventory.Enums;
using PurchaseOrders.Application.Domain.PurchaseOrders;
using PurchaseOrders.Application.Domain.PurchaseOrders.Enums;
using PurchaseOrders.Application.Dtos.PurchaseOrders;
using PurchaseOrders.Application.Dtos.PurchaseOrderStatements;
using PurchaseOrders.Application.Services.Financial;
using PurchaseOrders.Data;

namespace PurchaseOrders.Application.Services.PurchaseOrderStatements
{
    public class PurchaseOrderStatementService : IPurchaseOrderStatementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public PurchaseOrderStatementService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CanGetNewStatement(Guid purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == purchaseOrderId);

            if (purchaseOrder == null || purchaseOrder.State != Domain.PurchaseOrders.Enums.PurchaseOrderState.Approved)
                return false;

            var statementsIds = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrderId).Select(x => x.Id).ToListAsync();

            var accumQuantitesListPerOrderItem = await _context.PurchaseOrderStatementItems
                .Where(x => statementsIds.Contains(x.PurchaseOrderStatementId))
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new
                {
                    PurchaseOrderItemId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            if (purchaseOrder.Items.Count != accumQuantitesListPerOrderItem.Count)
                return true;

            foreach (var item in purchaseOrder.Items)
            {
                var accumItem = accumQuantitesListPerOrderItem.Single(x => x.PurchaseOrderItemId == item.Id);
                if (accumItem.TotalQuantity != item.Quantity)
                    return true;
            }

            return false;
        }
        public async Task<NewStatementDto> GetNewStatement(Guid purchaseOrderId)
        {
            var newStatementDto = await _context.PurchaseOrders
                .Where(x => x.Id == purchaseOrderId)
                .Select(x => new NewStatementDto
                {
                    StatementNumber = 0,
                    PoNumber = x.Number,
                    PoDate = x.Date.ToString("dd MMM yyyy"),
                    PoReference = x.Ref,
                    Project = x.Project.Name,
                    Supplier = x.Supplier.Name,
                    Currency = x.Currency.Name,
                    CurrencyFactor = x.CurrencyFactor,
                    Items = x.Items
                        .OrderBy(i => i.LineNo)
                        .Select(i => new NewStatementItemDto
                        {
                            PurchaseOrderItemId = i.Id,
                            LineNo = i.LineNo,
                            Item = i.Item.Name,
                            Unit = i.Unit.Name,
                            QuantityPo = i.Quantity,
                            QuantityAccum = 0,
                            UnitPrice = i.UnitPrice,
                            DiscountPercent = i.DiscountPercent ?? x.DiscountPercent,
                            AmountPo = i.Quantity * i.UnitPrice * (i.DiscountPercent ?? x.DiscountPercent),
                            AmountAccum = 0
                        })
                        .ToList()
                })
                .SingleOrDefaultAsync();

            if (newStatementDto == null)
                throw new NotFoundException("Purchase order not found.");

            var maxStatementNumber = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .Select(x => (int?)x.Number)
                .MaxAsync();

            newStatementDto.StatementNumber = (maxStatementNumber ?? 0) + 1;

            var statementsIds = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrderId).Select(x => x.Id).ToListAsync();

            var accumQuantitesListPerOrderItem = await _context.PurchaseOrderStatementItems
                .Where(x => statementsIds.Contains(x.PurchaseOrderStatementId))
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new
                {
                    PurchaseOrderItemId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            foreach (var itemDto in newStatementDto.Items)
            {
                var accumRow = accumQuantitesListPerOrderItem
                    .SingleOrDefault(x => x.PurchaseOrderItemId == itemDto.PurchaseOrderItemId);

                if (accumRow != null)
                {
                    itemDto.QuantityAccum = accumRow.TotalQuantity;
                    itemDto.AmountAccum = itemDto.QuantityAccum * itemDto.UnitPrice * itemDto.DiscountPercent;
                }
            }

            return newStatementDto;
        }

        public async Task<StatementDto> GetStatementById(Guid statementId)
        {
            var statement = await _context.PurchaseOrderStatements
                .Include(x => x.Items)
                .Include(x => x.Violations)
                .SingleOrDefaultAsync(x => x.Id == statementId);

            if (statement == null)
                throw new NotFoundException("Purchase order statement not found.");

            var statementDto = await _context.PurchaseOrders
                    .Where(x => x.Id == statement.PurchaseOrderId)
                    .Select(x => new StatementDto
                    {
                        Id = statement.Id,
                        Number = statement.Number,
                        Date = statement.Date,
                        Ref = statement.Ref,
                        Remarks = statement.Remarks,
                        State = (short)statement.State,
                        PoNumber = x.Number,
                        PoDate = x.Date.ToString("dd MMM yyyy"),
                        PoReference = x.Ref,
                        Project = x.Project.Name,
                        Supplier = x.Supplier.Name,
                        Currency = x.Currency.Name,
                        CurrencyFactor = x.CurrencyFactor,
                        Items = x.Items
                            .OrderBy(i => i.LineNo)
                            .Select(i => new StatementItemDto
                            {
                                Id = null,
                                PurchaseOrderItemId = i.Id,
                                LineNo = i.LineNo,
                                Item = i.Item.Name,
                                Unit = i.Unit.Name,
                                QuantityCurrent = 0,
                                QuantityPo = i.Quantity,
                                QuantityAccum = 0,
                                UnitPrice = i.UnitPrice,
                                DiscountPercent = i.DiscountPercent ?? x.DiscountPercent,
                                AmountCurrent = 0,
                                AmountPo = i.Quantity * i.UnitPrice * ((1 - i.DiscountPercent) ?? (1 - x.DiscountPercent)),
                                AmountAccum = 0
                            })
                            .ToList(),
                        Violations = statement.Violations.Select(x => new StatementViolationDto
                        {
                            Id = x.Id,
                            Amount = x.Amount,
                            Reason = x.Reason
                        })
                            .ToList()
                    })
                    .SingleOrDefaultAsync();

            if (statementDto == null)
                throw new NotFoundException("Purchase order statement not found.");

            foreach (var statementItemDto in statementDto.Items)
            {
                var statementItem = statement
                    .Items
                    .SingleOrDefault(x => x.PurchaseOrderItemId == statementItemDto.PurchaseOrderItemId);

                if (statementItem != null)
                {
                    statementItemDto.Id = statementItem.Id;
                    statementItemDto.QuantityCurrent = statementItem.Quantity;
                    statementItemDto.AmountCurrent = statementItem.Quantity * statementItemDto.UnitPrice * (1 - statementItemDto.DiscountPercent);
                }
            }


            var statementsIds = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == statement.PurchaseOrderId && x.Number >= statement.Number)
                .Select(x => x.Id).ToListAsync();

            var accumQuantitesListPerOrderItem = await _context.PurchaseOrderStatementItems
                .Where(x => statementsIds.Contains(x.PurchaseOrderStatementId))
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new
                {
                    PurchaseOrderItemId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            foreach (var statementItemDto in statementDto.Items)
            {
                var accumRow = accumQuantitesListPerOrderItem
                    .SingleOrDefault(x => x.PurchaseOrderItemId == statementItemDto.PurchaseOrderItemId);

                if (accumRow != null)
                {
                    statementItemDto.QuantityAccum = accumRow.TotalQuantity;
                    statementItemDto.AmountAccum = statementItemDto.QuantityAccum * statementItemDto.UnitPrice * (1 - statementItemDto.DiscountPercent);
                }
            }

            return statementDto;
        }

        public async Task ApproveQuantity(Guid statementId)
        {
            var statement = await _context.PurchaseOrderStatements
                .SingleOrDefaultAsync(x => x.Id == statementId);

            if (statement == null)
                throw new NotFoundException("purchase order statement not found.");

            if (statement.State != PurchaseOrderStatementState.Created)
                throw new BusinessException("statement must be in created state before approve quantity");

            statement.State = PurchaseOrderStatementState.QuantityApproved;

            var storeId = await _context.PurchaseOrders
                .Where(x => x.Id == statement.PurchaseOrderId)
                .Select(x => (Guid?)x.Project.StoreId)
                .SingleOrDefaultAsync();

            if (storeId == null)
                throw new BusinessException("no store found");

            var forInventoryResult = await _context.PurchaseOrderStatements
                .Where(x => x.Id == statementId)
                .Select(x => new
                {
                    StatementId = x.Id,
                    Items = x.Items.Join(_context.PurchaseOrderItems,
                     si => si.PurchaseOrderItemId,
                     poi => poi.Id,
                     (si, poi) => new
                     {
                         StatementItemId = si.Id,
                         Quantity = si.Quantity,
                         ItemId = poi.ItemId,
                         UnitId = poi.UnitId
                     }
                    ).ToList()
                })
                .SingleAsync();

            foreach (var item in forInventoryResult.Items)
            {
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Kind = InventoryMovementKind.PoReceipt,
                    StoreId = storeId.Value,
                    ItemId = item.ItemId,
                    UnitId = item.UnitId,
                    Quantity = item.Quantity,
                    TransactionId = item.StatementItemId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task Approve(Guid statementId)
        {
            var statement = await _context.PurchaseOrderStatements
                .SingleOrDefaultAsync(x => x.Id == statementId);

            if (statement == null)
                throw new NotFoundException("purchase order statement not found.");

            if (statement.State != PurchaseOrderStatementState.QuantityApproved)
                throw new BusinessException("statement must be in Quantity approved state before approve");

            statement.State = PurchaseOrderStatementState.Approved;

            await _context.SaveChangesAsync();
        }
        public async Task AddStatement(StatementCreationDto statementDto)
        {
            if (!(await ValidateStatementCreation(statementDto)))
                throw new BusinessException("statement infos are not valid");

            var statement = _mapper.Map<PurchaseOrderStatement>(statementDto);

            _context.PurchaseOrderStatements.Add(statement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatement(Guid statementId, StatementUpdateDto statementDto)
        {
            var statement = await _context.PurchaseOrderStatements
                .Include(x => x.Items)
                .Include(x => x.Violations)
                .SingleOrDefaultAsync(x => x.Id == statementId);

            if (statement == null)
                throw new NotFoundException("purchase order statement not found.");

            if (!(await ValidateStatementUpdate(statement, statementDto)))
                throw new BusinessException("statement infos are not valid");

            _mapper.Map(statementDto, statement);

            await _context.SaveChangesAsync();
        }
        private async Task<bool> ValidateStatementCreation(StatementCreationDto statementDto)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == statementDto.PurchaseOrderId);

            if (purchaseOrder == null || purchaseOrder.State != Domain.PurchaseOrders.Enums.PurchaseOrderState.Approved)
                return false;

            decimal violationsAmount = statementDto.Violations.Sum(x => x.Amount);

            decimal statementAmount = 0;
            foreach (var statementItem in statementDto.Items)
            {
                var poItem = purchaseOrder.Items.Single(x => x.Id == statementItem.PurchaseOrderItemId);
                statementAmount += statementItem.Quantity * poItem.UnitPrice * ((1 - poItem.DiscountPercent) ?? (1 - purchaseOrder.DiscountPercent));
            }

            if (statementAmount == 0 || statementAmount < violationsAmount)
                return false;

            var statementsIds = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrder.Id)
                .Select(x => x.Id).ToListAsync();

            var accumQuantitesListPerOrderItem = await _context.PurchaseOrderStatementItems
                .Where(x => statementsIds.Contains(x.PurchaseOrderStatementId))
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new
                {
                    PurchaseOrderItemId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            foreach (var statementItem in statementDto.Items)
            {
                var poItem = purchaseOrder.Items.Single(x => x.Id == statementItem.PurchaseOrderItemId);
                var accumItem = accumQuantitesListPerOrderItem.SingleOrDefault(x => x.PurchaseOrderItemId == statementItem.PurchaseOrderItemId);
                decimal PoQuantity = poItem.Quantity;
                decimal accumQuantity = 0;
                if (accumItem != null)
                    accumQuantity = accumItem.TotalQuantity;

                if (statementItem.Quantity + accumQuantity > PoQuantity)
                    return false;
            }

            return true;
        }

        private async Task<bool> ValidateStatementUpdate(PurchaseOrderStatement statement, StatementUpdateDto statementDto)
        {
            if (statement.State != Domain.PurchaseOrders.Enums.PurchaseOrderStatementState.Created)
                return false;

            var purchaseOrder = await _context.PurchaseOrders
                .Include(x => x.Items)
                .SingleOrDefaultAsync(x => x.Id == statement.PurchaseOrderId);

            if (purchaseOrder == null)
                return false;

            decimal violationsAmount = statementDto.Violations.Sum(x => x.Amount);

            decimal statementAmount = 0;
            foreach (var statementItem in statementDto.Items)
            {
                var poItem = purchaseOrder.Items.Single(x => x.Id == statementItem.PurchaseOrderItemId);
                statementAmount += statementItem.Quantity * poItem.UnitPrice * ((1 - poItem.DiscountPercent) ?? (1 - purchaseOrder.DiscountPercent));
            }

            if (statementAmount == 0 || statementAmount < violationsAmount)
                return false;

            var statementsIds = await _context.PurchaseOrderStatements
                .Where(x => x.PurchaseOrderId == purchaseOrder.Id && x.Id != statement.Id)
                .Select(x => x.Id).ToListAsync();

            var accumQuantitesListPerOrderItem = await _context.PurchaseOrderStatementItems
                .Where(x => statementsIds.Contains(x.PurchaseOrderStatementId))
                .GroupBy(x => x.PurchaseOrderItemId)
                .Select(g => new
                {
                    PurchaseOrderItemId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            foreach (var statementItem in statementDto.Items)
            {
                var poItem = purchaseOrder.Items.Single(x => x.Id == statementItem.PurchaseOrderItemId);
                var accumItem = accumQuantitesListPerOrderItem.SingleOrDefault(x => x.PurchaseOrderItemId == statementItem.PurchaseOrderItemId);
                decimal PoQuantity = poItem.Quantity;
                decimal accumQuantity = 0;
                if (accumItem != null)
                    accumQuantity = accumItem.TotalQuantity;

                if (statementItem.Quantity + accumQuantity > PoQuantity)
                    return false;
            }

            return true;
        }
    }
}
