using OrphanChildrenSupport.DataContracts.Resources;

namespace OrphanChildrenSupport.Infrastructure.Repositories.Specifications
{
    public class PagingSpecification
    {

        public PagingSpecification(QueryResource queryObj)
        {
            if (queryObj.Page == null || queryObj.Page == 0)
            {
                this.Skip = 0;
                if (queryObj.PageSize <= 0)
                {
                    this.IsTakeAll = true;
                }
                else
                {
                    this.Take = queryObj.PageSize;
                }
            }
            else
            {
                if (queryObj.PageSize <= 0)
                {
                    this.Take = 10;
                }
                else
                {
                    this.Take = queryObj.PageSize;
                }

                if (queryObj.Page <= 0)
                {
                    this.Skip = 0;
                }
                else
                {
                    this.Skip = (queryObj.Page.Value - 1) * this.Take;
                }
            }
        }

        public PagingSpecification(int skip, int take)
        {
            this.Skip = skip < 0 ? 0 : skip;
            this.Take = take < 0 ? 10 : take;
        }

        public int Skip { get; }
        public int Take { get; }
        public bool IsTakeAll { get; set; }
    }
}
