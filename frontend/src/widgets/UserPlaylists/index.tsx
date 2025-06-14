import './index.css';
import './adaptive.css';
import {UserTracks} from "../UserTracks";
import type {Playlist} from "../../entities/Playlist.ts";
import {useState} from "react";
import {Expand} from "../../shared/assets/svg/Expand.tsx";
import {RollUp} from "../../shared/assets/svg/RollUp.tsx";

interface UserPlaylistsProps {
    playlists: Playlist[];
}

export const UserPlaylists = ({playlists}: UserPlaylistsProps) => {
    const [expandedPlaylists, setExpandedPlaylists] = useState<Record<string, boolean>>({});

    const togglePlaylist = (playlistId: string) => {
        setExpandedPlaylists(prev => ({
            ...prev,
            [playlistId]: !prev[playlistId]
        }));
    };

    return (
        <div className="user-playlists">
            {playlists.length > 0 ? (
                <>
                    {playlists.map((playlist: Playlist) => (
                        <div className="playlist" key={playlist.id}>
                            <button className="toggle-tracks-btn" onClick={() => togglePlaylist(playlist.id)}>
                                <h3>{playlist.title}</h3>
                                <div>
                                    {expandedPlaylists[playlist.id] ? <RollUp /> : <Expand />}
                                </div>
                            </button>
                            {expandedPlaylists[playlist.id] && <UserTracks tracks={playlist.tracks}/>}
                        </div>
                    ))}
                </>
            ): <div className="no-playlists">Нет плейлистов</div>}
        </div>
    )
}