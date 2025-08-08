namespace SimpleRulesEngine
{
    public class Workflow(string workflowName, IReadOnlyCollection<Rule> rules)
    {
        public readonly string WorkflowName = workflowName;
        public readonly IReadOnlyCollection<Rule> Rules = rules;
    }
}
