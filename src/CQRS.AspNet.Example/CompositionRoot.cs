using CQRS.LightInject;
using LightInject;

namespace CQRS.AspNet.Example;

public class CompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.RegisterCommandHandlers();
        serviceRegistry.RegisterQueryHandlers();
    }
}