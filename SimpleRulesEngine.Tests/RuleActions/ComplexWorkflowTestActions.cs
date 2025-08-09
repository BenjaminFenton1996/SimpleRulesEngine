using SimpleRulesEngine.Interfaces;

namespace SimpleRulesEngine.Tests.RuleActions
{
    public class ComplexWorkflowInput
    {
        public decimal TotalCost;
        public decimal TotalDiscount;
        public DateTime Birthday;
        public bool IsLoyaltyMember;
        public IReadOnlyCollection<decimal> CartPrices = [];
    }

    public class ApplyDiscounts : IRulesEngineAction
    {
        public void Handle(object input, Dictionary<string, object> context)
        {
            if (input is not ComplexWorkflowInput complexWorkflowInput)
            {
                return;
            }

            foreach (var price in complexWorkflowInput.CartPrices)
            {
                complexWorkflowInput.TotalCost += price;
            }

            var discount = complexWorkflowInput.TotalCost / 100 * complexWorkflowInput.TotalDiscount;
            complexWorkflowInput.TotalCost -= discount;
        }
    }

    public class AddDiscount : IRulesEngineAction
    {
        public void Handle(object input, Dictionary<string, object> context)
        {
            if (input is not ComplexWorkflowInput complexWorkflowInput)
            {
                return;
            }

            if (!context.TryGetValue("discount", out var discountString))
            {
                return;
            }

            if (decimal.TryParse(discountString.ToString(), out var discount))
            {
                complexWorkflowInput.TotalDiscount += discount;
            }
        }
    }
}
