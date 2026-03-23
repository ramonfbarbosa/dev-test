import { Navigate, useLocation } from "react-router-dom";
import { isExpired } from "react-jwt"

import { NAVIGATION_PATH } from "@/constants";
import Loader from "../Loader";
import { Suspense } from "react";
import useAppSelector from "@/hooks/useAppSelector";
import { UserProfile } from "@/types/api/enums/UserProfile";

interface AuthGuardType {
  belongsTo?: UserProfile[]
  children: React.ReactNode;
}

function AuthGuard({ children, belongsTo }: AuthGuardType) {
  const { access_token, user } = useAppSelector(state => state.auth);
  const { pathname } = useLocation();

  const redirectTo = user?.profile === UserProfile.Operator
    ? NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE
    : NAVIGATION_PATH.DASHBOARD.ROOT;

  if (belongsTo !== undefined && belongsTo.length > 0 && user?.profile !== undefined) {
    const authorized = belongsTo.some((role) => user.profile === role);
    if (!authorized) {
      return <Navigate to={redirectTo} replace />;
    }
  }

  if ((access_token == null || isExpired(access_token))) {
    let route = `${NAVIGATION_PATH.AUTH.SIGN_IN.ABSOLUTE}`;
    if (pathname !== '/') {
      const searchParams = new URLSearchParams({ redirect_uri: pathname });
      route += `?${searchParams}`;
    }

    return <Navigate to={route} />
  }

  return (
    <Suspense fallback={<Loader />}>
      {children}
    </Suspense>
  );
}

export default AuthGuard;
