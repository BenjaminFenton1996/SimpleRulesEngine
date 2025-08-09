using System.Collections.Generic;

namespace SimpleRulesEngine.Interfaces
{
    public interface IRulesEngineAction
    {
        void Handle(object input, Dictionary<string, object> context);
    }
}
