using NexusCore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Permissions;

public class NexusCorePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(NexusCorePermissions.GroupName);

        // --- ZATCA E-Invoicing ---
        var zatcaGroup = context.AddGroup(
            NexusCorePermissions.Zatca.GroupName,
            L("Permission:Zatca"));

        var invoicesPermission = zatcaGroup.AddPermission(
            NexusCorePermissions.Zatca.Invoices, L("Permission:Zatca:Invoices"),
            multiTenancySide: MultiTenancySides.Tenant);
        invoicesPermission.AddChild(
            NexusCorePermissions.Zatca.InvoicesCreate, L("Permission:Zatca:Invoices:Create"));
        invoicesPermission.AddChild(
            NexusCorePermissions.Zatca.InvoicesEdit, L("Permission:Zatca:Invoices:Edit"));
        invoicesPermission.AddChild(
            NexusCorePermissions.Zatca.InvoicesDelete, L("Permission:Zatca:Invoices:Delete"));
        invoicesPermission.AddChild(
            NexusCorePermissions.Zatca.InvoicesSubmit, L("Permission:Zatca:Invoices:Submit"));

        zatcaGroup.AddPermission(
            NexusCorePermissions.Zatca.SellersManage, L("Permission:Zatca:Sellers:Manage"),
            multiTenancySide: MultiTenancySides.Tenant);
        zatcaGroup.AddPermission(
            NexusCorePermissions.Zatca.CertificatesManage, L("Permission:Zatca:Certificates:Manage"),
            multiTenancySide: MultiTenancySides.Tenant);
        zatcaGroup.AddPermission(
            NexusCorePermissions.Zatca.ManageSettings, L("Permission:Zatca:ManageSettings"),
            multiTenancySide: MultiTenancySides.Tenant);

        // --- Nafath SSO ---
        var nafathGroup = context.AddGroup(
            NexusCorePermissions.Nafath.GroupName,
            L("Permission:Nafath"));

        nafathGroup.AddPermission(
            NexusCorePermissions.Nafath.Login, L("Permission:Nafath:Login"),
            multiTenancySide: MultiTenancySides.Tenant);
        nafathGroup.AddPermission(
            NexusCorePermissions.Nafath.LinkIdentity, L("Permission:Nafath:LinkIdentity"),
            multiTenancySide: MultiTenancySides.Tenant);
        nafathGroup.AddPermission(
            NexusCorePermissions.Nafath.ManageSettings, L("Permission:Nafath:ManageSettings"),
            multiTenancySide: MultiTenancySides.Tenant);

        // --- Approval Workflows ---
        var workflowsGroup = context.AddGroup(
            NexusCorePermissions.Workflows.GroupName,
            L("Permission:Workflows"));

        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.DefinitionsCreate, L("Permission:Workflows:Definitions:Create"),
            multiTenancySide: MultiTenancySides.Tenant);
        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.DefinitionsEdit, L("Permission:Workflows:Definitions:Edit"),
            multiTenancySide: MultiTenancySides.Tenant);
        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.DefinitionsDelete, L("Permission:Workflows:Definitions:Delete"),
            multiTenancySide: MultiTenancySides.Tenant);

        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.InstancesViewAll, L("Permission:Workflows:Instances:ViewAll"),
            multiTenancySide: MultiTenancySides.Tenant);
        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.InstancesCancel, L("Permission:Workflows:Instances:Cancel"),
            multiTenancySide: MultiTenancySides.Tenant);

        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.Approve, L("Permission:Workflows:Approve"),
            multiTenancySide: MultiTenancySides.Tenant);
        workflowsGroup.AddPermission(
            NexusCorePermissions.Workflows.Delegate, L("Permission:Workflows:Delegate"),
            multiTenancySide: MultiTenancySides.Tenant);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NexusCoreResource>(name);
    }
}
