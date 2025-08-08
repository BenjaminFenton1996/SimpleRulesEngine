namespace SimpleRulesEngine
{
    public class Rule
    {
        public readonly string RuleName;
        public readonly string Expression;

        public Rule(
            string ruleName,
            string expression,
            string onEvaluation = null,
            string onSuccess = null,
            string onFailure = null)
        {
            RuleName = ruleName;
            Expression = expression;
            OnEvaluation = onEvaluation;
            OnSuccess = onSuccess;
            OnFailure = onFailure;
        }

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
    }
}
