import { BaseService } from "./BaseService";
import { User, UserListItem } from "@/types/api/User";
import { UserProfile, parseUserProfile } from "@/types/api/enums/UserProfile";
import { PagedList } from "@/types/api/PagedList";

type UserQueryResponse = {
  id: string;
  username: string;
  email: string;
  emailConfirmed: boolean;
  active: boolean;
  profile: string | UserProfile;
};

type CreateUserPayload = {
  username: string;
  email: string;
  password: string;
  profile: UserProfile;
};

type UpdateUserPayload = {
  id: string;
  username: string;
  email: string;
  profile: UserProfile;
};

export type UserListParams = {
  search?: string;
  profile?: number;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
};

class UserService extends BaseService {
  constructor() {
    super("user");
  }

  private normalizeUserProfile(profile: string | UserProfile): UserProfile {
    const parsedProfile = parseUserProfile(profile);

    if (parsedProfile === undefined) {
      throw new Error(`Perfil de usuário inválido retornado pela API: ${profile}`);
    }

    return parsedProfile;
  }

  async getAll(params?: UserListParams): Promise<PagedList<UserListItem>> {
    return await this.get<PagedList<UserListItem>>("", params);
  }

  async getById(id: string): Promise<User> {
    const user = await this.get<UserQueryResponse>(id);

    return {
      id: user.id,
      username: user.username,
      email: user.email,
      emailConfirmed: user.emailConfirmed,
      active: user.active,
      password: "",
      profile: this.normalizeUserProfile(user.profile),
    };
  }

  async create(user: User): Promise<{ id: string }> {
    const payload: CreateUserPayload = {
      username: user.username,
      email: user.email,
      password: user.password ?? "",
      profile: user.profile,
    };

    return await this.post<CreateUserPayload, { id: string }>("", payload);
  }

  async update(id: string, user: User): Promise<void> {
    const payload: UpdateUserPayload = {
      id,
      username: user.username,
      email: user.email,
      profile: user.profile,
    };

    await this.put<UpdateUserPayload, void>(id, payload);
  }

  async toggleActive(id: string): Promise<void> {
    await this.patch(`${id}/deactivate`);
  }

  async resendConfirmationEmail(id: string): Promise<void> {
    await this.post<undefined, void>(`${id}/resend-confirmation-email`, undefined);
  }

  async confirmEmail(userId: string, token: string): Promise<{ message?: string; Message?: string }> {
    return await this.get<{ message?: string; Message?: string }>("confirm-email", {
      userId,
      token,
    });
  }
}

export default new UserService();
