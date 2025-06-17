import type { User } from "../entities/User.ts";
import { axiosInstance } from "../app/axiosInstance.ts";
import type {Playlist} from "../entities/Playlist.ts";

export interface UserWithPlaylists extends User {
    playlists: Playlist[];
}

interface ApiPlaylist {
    id: string;
    title: string;
    tracks?: ApiTrack[];
}

interface ApiTrack {
    id: string;
    deezerTrackId: string;
    title: string;
    artist: string;
    previewUrl: string;
    coverUrl: string;
}

export async function fetchUserWithPlaylists(userId: string): Promise<UserWithPlaylists> {
    try {
        const response = await axiosInstance.get(`/User/get-user-with-playlists/${userId}`);

        return {
            userId: response.data.id,
            username: response.data.username,
            userPhoto: response.data.userPhoto,
            playlists: response.data.playlists?.map((playlist: ApiPlaylist) => ({
                id: playlist.id,
                title: playlist.title,
                tracks: playlist.tracks?.map((track: ApiTrack) => ({
                    id: track.id,
                    deezerTrackId: track.deezerTrackId,
                    title: track.title,
                    artist: track.artist,
                    previewUrl: track.previewUrl,
                    coverUrl: track.coverUrl,
                })) || []
            })) || []
        };
    } catch (error) {
        console.error('Error fetching user with playlists:', error);
        throw error;
    }
}