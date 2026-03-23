export enum UserProfile {
    Administrator = 1,
    Operator = 2,
}

export function getBadgeColorByUserProfile(value: string) {
    switch (value) {
        case UserProfile[UserProfile.Administrator]:
            return "dark";
        case UserProfile[UserProfile.Operator]:
            return "secondary";
    }
}

export function getUserProfileLabel(value?: UserProfile | string | null) {
    switch (value) {
        case UserProfile.Administrator:
        case UserProfile[UserProfile.Administrator]:
            return "Administrador";
        case UserProfile.Operator:
        case UserProfile[UserProfile.Operator]:
            return "Operador";
        default:
            return value ? `${value}` : "";
    }
}

export function parseUserProfile(value?: UserProfile | string | null): UserProfile | undefined {
    if (value === undefined || value === null || value === "") {
        return undefined;
    }

    if (typeof value === "number") {
        return value as UserProfile;
    }

    const parsedValue = UserProfile[value as keyof typeof UserProfile];
    if (typeof parsedValue === "number") {
        return parsedValue as UserProfile;
    }

    const numericValue = Number(value);
    if (!Number.isNaN(numericValue) && UserProfile[numericValue as UserProfile] !== undefined) {
        return numericValue as UserProfile;
    }

    return undefined;
}

export function userProfileOptions() {
    return [
        { id: UserProfile.Administrator, name: "Administrador" },
        { id: UserProfile.Operator, name: "Operador" },
    ]
}
