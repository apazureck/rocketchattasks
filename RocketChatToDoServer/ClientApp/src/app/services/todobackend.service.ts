import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class TodobackendService {
  private getOptions() {
    return {
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + localStorage.getItem('jwt'),
        'Content-Type': 'application/json'
      })
    };
  }
  constructor(@Inject('BASE_URL') private baseUrl: string, private http: HttpClient) {
    if (!baseUrl.endsWith('/')) {
      baseUrl = baseUrl + '/';
    }
  }

  getUser(userId: number): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'api/users/' + userId, this.getOptions());
  }

  getTasksForUser(userId: number): Observable<Task[]> {
    return this.http.get<Task[]>(this.baseUrl + 'api/tasks/forUser/' + userId, this.getOptions());
  }

  getTask(taskId: number) {
    return this.http.get<Task>(this.baseUrl + 'api/tasks/' + taskId, this.getOptions());
  }

  setTaskDone(taskId: number, userId: number, done: boolean = true) {
    return this.http.get<Task>(this.baseUrl + 'api/tasks/forUser/' + userId + '/' + (done ? 'setDone' : 'setUndone') + '/' + taskId,
      this.getOptions());
  }

  getFilteredUserList(searchString: string) {
    return this.http.get<User[]>(this.baseUrl + 'api/users/filter' + (searchString ? '/' + searchString : ''), this.getOptions());
  }

  addAssigneeToTask(user: User, taskId: number) {
    return this.http.post(this.baseUrl + 'api/tasks/' + taskId + '/addAssignee', user, this.getOptions());
  }

  removeAssignee(taskID: number, userID: number) {
    return this.http.post(this.baseUrl + 'api/tasks/' + taskID + '/removeAssignee', userID, this.getOptions());
  }

  updateTask(task: Task) {
    return this.http.put(this.baseUrl + 'api/tasks', task, this.getOptions());
  }

  login(credentials: string) {
    return this.http.post(this.baseUrl + 'api/auth/login', credentials, this.getOptions());
  }
}
