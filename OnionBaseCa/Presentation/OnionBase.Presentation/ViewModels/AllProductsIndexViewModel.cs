using OnionBase.Domain.Entities;

namespace OnionBase.Presentation.ViewModels
{
    public class AllProductsIndexViewModel
    {
        public List<Product>? Products { get; set; }
        public List<ProductVariant>? ProductVariants { get; set; }
        public bool? fromVariant { get; set; }
        public bool? forColor { get; set; }
        public bool? forSize { get; set;}
        public string? selectedColor { get; set; }
        public string? selectedSize { get; set; }
    }
}
