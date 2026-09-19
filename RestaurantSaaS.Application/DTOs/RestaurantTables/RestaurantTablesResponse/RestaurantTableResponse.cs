using System;

namespace RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesResponse;

// BranchId removed: RestaurantTable is always fetched within an
// already-known Branch context (route-scoped). QRCodeToken is kept - it is
// genuinely needed by the frontend to render/print the table's QR code.
public class RestaurantTableResponse
{
    public int TableId { get; set; }

    public string TableNumber { get; set; } = null!;

    public int Capacity { get; set; }

    public Guid QRCodeToken { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}