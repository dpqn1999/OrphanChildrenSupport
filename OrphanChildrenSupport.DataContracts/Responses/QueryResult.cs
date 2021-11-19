using System.Collections.Generic;

namespace OrphanChildrenSupport.DataContracts
{
    public class QueryResult<T>
    {
        public long TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
