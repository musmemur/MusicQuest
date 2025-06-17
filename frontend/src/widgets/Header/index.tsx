import "./index.css";
import './adaptive.css';
import {Link} from "react-router";
import {useEffect} from 'react';
import {LogOutButton} from "../../shared/ui/LogOutButton";
import {Logo} from "../../shared/assets/svg/Logo.tsx";
import type {AppDispatch, RootState} from "../../app/store.ts";
import {useDispatch, useSelector} from "react-redux";
import {loadAuthUser} from "../../features/loadAuthUser.ts";

export const Header = () => {
    const dispatch: AppDispatch = useDispatch();
    const authUser = useSelector((state: RootState) => state.loadAuthUser.value);

    useEffect(() => {
        if (!authUser) {
            dispatch(loadAuthUser());
        }
    }, [authUser, dispatch]);

    return (
        <header>
            <nav className="header-nav">
                <Link to="/home" className="logo">
                    <Logo />
                    <span>MusicQuiz</span>
                </Link>

                {authUser ? (
                    <div className="auth-user-container">
                        <Link to={`/user/${encodeURIComponent(authUser.userId)}`} className="user-header-info">
                            <img src={authUser.userPhoto} alt={`${authUser.username} avatar`}/>
                            <span>{authUser.username}</span>
                        </Link>
                        <LogOutButton/>
                    </div>
                ) : (<Link to="/sign-up" className="enter-button">войти</Link>)}
            </nav>
        </header>
    );
};