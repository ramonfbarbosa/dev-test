import { logout } from "@/redux/slices/auth.slice";
import { store } from "@/redux/store";

const AUTH_STORAGE_KEY = "persist:auth";

export function clearAuthSession() {
  store.dispatch(logout());
  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}
