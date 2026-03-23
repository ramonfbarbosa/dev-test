import { BaseEntity } from "@/types/api/BaseEntity";
import { UserProfile } from "./enums/UserProfile";

export type User = {
    id?: string,
    username: string,
    email: string,
    emailConfirmed: boolean,
    active: boolean,
    password?: string,
    profile: UserProfile;
} & BaseEntity;

export type UserListItem = Omit<User, "profile" | "password"> & {
    profile: string;
};
