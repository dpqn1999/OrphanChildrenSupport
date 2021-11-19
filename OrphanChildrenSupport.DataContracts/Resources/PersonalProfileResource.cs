using System;
using OrphanChildrenSupport.Services.Models;

namespace OrphanChildrenSupport.DataContracts
{
    public class PersonalProfileResource : EngineEntity
    {
        public string AccountName { get; set; }
        public string FullName { get; set; }
        public bool Gender { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
}
