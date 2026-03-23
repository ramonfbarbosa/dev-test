import axios, { AxiosError } from "axios";
import { NAVIGATION_PATH } from "@/constants";
import { store } from "@/redux/store";
import { AuthState } from "@/redux/slices/auth.slice";
import { clearAuthSession } from "@/utils/authSession";

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL as string,
});

let abortController = new AbortController();

axiosInstance.interceptors.request.use(
  (config) => {
    config.signal = abortController.signal;

    const auth = store.getState().auth as AuthState;
    if (auth && auth.access_token) {
      config.headers ??= {};
      config.headers.Authorization = `Bearer ${auth.access_token}`;
    }

    if (config.params) {
      Object.entries(config.params)?.forEach(([key, value]) => {
        if (typeof value === "string" && value.length === 10 && value.match(/^\d{2}\/\d{2}\/\d{4}$/)) {
          config.params[key] = value.split('/').reverse().join('-');
        }
      })
    }

    return config;
  });

axiosInstance.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      clearAuthSession();
      const pathname = window.location.pathname;
      let route = `${NAVIGATION_PATH.AUTH.SIGN_IN.ABSOLUTE}`;
      if (pathname !== '/') {
        const searchParams = new URLSearchParams({ redirect_uri: pathname });
        route += `?${searchParams}`;
      }

      window.location.href = (window.location.host.startsWith("localhost")
        ? "http://"
        : "https://")
        + window.location.host
        + route
      return;
    }
    if (error.response?.status == 404) {
      return Promise.reject({ message: "Recurso não encontrado", status: 404 });
    }
    else if (error.response?.status === 403) {
      return Promise.reject({ message: "Recurso proibido", status: 404 })
    }
    else if (error?.response?.data) {
      return Promise.reject(error.response.data);
    }
    else if (error.message === 'Network Error') {
      return Promise.reject({ message: "Falha de conexão com o servidor", status: 500 });
    }

    return Promise.reject(error);
  }
);

export const cancelPendingRequests = () => {
  abortController.abort();
  abortController = new AbortController();
};

export default axiosInstance;
