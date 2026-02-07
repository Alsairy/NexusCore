using Xunit;

namespace NexusCore.EntityFrameworkCore;

[CollectionDefinition(NexusCoreTestConsts.CollectionDefinitionName)]
public class NexusCoreEntityFrameworkCoreCollection : ICollectionFixture<NexusCoreEntityFrameworkCoreFixture>
{

}
