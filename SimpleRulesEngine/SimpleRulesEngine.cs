namespace SimpleRulesEngine
{
    public class SimpleRulesEngine
    {
        public SimpleRulesEngine(IReadOnlyCollection<Workflow> workflows)
        {
            _workflows = workflows.ToDictionary(x => x.WorkflowName, x => x);
        }

        private Dictionary<string, Delegate> _registeredActions = [];
        private readonly Dictionary<string, Workflow> _workflows;
        private readonly RuleEvaluator _ruleEvaluator = new();

        public Dictionary<string, Dictionary<string, bool>> EvaluateAll(object input)
        {
            return EvaluateAll(input, _workflows.Values);
        }

        public void RegisterAction(IRulesEngineAction concreteInstance)
        {
            _registeredActions.Add(concreteInstance.GetType().Name, concreteInstance.Handle);
        }

        public void RegisterAction<ConcreteAction>() where ConcreteAction : IRulesEngineAction, new()
        {
            var instance = new ConcreteAction();
            _registeredActions.Add(typeof(ConcreteAction).Name, instance.Handle);
        }

        public Dictionary<string, Dictionary<string, bool>> EvaluateAll(object input, IReadOnlyCollection<Workflow> workflows)
        {
            var results = new Dictionary<string, Dictionary<string, bool>>();
            foreach (var workflow in workflows)
            {
                results.Add(workflow.WorkflowName, Evaluate(input, workflow));
            }
            return results;
        }

        public Dictionary<string, bool> Evaluate(object input, string workflowName)
        {
            _ = _workflows.TryGetValue(workflowName, out var workflow);
            if (workflow is null)
            {
                return [];
            }

            return Evaluate(input, workflow);
        }

        public Dictionary<string, bool> Evaluate(object input, Workflow workflow)
        {
            return _ruleEvaluator.EvaluateWorkflow(input, _registeredActions, workflow);
        }
    }
}
