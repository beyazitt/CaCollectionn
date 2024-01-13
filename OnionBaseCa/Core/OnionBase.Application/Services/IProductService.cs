using OnionBase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Application.Services
{
    public interface IProductService
    {
        Task<byte[]> QrCodeToProductAsync(Guid productId);
    }
}
