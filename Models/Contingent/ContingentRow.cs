namespace DepartmentLoadApp.Models.Contingent
{
    public class ContingentRow
    {
        public int Id { get; set; }

        public string DirectionCode { get; set; } = string.Empty;
        public bool IsBachelor { get; set; }
        public bool IsMaster { get; set; }

        // СТУДЕНТЫ
        public int Course1Count { get; set; }
        public int Course2Count { get; set; }
        public int Course3Count { get; set; }
        public int Course4Count { get; set; }

        // ГРУППЫ
        public int Course1Groups { get; set; }
        public int Course2Groups { get; set; }
        public int Course3Groups { get; set; }
        public int Course4Groups { get; set; }

        // ПОДГРУППЫ
        public int Course1Subgroups { get; set; }
        public int Course2Subgroups { get; set; }
        public int Course3Subgroups { get; set; }
        public int Course4Subgroups { get; set; }

        public int TotalCount { get; set; }
    }
}