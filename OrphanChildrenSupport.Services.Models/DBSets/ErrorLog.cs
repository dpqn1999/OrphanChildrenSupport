using System;

namespace OrphanChildrenSupport.Services.Models
{
    public class ErrorLog : EngineEntity
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
    }
}
