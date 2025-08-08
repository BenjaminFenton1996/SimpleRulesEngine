using DynamicExpresso;

namespace SimpleRulesEngine
{
    public class RuleEvaluator
    {
        private readonly Interpreter Interpreter = new();

        public Dictionary<string, bool> EvaluateWorkflow(object input, IReadOnlyDictionary<string, Delegate> registeredActions, Workflow workflow)
        {
            var ruleResults = new Dictionary<string, bool>();
            foreach (var rule in workflow.Rules)
            {
                var rulePassed = EvaluateRule(input, rule);
                ruleResults.Add(rule.RuleName, rulePassed);

                if (rule.OnEvaluation is not null)
                {
                    if (registeredActions.TryGetValue(rule.OnEvaluation, out var ruleAction))
                    {
                        ruleAction.DynamicInvoke(input);
                    }
                }
                if (rulePassed && rule.OnSuccess is not null)
                {
                    if (registeredActions.TryGetValue(rule.OnSuccess, out var ruleAction))
                    {
                        ruleAction.DynamicInvoke(input);
                    }
                }
                else if (rule.OnFailure is not null)
                {
                    if (registeredActions.TryGetValue(rule.OnFailure, out var ruleAction))
                    {
                        ruleAction.DynamicInvoke(input);
                    }
                }
            }

            return ruleResults;
        }

        private bool EvaluateRule(object input, Rule rule)
        {
            return EvaluateExpression(input, rule.Expression);
        }

        private bool EvaluateExpression(object input, string expression)
        {
            var parameters = new[]
            {
                new Parameter("input", input.GetType(), input)
            };

            var result = Interpreter.Eval<bool>(expression, parameters);
            return result;
        }
    }
}
