import './index.css';
import './adaptive.css';
import photoPlaceholder from '../../shared/assets/photo-placeholder.png'
import type {UserWithPlaylists} from "../../processes/fetchUserWithPlaylists.ts";

interface UserCardProps {
    user: UserWithPlaylists;
}

export const UserCard = ({ user }: UserCardProps) => {
    return (
        <div className="user-card">
            <img src={user.userPhoto || photoPlaceholder} alt="User avatar" />
            <h1 className="user-name">{user.username}</h1>
            <span>Получено плейлистов: {user.playlists.length}</span>
        </div>
    );
};