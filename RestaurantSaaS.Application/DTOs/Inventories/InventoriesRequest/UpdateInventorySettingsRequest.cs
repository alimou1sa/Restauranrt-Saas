using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Inventories.InventoriesRequest
{


    // Intentionally named "Settings" and not "UpdateInventoryRequest": this
    // only covers the reorder threshold. Quantity is deliberately excluded -
    // stock levels change exclusively through InventoryTransaction records
    // to preserve an accurate audit trail.
    public class UpdateInventorySettingsRequest
    {
        [Range(0, double.MaxValue)]
        public decimal ReorderLevel { get; set; }
    }
}
