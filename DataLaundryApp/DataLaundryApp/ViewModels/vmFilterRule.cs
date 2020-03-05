using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DataLaundryApp.ViewModels
{
    public class vmFilterRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public bool IsEnable { get; set; }

        [Required]
        [Display(Name = "Field Name")]
        public int? FieldId { get; set; }
        public List<FilterCriteria> FilterCriteria { get; set; }
    }

    public class FilterCriteria
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public int OperatorId { get; set; }
        public string OperatorName { get; set; }
        public string Value { get; set; }
    }
}