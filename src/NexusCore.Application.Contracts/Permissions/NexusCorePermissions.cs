namespace NexusCore.Permissions;

public static class NexusCorePermissions
{
    public const string GroupName = "NexusCore";

    public static class Zatca
    {
        public const string GroupName = NexusCorePermissions.GroupName + ".Zatca";

        public const string Invoices = GroupName + ".Invoices";
        public const string InvoicesCreate = Invoices + ".Create";
        public const string InvoicesEdit = Invoices + ".Edit";
        public const string InvoicesDelete = Invoices + ".Delete";
        public const string InvoicesSubmit = Invoices + ".Submit";

        public const string SellersManage = GroupName + ".Sellers.Manage";
        public const string CertificatesManage = GroupName + ".Certificates.Manage";
        public const string ManageSettings = GroupName + ".ManageSettings";
    }

    public static class Nafath
    {
        public const string GroupName = NexusCorePermissions.GroupName + ".Nafath";

        public const string Login = GroupName + ".Login";
        public const string LinkIdentity = GroupName + ".LinkIdentity";
        public const string ManageSettings = GroupName + ".ManageSettings";
    }

    public static class Workflows
    {
        public const string GroupName = NexusCorePermissions.GroupName + ".Workflows";

        public const string DefinitionsCreate = GroupName + ".Definitions.Create";
        public const string DefinitionsEdit = GroupName + ".Definitions.Edit";
        public const string DefinitionsDelete = GroupName + ".Definitions.Delete";

        public const string InstancesViewAll = GroupName + ".Instances.ViewAll";
        public const string InstancesCancel = GroupName + ".Instances.Cancel";

        public const string Approve = GroupName + ".Approve";
        public const string Delegate = GroupName + ".Delegate";
    }
}
