using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserResponse;

public class OrganizationUserResponse
{
    public int OrganizationUserId { get; set; }

    public int OrganizationId { get; set; }

    public int UserId { get; set; }

    public string UserFullName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public int? BranchId { get; set; }

    public string? BranchName { get; set; }

    public bool IsActive { get; set; }

    public DateTime JoinedAtUtc { get; set; }

    public DateTime? RemovedAtUtc { get; set; }
}