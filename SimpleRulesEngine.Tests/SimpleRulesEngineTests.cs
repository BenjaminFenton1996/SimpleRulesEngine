using SimpleRulesEngine.Tests.RuleActions;
using System.Text.Json;

namespace SimpleRulesEngine.Tests
{
    [TestFixture]
    public class SimpleRulesEngineTests
    {
        private SimpleRulesEngine _rulesEngine = new([]);

        [SetUp]
        public void SetUp()
        {
            _rulesEngine = new([]);
        }

        [TestCase]
        public void ManyLargeWorkflowsTest()
        {
            var workflows = new List<Workflow>()
            {
                new Workflow("workflowOne", new List<Rule>()
                {
                    new Rule(
                        "rule1", "input.SomeNumber == 1",
                        onEvaluation: typeof(SetEvaluationActionAsInvoked).Name
                    ),
                    new Rule(
                        "rule2", "input.OnEvaluationActionWasInvoked == true",
                        onSuccess: typeof(SetSuccessActionAsInvoked).Name
                    ),
                    new Rule(
                        "rule3", "input.OnSuccessActionWasInvoked == false",
                        onFailure: typeof(SetFailureActionAsInvoked).Name
                    ),
                    new Rule(
                        "rule4", "input.SomeNumber == 1 && input.OnEvaluationActionWasInvoked == true && input.OnSuccessActionWasInvoked == false"
                    )
                })
            };

            _rulesEngine = new SimpleRulesEngine(workflows);
            _rulesEngine.RegisterAction(new SetEvaluationActionAsInvoked());
            _rulesEngine.RegisterAction<SetSuccessActionAsInvoked>();
            _rulesEngine.RegisterAction<SetFailureActionAsInvoked>();

            var input = new InvocationTestInput()
            {
                SomeNumber = 1
            };

            //Evaluate the workflow 10,000 times to test performance
            for (int i = 0; i < 10000; i++)
            {
                var evaluationResult = _rulesEngine.EvaluateAll(input);
                Assert.Multiple(() =>
                {
                    Assert.That(input.OnSuccessActionWasInvoked, Is.True);
                    Assert.That(input.OnFailureActionWasInvoked, Is.True);
                    Assert.That(input.OnEvaluationActionWasInvoked, Is.True);
                });

                input.SomeNumber = 1;
                input.OnSuccessActionWasInvoked = false;
                input.OnFailureActionWasInvoked = false;
                input.OnEvaluationActionWasInvoked = false;
            }
        }

        [TestCase]
        public void ComplexWorkflowTest()
        {
            _rulesEngine.RegisterAction<AddDiscount>();
            _rulesEngine.RegisterAction<ApplyDiscounts>();

            var workflow = new Workflow("discountWorkflow", new List<Rule>()
            {
                new Rule(
                    "checkBirthdayDiscount", "input.Birthday.ToString() == \"22/03/1990 00:00:00\"",
                    onSuccess: typeof(AddDiscount).Name,
                    context: new (){ { "discount", 5 } }
                ),
                new Rule(
                    "checkLoyaltyDiscount", "input.IsLoyaltyMember == true",
                    onSuccess: typeof(AddDiscount).Name,
                    context: new (){ { "discount", 10 } }
                ),
                new Rule(
                    "applyDiscounts", "input.TotalDiscount > 0",
                    onSuccess: typeof(ApplyDiscounts).Name
                )
            });

            var input = new ComplexWorkflowInput()
            {
                Birthday = new DateTime(year: 1990, month: 3, day: 22),
                TotalCost = 0,
                TotalDiscount = 0,
                CartPrices = [1.50M, 4, 0.20M],
                IsLoyaltyMember = true
            };

            var evaluationResult = _rulesEngine.Evaluate(input, workflow);
            Assert.Multiple(() =>
            {
                Assert.That(evaluationResult.Values, Has.All.True);
                Assert.That(input.TotalCost, Is.EqualTo(4.845M));
            });
        }

        [TestCase]
        public void RuleActionsShouldBeTriggered()
        {
            _rulesEngine.RegisterAction(new SetEvaluationActionAsInvoked());
            _rulesEngine.RegisterAction<SetSuccessActionAsInvoked>();
            _rulesEngine.RegisterAction<SetFailureActionAsInvoked>();

            var rule = new Rule(
                "someRule", "input.SomeNumber == 1",
                onEvaluation: typeof(SetEvaluationActionAsInvoked).Name,
                onSuccess: typeof(SetSuccessActionAsInvoked).Name,
                onFailure: typeof(SetFailureActionAsInvoked).Name
            );

            var workflow = new Workflow("name", [rule]);

            var input = new InvocationTestInput()
            {
                SomeNumber = 1
            };

            //The rule should pass, so only the OnSuccess and OnEvaluation actions should be executed
            var evaluationResult = _rulesEngine.Evaluate(input, workflow);
            Assert.Multiple(() =>
            {
                Assert.That(input.OnSuccessActionWasInvoked, Is.True);
                Assert.That(input.OnFailureActionWasInvoked, Is.False);
                Assert.That(input.OnEvaluationActionWasInvoked, Is.True);
            });

            input.SomeNumber = 2;
            input.OnSuccessActionWasInvoked = false;
            input.OnFailureActionWasInvoked = false;
            input.OnEvaluationActionWasInvoked = false;

            //The rule should fail, so only the OnFailure and OnEvaluation actions should be executed
            evaluationResult = _rulesEngine.Evaluate(input, workflow);
            Assert.Multiple(() =>
            {
                Assert.That(input.OnSuccessActionWasInvoked, Is.False);
                Assert.That(input.OnFailureActionWasInvoked, Is.True);
                Assert.That(input.OnEvaluationActionWasInvoked, Is.True);
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
