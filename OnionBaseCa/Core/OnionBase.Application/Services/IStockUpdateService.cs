using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Application.Services
{
    public interface IStockUpdateService
    {
        Task<bool> UpdateTotalStock(Guid productId);
        Task<bool> AddStock(Guid productId, Guid productVariantId, int quantity);
        Task<bool> RemoveStock(Guid productId, Guid productVariantId, int quantity);
        Task<bool> DeleteAllStock(Guid productId, Guid productVariantId);


    }
}
