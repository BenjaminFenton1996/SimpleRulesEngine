using System.Collections.Generic;

namespace SimpleRulesEngine
{
    public class Rule
    {
        public Rule(
            string ruleName,
            string expression,
            string onEvaluation = null,
            string onSuccess = null,
            string onFailure = null,
            Dictionary<string, object> context = null)
        {
            RuleName = ruleName;
            Expression = expression;
            OnEvaluation = onEvaluation;
            OnSuccess = onSuccess;
            OnFailure = onFailure;
            Context = context;
        }

        public readonly string RuleName;
        public readonly string Expression;

        /// <summary>
        /// This action is executed whether the rule passes or fails and always goes first
        /// </summary>
        public readonly string OnEvaluation;

        /// <summary>
        /// This action is only executed if the rule passes
        /// </summary>
        public readonly string OnSuccess;

        /// <summary>
        /// This action is only executed if the rule fails
        /// </summary>
        public readonly string OnFailure;

        /// <summary>
        /// Holds additional context for use in rule actions
        /// </summary>
        public readonly Dictionary<string, object> Context;
    }
}
