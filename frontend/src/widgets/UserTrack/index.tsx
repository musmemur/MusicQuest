import './index.css';
import './adaptive.css';
import type { Track } from "../../entities/Track.ts";
import { PausedTrack } from "../../shared/assets/svg/PausedTrack.tsx";

export const UserTrack = ({ track }: { track: Track }) => {

    const openInDeezer = () => {
        window.open(`https://www.deezer.com/track/${track.deezerTrackId}`, '_blank');
    };

    return (
        <div className="user-track-container">
            <div style={{ position: 'relative' }}>
                <img src={track.coverUrl} alt={`Cover: ${track.title}`} />
                <button
                    className="play-button"
                    onClick={openInDeezer}
                    disabled={!track.previewUrl}
                >
                    <PausedTrack />
                </button>
            </div>
            <div className="user-track-info">
                <span>{track.artist} — {track.title}</span>
            </div>
        </div>
    );
};