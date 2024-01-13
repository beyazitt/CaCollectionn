using Microsoft.AspNetCore.Mvc;
using OnionBase.Domain.Entities;

namespace OnionBase.Presentation.ViewModels
{
    public class AddProductStep3ViewModel
    {
        public int? Stock { get; set; }
        public List<ProductVariant>? productVariants  { get; set; }
        public Guid? ProductId { get; set; }

        [BindProperty]
        public IFormFile Image { get; set; }

    }
}
