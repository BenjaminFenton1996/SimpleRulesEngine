using SimpleRulesEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleRulesEngine
{
    public class SimpleRulesEngine
    {
        public SimpleRulesEngine(IReadOnlyCollection<Workflow> workflows)
        {
            _registeredActions = new Dictionary<string, Delegate>();
            _workflows = workflows.ToDictionary(x => x.WorkflowName, x => x);
        }

        private readonly Dictionary<string, Delegate> _registeredActions;
        private readonly Dictionary<string, Workflow> _workflows;
        private readonly WorkflowEvaluator _ruleEvaluator = new WorkflowEvaluator();

        public Dictionary<string, Dictionary<string, bool>> EvaluateAll(object input)
        {
            return EvaluateAll(input, _workflows.Values);
        }

        public void RegisterAction(IRulesEngineAction concretion)
        {
            var handlerDelegate = Delegate.CreateDelegate(typeof(Action<object>), concretion, nameof(IRulesEngineAction.Handle));
            _registeredActions.Add(concretion.GetType().Name, handlerDelegate);
        }

        public void RegisterAction<ConcreteAction>() where ConcreteAction : IRulesEngineAction, new()
        {
            var concretion = new ConcreteAction();
            var handlerDelegate = Delegate.CreateDelegate(typeof(Action<object>), concretion, nameof(IRulesEngineAction.Handle));
            _registeredActions.Add(typeof(ConcreteAction).Name, handlerDelegate);
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
                return new Dictionary<string, bool>();
            }

            return Evaluate(input, workflow);
        }

        public Dictionary<string, bool> Evaluate(object input, Workflow workflow)
        {
            return _ruleEvaluator.EvaluateWorkflow(input, _registeredActions, workflow);
        }
    }
}
