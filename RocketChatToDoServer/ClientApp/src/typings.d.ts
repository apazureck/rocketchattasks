/* SystemJS module definition */
declare var module: NodeModule;
interface NodeModule {
  id: string;
}

interface User {
  id: number,
  name: string,
  tasks: UserTaskMap[]
}

interface UserTaskMap {
  id: number
  userId: number,
  taskId: number,
  user: User,
  task: Task
}

interface Task {
  id: number,
  dueDate: Date,
  creationDate: Date,
  initiatorId: number,
  initiator: User,
  assignees: UserTaskMap[],
  title: string,
  description: string,
  done: boolean
}