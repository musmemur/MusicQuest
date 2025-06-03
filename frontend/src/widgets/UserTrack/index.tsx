import { useState, useEffect } from 'react';
import './index.css';
import type { Track } from "../../entities/Track.ts";
import { PausedTrack } from "../../shared/assets/svg/PausedTrack.tsx";
import { PlayedTrack } from "../../shared/assets/svg/PlayedTrack.tsx";

declare global {
    interface Window {
        DZ: any;
    }
}

export const UserTrack = ({ track }: { track: Track }) => {
    const [isPlaying, setIsPlaying] = useState(false);
    const [playerReady, setPlayerReady] = useState(false);

    useEffect(() => {
        if (!window.DZ) {
            const script = document.createElement('script');
            script.src = 'https://cdn-files.deezer.com/js/min/dz.js';
            script.onload = () => {
                window.DZ.init({
                    appId: 'YOUR_APP_ID', // Замените на ваш реальный APP_ID
                    channelUrl: `${window.location.origin}/channel.html`,
                    player: {
                        onload: () => setPlayerReady(true),
                        onplayerplay: () => setIsPlaying(true),
                        onplayerpause: () => setIsPlaying(false)
                    }
                });
            };
            document.body.appendChild(script);
        } else if (window.DZ && !playerReady) {
            window.DZ.init({
                appId: 'YOUR_APP_ID',
                channelUrl: `${window.location.origin}/channel.html`,
                player: {
                    onload: () => setPlayerReady(true),
                    onplayerplay: () => setIsPlaying(true),
                    onplayerpause: () => setIsPlaying(false)
                }
            });
        }

        return () => {
            if (window.DZ?.player) {
                window.DZ.player.pause();
            }
        };
    }, []);

    const handlePlayPause = () => {
        if (!playerReady) return;

        if (isPlaying) {
            window.DZ.player.pause();
        } else {
            window.DZ.player.playTracks([track.id]);
        }
    };

    return (
        <div className="user-track-container">
            <div style={{ position: 'relative' }}>
                <img src={track.coverUrl} alt={`Cover: ${track.title}`} />
                <button
                    className="play-button"
                    onClick={handlePlayPause}
                    disabled={!playerReady || !track.id}
                >
                    {!playerReady ? (
                        <span>Loading...</span>
                    ) : isPlaying ? (
                        <PlayedTrack />
                    ) : (
                        <PausedTrack />
                    )}
                </button>
            </div>
            <div className="user-track-info">
                <span>{track.artist} — {track.title}</span>
            </div>
        </div>
    );
};