using OnionBase.Application.Repositories;
using OnionBase.Application.Services;
using OnionBase.Domain.Entities;
using OnionBase.Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnionBase.Persistance.Services
{
    public class ProductService : IProductService
    {
        readonly IProductReadRepository _productReadRepository;
        readonly IQRCodeService _qrCodeService;

        public ProductService(IProductReadRepository productReadRepository, IQRCodeService qrCodeService)
        {
            _productReadRepository = productReadRepository;
            _qrCodeService = qrCodeService;
        }

        public async Task<byte[]> QrCodeToProductAsync(Guid productId)
        {
            Product product = _productReadRepository.GetWhere(a => a.ProductId == productId).FirstOrDefault();
            if(product == null)
            {
                throw new Exception("Product not found");
            }
            var plainObject = new
            {
                product.Id,
                product.ProductName,
                product.ProductDescription,
                product.Price,
                product.TotalStock,
                product.Image
            };
            string plainText = JsonSerializer.Serialize(plainObject);
            return _qrCodeService.GenerateQrCode(plainText);
        }
    }
}



