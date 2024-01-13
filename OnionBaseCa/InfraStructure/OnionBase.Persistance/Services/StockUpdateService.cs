using Microsoft.EntityFrameworkCore;
using OnionBase.Application.Repositories;
using OnionBase.Application.Services;
using OnionBase.Domain.Entities;
using OnionBase.Persistance.Contexts;
using OnionBase.Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionBase.Persistance.Services
{
    public class StockUpdateService : IStockUpdateService
    {
        readonly IProductReadRepository _productReadRepository;
        readonly IProductWriteRepository _productWriteRepository;
        readonly UserDbContext _dbContext;

        public StockUpdateService(IProductReadRepository productReadRepository, IProductWriteRepository productWriteRepository, UserDbContext dbContext)
        {
            _productReadRepository = productReadRepository;
            _productWriteRepository = productWriteRepository;
            _dbContext = dbContext;
        }



        public async Task<bool> AddStock(Guid productId, Guid productVariantId, int quantity)
        {
            
            try
            {
                Product product = _productReadRepository.GetWhere(a => a.ProductId == productId).FirstOrDefault();
                ProductVariant productVariant = _dbContext.ProductVariants.Where(a => a.ProductVariantId == productVariantId).FirstOrDefault();
                productVariant.Stock += quantity;
                _dbContext.Update(productVariant);
                _dbContext.SaveChanges();
                UpdateTotalStock(productId);
                return  true;
            }
            catch (Exception)
            {
                // Hata durumunda false döndür
                return false;
            }

        }

        public async Task<bool> RemoveStock(Guid productId, Guid productVariantId, int quantity)
        {
            
            try
            {
                Product product = _productReadRepository.GetWhere(a => a.ProductId == productId).FirstOrDefault();
                ProductVariant productVariant = _dbContext.ProductVariants.Where(a => a.ProductVariantId == productVariantId).FirstOrDefault();
                productVariant.Stock -= quantity;
                _dbContext.Update(productVariant);
                _dbContext.SaveChanges();
                UpdateTotalStock(productId);

                return true;
            }
            catch (Exception)
            {
                // Hata durumunda false döndür
                return false;
            }

        }

        public async Task<bool> DeleteAllStock(Guid productId, Guid productVariantId)
        {
            
            try
            {
                Product product = _productReadRepository.GetWhere(a => a.ProductId == productId).FirstOrDefault();
                ProductVariant productVariant = _dbContext.ProductVariants.Where(a => a.ProductVariantId == productVariantId).FirstOrDefault();
                _dbContext.Remove(productVariant);
                UpdateTotalStock(productId);
                _productWriteRepository.Update(product);
                await _productWriteRepository.SaveAsync();
                UpdateTotalStock(productId);

                return true;
            }
            catch (Exception)
            {
                // Hata durumunda false döndür
                return false;
            }
        }

        public async Task<bool> UpdateTotalStock(Guid productId)
        {
            
            try
            {
                var liste = _dbContext.ProductVariants.Where(a => a.ProductId == productId).ToList();
                int stock = 0;
                foreach (var her in liste)
                {
                    stock += her.Stock;
                };
                Product product = _productReadRepository.GetWhere(x => x.ProductId == productId).FirstOrDefault();
                product.TotalStock = stock;
                _productWriteRepository.Update(product);
                _dbContext.SaveChanges();
                return  true;
            }
            catch (Exception)
            {
                // Hata durumunda false döndür
                return false;
            }
        }



    }
}
