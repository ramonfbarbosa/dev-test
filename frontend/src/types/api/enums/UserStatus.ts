export enum UserStatus {
    Confirmed = 0,
    PendingInformation = 1,
    ForceChangePassword = 2,
    Blocked = 3
}

export function getBadgeColorByUserStatus(value: UserStatus) {
    switch (value) {
        case UserStatus.Confirmed:
            return "success";
        case UserStatus.PendingInformation:
            return "warning";
        case UserStatus.ForceChangePassword:
            return "info";
        case UserStatus.Blocked:
            return "danger";
    }
}


