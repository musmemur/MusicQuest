import './index.css';
import type {FC} from "react";
import {useNavigate} from "react-router";

export const SignUpPage: FC = () => {
    const navigate = useNavigate();

    return (
        <div className="sign-up-page">
            <div className="sign-up-page-container">
                <button onClick={() => navigate(-1)}>X</button>
                <main className="sign-up-main-container">
                    <button onClick={() => navigate("./login")}><span>ВОЙТИ</span></button>
                    <button onClick={() => navigate("./register")}><span>ЗАРЕГИСТРИРОВАТЬСЯ</span></button>
                </main>
            </div>
        </div>
    );
};
