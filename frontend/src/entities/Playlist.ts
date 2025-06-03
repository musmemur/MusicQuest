import type {Track} from "./Track.ts";

export type Playlist = {
    id: string;
    title: string;
    tracks: Track[];
}