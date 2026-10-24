using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public static class PaymentRules
    {
        public const string Cash = "Cash";
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";

        public static readonly string[] AllowedMethods =
            { "Cash", "Card", "Online", "Other", "ShamCash", "SyriatelCash", "MTNCash" };

        public static readonly string[] AllowedStatuses = { Pending, Completed, Failed, Refunded };

        public const string AmountMustBePositiveMessage = "Payment amount must be greater than 0.";
        public const string AmountDecimalsMessage = "Payment amount cannot have more than 2 decimal places.";
        public const string ReferenceRequiredMessage = "A transaction reference is required for this payment method.";
        public const string ReferenceDuplicateMessage = "A payment with this transaction reference already exists for this payment method.";
        public const string OrderCanceledMessage = "Payments cannot be recorded on a Canceled order.";

        public static string ExceedsBalanceMessage(decimal remaining) =>
            $"The payment exceeds the remaining balance of {remaining.ToString("F2", CultureInfo.InvariantCulture)}.";

        public static string TransitionMessage(string from, string to) =>
            $"A {from} payment cannot be changed to {to}.";

        public static bool CanMove(string from, string to) =>
            (from == Pending && (to == Completed || to == Failed)) || (from == Completed && to == Refunded);
    }
}
