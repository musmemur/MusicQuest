import './index.css';
import './adaptive.css';
import {Header} from "../../widgets/Header";
import {UserCard} from "../../widgets/UserCard";
import {UserData} from "../../widgets/UserData";
import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import {fetchUserWithPlaylists, type UserWithPlaylists} from "../../processes/fetchUserWithPlaylists.ts";
import {ErrorContainer} from "../../widgets/ErrorContainer";

export const UserPage = () => {
    const { userId } = useParams<{ userId?: string }>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [user, setUser] = useState<UserWithPlaylists | null>(null);

    useEffect(() => {
        if (userId) {
            const fetchUserData = async () => {
                try {
                    setIsLoading(true);
                    const userData = await fetchUserWithPlaylists(userId);
                    setUser(userData);
                } catch (error) {
                    console.error("Error fetching user data:", error);
                    setUser(null);
                }
            };

            fetchUserData();
        }
    }, [userId]);

    if (!isLoading) {
        return <div>Загрузка...</div>;
    }

    if (!user) {
        return <div>
            <Header/>
            <ErrorContainer />
        </div>
    }

    return (
        <>
            <Header/>
            <main className="userPage-main">
                <UserCard user={user}/>
                <UserData playlists={user.playlists}/>
            </main>
        </>
    );
};