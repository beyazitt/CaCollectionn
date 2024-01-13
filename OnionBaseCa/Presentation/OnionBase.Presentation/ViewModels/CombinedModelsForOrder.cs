using OnionBase.Presentation.DTOs;

namespace OnionBase.Presentation.ViewModels
{
    public class CombinedModelsForOrder
    {
        public OrderViewModelcs Model1 { get; set; }
        public ProductCodeDTO Model2 { get; set; }
        public Double price { get; set; }
        public string? AdditionalFeature { get; set; }
    }
}
