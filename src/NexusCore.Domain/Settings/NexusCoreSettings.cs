namespace NexusCore.Settings;

public static class NexusCoreSettings
{
    private const string Prefix = "NexusCore";

    public static class Saudi
    {
        private const string SaudiPrefix = Prefix + ".Saudi";

        public const string DefaultCalendar = SaudiPrefix + ".DefaultCalendar";

        public static class Zatca
        {
            private const string ZatcaPrefix = SaudiPrefix + ".Zatca";

            public const string Environment = ZatcaPrefix + ".Environment";
            public const string ApiBaseUrl = ZatcaPrefix + ".ApiBaseUrl";
            public const string ComplianceCsid = ZatcaPrefix + ".ComplianceCsid";
            public const string ProductionCsid = ZatcaPrefix + ".ProductionCsid";
            public const string Secret = ZatcaPrefix + ".Secret";
        }

        public static class Nafath
        {
            private const string NafathPrefix = SaudiPrefix + ".Nafath";

            public const string AppId = NafathPrefix + ".AppId";
            public const string AppKey = NafathPrefix + ".AppKey";
            public const string ApiBaseUrl = NafathPrefix + ".ApiBaseUrl";
            public const string CallbackUrl = NafathPrefix + ".CallbackUrl";
            public const string TimeoutSeconds = NafathPrefix + ".TimeoutSeconds";
        }
    }
}
