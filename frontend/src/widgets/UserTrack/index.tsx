import './index.css';
import './adaptive.css';
import type { Track } from "../../entities/Track.ts";
import { PausedTrack } from "../../shared/assets/svg/PausedTrack.tsx";
import {useState} from "react";

export const UserTrack = ({ track }: { track: Track }) => {
    const [imageLoaded, setImageLoaded] = useState(false);

    const openInDeezer = () => {
        window.open(`https://www.deezer.com/track/${track.deezerTrackId}`, '_blank');
    };

    return (
        <div className="user-track-container">
            <div className="user-track-photo-container">
                <img
                    src={track.coverUrl}
                    alt={`Cover: ${track.title}`}
                    onLoad={() => setImageLoaded(true)}
                    style={{ display: imageLoaded ? 'block' : 'none' }}
                />
                {imageLoaded && (
                    <button
                        className="play-button"
                        onClick={openInDeezer}
                        disabled={!track.previewUrl}
                    >
                        <PausedTrack />
                    </button>
                )}
            </div>
            {imageLoaded && (
                <div className="user-track-info">
                    <span>{track.artist} — {track.title}</span>
                </div>
            )}
        </div>
    );
};