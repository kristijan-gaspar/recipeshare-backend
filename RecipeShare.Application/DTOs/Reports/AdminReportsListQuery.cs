using RecipeShare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeShare.Application.DTOs.Reports
{
    public class AdminReportsListQuery
    {
        public ReportStatus? ReportStatus { get; set; }
       
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; } = 1;
       
        [Range(1, 20, ErrorMessage = "Page size must be between 1 and 20.")]
        public int PageSize { get; set; } = 20;
    }
}
