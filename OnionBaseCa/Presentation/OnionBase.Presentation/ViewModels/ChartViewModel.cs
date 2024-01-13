using OnionBase.Domain.Entities;

namespace OnionBase.Presentation.ViewModels
{
    public class ChartViewModel
    {
        public Guid? ProductId { get; set; }
        public ChartViewModel()
        {
            // Constructor içinde Date ve Views listelerini başlat
            Date = new List<DateTime>();
            Views = new List<int>();
        }
        public List<DateTime> Date { get; set; }
        public List<int> Views { get; set; }
    }
}
