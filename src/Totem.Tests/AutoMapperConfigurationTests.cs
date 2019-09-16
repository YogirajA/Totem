using static Totem.Tests.Testing;

namespace Totem.Tests
{
    public class AutoMapperConfigurationTests
    {
        public void ShouldPassValidation()
        {
            MapperConfiguration().AssertConfigurationIsValid();
        }
    }
}