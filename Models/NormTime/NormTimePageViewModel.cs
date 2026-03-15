using System.Collections.Generic;

namespace DepartmentLoadApp.Models.NormTime
{
    public class NormTimePageViewModel
    {
        public List<NormTimeRowViewModel> Rows { get; set; } = new();
    }
}