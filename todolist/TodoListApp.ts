import {
    IAppAccessors,
    IConfigurationExtend,
    IEnvironmentRead,
    ILogger,
} from '@rocket.chat/apps-engine/definition/accessors';
import { App } from '@rocket.chat/apps-engine/definition/App';
import { IAppInfo } from '@rocket.chat/apps-engine/definition/metadata';
import { SettingType } from '@rocket.chat/apps-engine/definition/settings';
import { TodoSlashCommand } from './commands/todo.command';

export class TodoListApp extends App {
    constructor(info: IAppInfo, logger: ILogger, accessors: IAppAccessors) {
        super(info, logger, accessors);
    }

    public async initialize(): Promise<void> {
        console.log('Todolist app 0.0.2 initialized');
    }

    protected async extendConfiguration(configuration: IConfigurationExtend, environmentRead: IEnvironmentRead): Promise<void> {
        // console.log('Adding setting LoginName');
        // await configuration.settings.provideSetting({
        //     id: 'todoList_loginName',
        //     type: SettingType.STRING,
        //     packageValue: 'todo.bot',
        //     required: true,
        //     public: false,
        //     i18nLabel: 'settings_loginName_Label',
        //     i18nDescription: 'settings_todoList_loginName_Description',
        // });
        // console.log('adding setting login password');
        // await configuration.settings.provideSetting({
        //     id: 'todoList_loginPassword',
        //     type: SettingType.STRING,
        //     packageValue: '',
        //     required: true,
        //     public: false,
        //     i18nLabel: 'settings_loginPassword_Label',
        //     i18nDescription: 'settings_loginPassword_Description',
        // });
        console.log('Adding slashcommand /todo');
        await configuration.slashCommands.provideSlashCommand(new TodoSlashCommand(this));
    }
}
