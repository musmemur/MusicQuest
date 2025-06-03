import './index.css';
import {UserPlaylists} from "../UserPlaylists";
import type {Playlist} from "../../entities/Playlist.ts";

interface UserDataProps {
    playlists: Playlist[];
}

export const UserData = ({ playlists }: UserDataProps) => {
    return (
        <div className="user-data">
            <h2>Плейлисты</h2>
            <UserPlaylists playlists={playlists} />
        </div>
    );
};