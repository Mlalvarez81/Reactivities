import { SegmentGroup } from 'semantic-ui-react';
import { Activity } from './../models/activity';
import { makeAutoObservable, runInAction } from 'mobx';
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { ChatComment } from "../models/comment";
import { store } from './store';
import { runInContext } from 'vm';

export default class CommentStore{
    comments: ChatComment[] = [];
    hubConnection: HubConnection | null = null;

    constructor() {
        makeAutoObservable(this);
    }

    createHubconnection = (activityId: string) => {
        if (store.activityStore.selectedActivity){
            this.hubConnection = new HubConnectionBuilder()
                .withUrl('http://localhost:5000/chat?activityId=' + activityId, {
                    accessTokenFactory: () => store.userStore.user?.token!
                })
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build();

            this.hubConnection.start().catch(error => console.log('Error establiching the connection: ', error));

            this.hubConnection.on('LoadComments', (comments: ChatComment[]) => {
                runInAction(() => {
                    comments.forEach(comment => {
                        comment.createdAt = new Date(comment.createdAt + 'Z');
                    })
                    this.comments = comments;
                });
            })

            this.hubConnection.on('ReceiveComment', (comment: ChatComment) => {
                runInAction(() => {
                    comment.createdAt = new  Date(comment.createdAt)
                    this.comments.unshift(comment);
                })
            })
        }
    }

    stopHubConnection = () => {
        this.hubConnection?.stop().catch( error => console.log('Error stopping connection: ', error));
    }

    clearcomments = () => {
        this.comments = [];
        this.stopHubConnection();
    }

    
    addComment = async(values: {body: string, activityId?: string}) => {
        values.activityId = store.activityStore.selectedActivity?.id;
        try {
            await this.hubConnection?.invoke('SendComment', values);            
        } catch (error) {
            console.log(error);
        }
    }

    
}