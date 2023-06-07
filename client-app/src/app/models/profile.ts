import { string } from "yup"
import { User } from "./user";

export interface Profile {
    username: string;
    displayName: string;
    image?: string;
    bio?:string;
}

export class Profile implements Profile {
    constructor(user: User){
        this.username = user.username;
        this.displayName = this.displayName;
        this.image = user.image;
    }
}
