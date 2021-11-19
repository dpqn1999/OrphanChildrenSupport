using System;
using System.ComponentModel.DataAnnotations;

namespace OrphanChildrenSupport.Services.Models
{
    public class PersonalProfile : EngineEntity
    {
        public string AccountName { get; set; }
        public string FullName { get; set; }
        public bool Gender { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }
    }
}
