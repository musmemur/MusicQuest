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
    const [isLoading, setIsLoading] = useState<boolean>(true); // Начинаем с true
    const [user, setUser] = useState<UserWithPlaylists | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchUserData = async () => {
            if (!userId) {
                setError("User ID is not provided");
                setIsLoading(false);
                return;
            }

            try {
                setIsLoading(true);
                setError(null);
                const userData = await fetchUserWithPlaylists(userId);
                setUser(userData);
            } catch (error) {
                console.error("Error fetching user data:", error);
                setError("Failed to load user data");
                setUser(null);
            } finally {
                setIsLoading(false);
            }
        };

        fetchUserData();
    }, [userId]);

    if (isLoading) {
        return (
            <div className="page-container">
                <Header />
                <div className="loading-container">
                    <div>Загрузка...</div>
                </div>
            </div>
        );
    }

    if (error || !user) {
        return (
            <div className="page-container">
                <Header />
                <ErrorContainer />
            </div>
        );
    }

    return (
        <div className="page-container">
            <Header />
            <main className="userPage-main">
                <UserCard user={user} />
                <UserData playlists={user.playlists} />
            </main>
        </div>
    );
};