namespace SimpleRulesEngine
{
    public class Rule(
        string ruleName,
        string expression,
        string? onEvaluation = null,
        string? onSuccess = null,
        string? onFailure = null)
    {
        public readonly string RuleName = ruleName;
        public readonly string Expression = expression;

        /// <summary>
        /// This action is executed whether the rule passes or fails
        /// </summary>
        public readonly string? OnEvaluation = onEvaluation;

        /// <summary>
        /// This action is only executed if the rule passes
        /// </summary>
        public readonly string? OnSuccess = onSuccess;

        /// <summary>
        /// This action is only executed if the rule fails
        /// </summary>
        public readonly string? OnFailure = onFailure;
    }
}
