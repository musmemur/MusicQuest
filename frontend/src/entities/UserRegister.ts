import type {UserPhoto} from "./UserPhoto.ts";

export type UserRegister = {
    username: string;
    password: string;
    userPhoto: UserPhoto | null;
}