import "./index.css";
import {Link} from "react-router";
import {useEffect, useState} from 'react';
import {fetchAuthUserData} from "../../processes/fetchAuthUserData.ts";
import type {User} from "../../entities/User.ts";
import userPhotoPlaceholder from "../../shared/assets/photo-placeholder.png";
import {LogOutButton} from "../../shared/ui/LogOutButton";
import {Logo} from "../../shared/assets/svg/Logo.tsx";

export const Header = () => {
    const [authUser, setAuthUser] = useState<User | null>(null);

    useEffect(() => {
        const loadUser = async () => {
            try {
                const fetchedUser = await fetchAuthUserData();
                fetchedUser.userPhoto = fetchedUser.userPhoto || userPhotoPlaceholder;
                const loggedUser: User = fetchedUser as User;
                setAuthUser(loggedUser);
            } catch {
                setAuthUser(null);
            }
        };

        (async () => {
            await loadUser();
        })();
    }, []);

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