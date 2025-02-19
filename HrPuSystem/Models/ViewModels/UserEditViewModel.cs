using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrPuSystem.Models.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        public IList<string> CurrentRoles { get; set; } = new List<string>();
        public IList<string> AllRoles { get; set; } = new List<string>();
        public IList<string> SelectedRoles { get; set; } = new List<string>();
    }
}