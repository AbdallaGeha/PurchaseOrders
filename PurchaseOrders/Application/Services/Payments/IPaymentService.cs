using PurchaseOrders.Application.Dtos.Payments;

namespace PurchaseOrders.Application.Services.Payments
{
    /// <summary>
    /// Defines business operations related to payments against
    /// purchase order statements.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Determines whether a payment can be inserted
        /// for the specified statement.
        /// </summary>
        Task<bool> CanInsertPayment(Guid statementId);

        /// <summary>
        /// Gets the data required to insert a payment
        /// for the specified statement considering previous statements and payments
        /// </summary>
        Task<InsertPaymentGetDetailsDto> GetInsertPaymentDetails(Guid statementId);

        /// <summary>
        /// Gets the data required to insert a payment
        /// for the specified statement and purchase order.
        /// </summary>
        Task<InsertPaymentGetDetailsDto> GetInsertPaymentDetails(Guid statementId, Guid purchaseOrderId);

        /// <summary>
        /// Inserts a new payment, considering accumulative previous statements amount
        /// and previous payments
        /// </summary>
        Task InsertPayment(RegularPaymentCreationDto paymentDto);
    }
}