using System.Collections.Generic;

namespace SimpleRulesEngine
{
    public class Workflow
    {
        public readonly string WorkflowName;
        public readonly IReadOnlyCollection<Rule> Rules;

        public Workflow(string workflowName, IReadOnlyCollection<Rule> rules)
        {
            WorkflowName = workflowName;
            Rules = rules;
        }
    }
}
