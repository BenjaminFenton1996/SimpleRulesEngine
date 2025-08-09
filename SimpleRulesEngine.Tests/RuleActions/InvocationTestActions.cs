using SimpleRulesEngine.Interfaces;

namespace SimpleRulesEngine.Tests.RuleActions
{
    public class InvocationTestInput
    {
        public int SomeNumber { get; set; }
        public bool OnEvaluationActionWasInvoked { get; set; }
        public bool OnSuccessActionWasInvoked { get; set; }
        public bool OnFailureActionWasInvoked { get; set; }
    }

    public class SetEvaluationActionAsInvoked : IRulesEngineAction
    {
        public void Handle(object input, Dictionary<string, object> context)
        {
            if (input is InvocationTestInput envelope)
            {
                envelope.OnEvaluationActionWasInvoked = true;
            }
        }
    }

    public class SetSuccessActionAsInvoked : IRulesEngineAction
    {
        public void Handle(object input, Dictionary<string, object> context)
        {
            if (input is InvocationTestInput envelope)
            {
                envelope.OnSuccessActionWasInvoked = true;
            }
        }
    }

    public class SetFailureActionAsInvoked : IRulesEngineAction
    {
        public void Handle(object input, Dictionary<string, object> context)
        {
            if (input is InvocationTestInput invocationTestInput)
            {
                invocationTestInput.OnFailureActionWasInvoked = true;
            }
        }
    }
}
