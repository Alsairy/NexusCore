using NexusCore.Samples;
using Xunit;

namespace NexusCore.EntityFrameworkCore.Domains;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<NexusCoreEntityFrameworkCoreTestModule>
{

}
