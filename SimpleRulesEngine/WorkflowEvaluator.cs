using DynamicExpresso;
using System;
using System.Collections.Generic;

namespace SimpleRulesEngine
{
    public class WorkflowEvaluator
    {
        private readonly Interpreter Interpreter = new Interpreter();

        public Dictionary<string, bool> EvaluateWorkflow(object input, IReadOnlyDictionary<string, Delegate> registeredActions, Workflow workflow)
        {
            var ruleResults = new Dictionary<string, bool>();
            foreach (var rule in workflow.Rules)
            {
                var rulePassed = EvaluateRule(input, rule);
                ruleResults.Add(rule.RuleName, rulePassed);

                if (rule.OnEvaluation != null)
                {
                    if (registeredActions.TryGetValue(rule.OnEvaluation, out var ruleAction))
                    {
                        ruleAction.DynamicInvoke(input, rule.Context);
                    }
                }
                if (rule.OnSuccess != null && rulePassed)
                {
                    if (registeredActions.TryGetValue(rule.OnSuccess, out var ruleAction))
                    {
                        ruleAction.DynamicInvoke(input, rule.Context);
                    }
                }
                else if (rule.OnFailure != null && !rulePassed)
                {
                    if (registeredActions.TryGetValue(rule.OnFailure, out var ruleAction))
                    {
                        ruleAction.DynamicInvoke(input, rule.Context);
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
