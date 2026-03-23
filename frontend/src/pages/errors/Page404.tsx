import React from "react";
import { Link } from "react-router-dom";
import { Helmet } from "react-helmet-async";

import { Button } from "react-bootstrap";
import { NAVIGATION_PATH } from "@/constants";

import Image from "@/assets/img/page404.svg"

import useAppSelector from "@/hooks/useAppSelector";
import { UserProfile } from "@/types/api/enums/UserProfile";

const Page404 = () => {
    const { user } = useAppSelector((state) => state.auth);
    const navigate: string = !!user && user.profile === UserProfile.Operator
        ? NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE
        : NAVIGATION_PATH.DASHBOARD.ROOT;

    return (
        <React.Fragment>
            <Helmet title="Página não encontrada" />
            <div className="text-center" style={{ display: 'flex', flexDirection: 'column', gap: 20, alignItems: 'center', maxWidth: '90vw', margin: 'auto' }}>
                <img src={Image} width={400} />
                <p className="h2">Página não encontrado</p>
                <p className="lead fw-normal mt-3 mb-4">
                    A página que voce está procurando não existe.
                </p>
                <Link to={navigate}>
                    <Button variant="primary" size="lg">
                        Retornar ao site
                    </Button>
                </Link>
            </div>
        </React.Fragment>
    );
}

export default Page404;
