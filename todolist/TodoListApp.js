"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const App_1 = require("@rocket.chat/apps-engine/definition/App");
const todo_command_1 = require("./commands/todo.command");
class TodoListApp extends App_1.App {
    constructor(info, logger, accessors) {
        super(info, logger, accessors);
    }
    async initialize() {
        console.log('Todolist app 0.0.2 initialized');
        this.getLogger().info('Todolist app initialized');
    }
    async extendConfiguration(configuration, environmentRead) {
        console.log('Adding slashcommand /todo');
        await configuration.slashCommands.provideSlashCommand(new todo_command_1.TodoSlashCommand(this));
    }
}
exports.TodoListApp = TodoListApp;
