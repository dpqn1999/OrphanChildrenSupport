using System.Collections.Generic;

namespace OrphanChildrenSupport.DataContracts.Resources
{
    public class QueryResultResource<T>
    {
        public long TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}