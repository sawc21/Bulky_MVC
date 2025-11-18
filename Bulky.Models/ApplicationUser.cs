using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? StreetAddress { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
        public string? State { get; set; } = string.Empty;
        public string? PostalCode { get; set; } = string.Empty;
        public int? CompanyId { get; set; }



        [ForeignKey("CompanyId")]
        [ValidateNever]
        public Company? Company { get; set; }
        

        [NotMapped]
        public string? Role {  get; set; }
    }
}
