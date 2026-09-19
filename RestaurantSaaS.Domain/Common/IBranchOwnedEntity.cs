using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Domain.Common
{
    /// <summary>
    /// كيان مملوك لفرع مباشرة (BranchId NOT NULL) بلا OrganizationId خاص به.
    /// يشترط وجود navigation property باسم "Branch" بالضبط على الكيان.
    /// </summary>
    public interface IBranchOwnedEntity
    {
        int BranchId { get; }
    }
}
