namespace DepartmentLoadApp.Models.Contingent
{
    public class ContingentDirectionRowViewModel
    {
        public string DirectionCode { get; set; } = string.Empty;

        public int Course1Count { get; set; }
        public int Course2Count { get; set; }
        public int Course3Count { get; set; }
        public int Course4Count { get; set; }
        public bool IsBachelor { get; set; }
        public bool IsMaster { get; set; }
    }
}