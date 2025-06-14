import './index.css';
import './adaptive.css';
import {UserTrack} from "../UserTrack";
import type {Track} from "../../entities/Track.ts";

interface UserTracksProps {
    tracks: Track[];
}

export const UserTracks = ({ tracks }: UserTracksProps) => {
    return (
        <div className="user-tracks">
            {tracks.map((track: Track) => (
                <UserTrack key={track.id} track={track} />
            ))}
        </div>
    );
};