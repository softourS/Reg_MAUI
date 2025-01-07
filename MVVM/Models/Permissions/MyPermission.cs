using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PlannerBusConductoresN_MAUI.Models.Permissions;

public class MyPermission : BasePermission {
    // This method checks if current status of the permission.
    public override Task<PermissionStatus> CheckStatusAsync() {
        throw new NotImplementedException();
    }

    // This method is optional and a PermissionException is often thrown if a permission is not declared.
    public override void EnsureDeclared() {
        throw new NotImplementedException();
    }

    // Requests the user to accept or deny a permission.
    public override Task<PermissionStatus> RequestAsync() {
        throw new NotImplementedException();
    }

    // Indicates that the requestor should prompt the user as to why the app requires the permission, because the
    // user has previously denied this permission.
    public override bool ShouldShowRationale() {
        throw new NotImplementedException();
    }
}