"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class TodoSlashCommand {
    constructor(app) {
        this.app = app;
        this.command = 'todo';
        this.i18nParamsExample = 'todoList_todo_command_preview';
        this.i18nDescription = 'todoList_todo_command_description';
        this.providesPreview = true;
    }
    async executor(context, read, modify, http, persis) {
        console.log('executor called');
    }
    async executePreviewItem(item, context, read, modify, http, persis) {
        console.log(item);
        const builder = modify.getCreator().startMessage().setSender(context.getSender()).setRoom(context.getRoom());
        builder.setText('@<username> @<username>: task to do UNTIL date');
    }
}
exports.TodoSlashCommand = TodoSlashCommand;
