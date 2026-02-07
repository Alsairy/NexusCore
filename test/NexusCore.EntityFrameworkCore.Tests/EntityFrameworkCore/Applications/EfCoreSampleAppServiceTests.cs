using NexusCore.Samples;
using Xunit;

namespace NexusCore.EntityFrameworkCore.Applications;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<NexusCoreEntityFrameworkCoreTestModule>
{

}
