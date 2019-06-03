import { IHttp, IModify, IPersistence, IRead } from '@rocket.chat/apps-engine/definition/accessors';
import { ISlashCommand,  ISlashCommandPreviewItem, SlashCommandContext } from '@rocket.chat/apps-engine/definition/slashcommands';
import { TodoListApp } from '../TodoListApp'

export class TodoSlashCommand implements ISlashCommand {
    public command: string = 'todo';
    public i18nParamsExample = 'todoList_todo_command_preview';
    public i18nDescription = 'todoList_todo_command_description';
    public providesPreview = true;
    public permission?: string | undefined;
    constructor(private readonly app: TodoListApp) {

    }
    // tslint:disable-next-line:max-line-length
    public async executor(context: SlashCommandContext, read: IRead, modify: IModify, http: IHttp, persis: IPersistence): Promise<void> {
        console.log('executor called');
    }

    // tslint:disable-next-line:max-line-length
    public async executePreviewItem(item: ISlashCommandPreviewItem, context: SlashCommandContext, read: IRead, modify: IModify, http: IHttp, persis: IPersistence): Promise<void> {
        console.log(item);

        const builder = modify.getCreator().startMessage().setSender(context.getSender()).setRoom(context.getRoom());

        builder.setText('@<username> @<username>: task to do UNTIL date');
    }
}
