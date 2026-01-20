using PurchaseOrders.Application.Dtos.Setup;

namespace PurchaseOrders.Application.Services.Setup
{
    public interface ILookupService
    {
        Task<List<KeyValueDto>> GetCurrenciesLookup();
        Task<List<KeyValueDto>> GetItemsLookup();
        Task<List<KeyValueDto>> GetProjectsLookup();
        Task<List<KeyValueDto>> GetSuppliersLookup();
        Task<List<KeyValueDto>> GetUnitsLookup(Guid itemId);
    }
}