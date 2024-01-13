using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnionBase.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace OnionBase.Presentation.ViewModels
{
    public class AddProductViewModel
    {
        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Açıklama gereklidir.")]
        public string ProductDescription { get; set; }


        [Required(ErrorMessage = "Stok miktarı gereklidir.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        public int Price { get; set; }

        public Guid? ProductId { get; set; }

        public int ProductCode { get; set; }  // Otomatik oluşturulan ürün kodu

        //[Required(ErrorMessage = "Kategori gereklidir.")]
        public int SelectedCategoryId { get; set; }

        public List<SelectListItem>? Categories { get; set; }
        public List<String>? Bedenler { get; set; }
        public List<String>? Renkler { get; set; }
        public List<Sizes>? Sizes { get; set; }


        [BindProperty]
        public IFormFile Image { get; set; }


        public string ProductDescriptionColor { get; set; }
        public string ProductDescriptionFont { get; set; }
        public string ProductDescriptionFontSize { get; set; }


        // Dinamik olarak eklenen öznitelik değerleri için bir sözlük yapısı
        // Bu yapı, her bir öznitelik için değer listesi sağlar.
        public Dictionary<Guid, List<string>> DynamicAttributeValues { get; set; } = new Dictionary<Guid, List<string>>();
    }
}
