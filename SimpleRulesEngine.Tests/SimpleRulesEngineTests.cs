using SimpleRulesEngine.Interfaces;

namespace SimpleRulesEngine.Tests
{
    public class InputEnvelope
    {
        public int SomeNumber { get; set; }
        public bool OnEvaluationActionWasInvoked { get; set; }
        public bool OnSuccessActionWasInvoked { get; set; }
        public bool OnFailureActionWasInvoked { get; set; }
    }

    public class OnEvaluationAction : IRulesEngineAction
    {
        public void Handle(object input)
        {
            if (input is InputEnvelope envelope)
            {
                envelope.OnEvaluationActionWasInvoked = true;
            }
        }
    }

    public class OnSuccessAction : IRulesEngineAction
    {
        public void Handle(object input)
        {
            if (input is InputEnvelope envelope)
            {
                envelope.OnSuccessActionWasInvoked = true;
            }
        }
    }

    public class OnFailureAction : IRulesEngineAction
    {
        public void Handle(object input)
        {
            if (input is InputEnvelope envelope)
            {
                envelope.OnFailureActionWasInvoked = true;
            }
        }
    }

    public class SimpleRulesEngineTests
    {
        private SimpleRulesEngine _rulesEngine = new([]);

        [SetUp]
        public void SetUp()
        {
            _rulesEngine = new([]);
        }

        [TestCase]
        public void RuleActionsShouldBeTriggered()
        {
            _rulesEngine.RegisterAction<OnSuccessAction>();
            _rulesEngine.RegisterAction<OnFailureAction>();
            _rulesEngine.RegisterAction(new OnEvaluationAction());

            var rule = new Rule(
                "someRule", "input.SomeNumber == 1",
                onEvaluation: typeof(OnEvaluationAction).Name,
                onSuccess: typeof(OnSuccessAction).Name,
                onFailure: typeof(OnFailureAction).Name
            );

            var workflow = new Workflow("name", [rule]);

            var envelope = new InputEnvelope()
            {
                SomeNumber = 1
            };

            //The rule should pass, so only the OnSuccess and OnEvaluation actions should be executed
            var evaluationResult = _rulesEngine.Evaluate(envelope, workflow);
            Assert.Multiple(() =>
            {
                Assert.That(envelope.OnSuccessActionWasInvoked, Is.True);
                Assert.That(envelope.OnFailureActionWasInvoked, Is.False);
                Assert.That(envelope.OnEvaluationActionWasInvoked, Is.True);
            });

            envelope.SomeNumber = 2;
            envelope.OnSuccessActionWasInvoked = false;
            envelope.OnFailureActionWasInvoked = false;
            envelope.OnEvaluationActionWasInvoked = false;

            //The rule should fail, so only the OnFailure and OnEvaluation actions should be executed
            evaluationResult = _rulesEngine.Evaluate(envelope, workflow);
            Assert.Multiple(() =>
            {
                Assert.That(envelope.OnSuccessActionWasInvoked, Is.False);
                Assert.That(envelope.OnFailureActionWasInvoked, Is.True);
                Assert.That(envelope.OnEvaluationActionWasInvoked, Is.True);
            });
        }

        [Test]
        public void EvaluateMultipleValidRulesAndMultipleValidWorkflows()
        {
            const string workflowWithPassingRules = "workflowWithPassingRules";
            const string workflowWithFailingRule = "workflowWithFailingRule";

            var workflows = new List<Workflow>() {
                new (workflowWithPassingRules, [
                    new ("integerRule", "input.SomeInteger == 1"),
                    new ("stringRule", "input.SomeString == \"Test\"")
                ]),
                new (workflowWithFailingRule, [
                    new ("integerRule", "input.SomeInteger == 2"),
                    new ("stringRule", "input.SomeString == \"Test\"")
                ])
            };

            var someInstance = new
            {
                SomeInteger = 1,
                SomeString = "Test"
            };

            var evaluationResults = _rulesEngine.EvaluateAll(someInstance, workflows);

            Assert.Multiple(() =>
            {
                Assert.That(evaluationResults[workflowWithPassingRules].Values, Has.All.EqualTo(true));
                Assert.That(evaluationResults[workflowWithFailingRule]["integerRule"], Is.False);
                Assert.That(evaluationResults[workflowWithFailingRule]["stringRule"], Is.True);
            });
        }

        [Test]
        public void ValidWorkflowsShouldEvaluateCorrectly_ReferenceInput()
        {
            var rule = new Rule("someRule", "input.SomeValue == 1");
            var workflow = new Workflow("workflowName", [rule]);

            var someInstance = new
            {
                SomeValue = 1
            };

            var evaluationResult = _rulesEngine.Evaluate(someInstance, workflow);
            Assert.That(evaluationResult["someRule"], Is.True);
        }

        [TestCase("1", "input == \"1\"", true)]
        [TestCase(1, "input == 1", true)]
        [TestCase("abc", "input == \"1\"", false)]
        public void ValidWorkflowsShouldEvaluateCorrectly_SimpleInput(object input, string expression, bool expectedResult)
        {
            var rule = new Rule("someRule", expression);
            var workflow = new Workflow("name", [rule]);

            var evaluationResult = _rulesEngine.Evaluate(input, workflow);
            Assert.That(evaluationResult["someRule"], Is.EqualTo(expectedResult));
        }
    }
}
