using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Payments;
    using global::RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IPaymentService
    {

        Task<PaymentResponse> CreateAsync(int orderId, CreatePaymentRequest request);

        Task<PaymentResponse?> GetByIdAsync(int paymentId);

        Task<List<PaymentResponse>> GetAllByOrderAsync(int orderId);


        Task<PaymentResponse?> UpdateStatusAsync(int paymentId, UpdatePaymentStatusRequest request);
    }
}
