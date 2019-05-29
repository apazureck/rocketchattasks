import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class TodobackendService {
  constructor(@Inject('BASE_URL') private baseUrl: string, private http: HttpClient) {
    if (!baseUrl.endsWith('/')) {
      baseUrl = baseUrl + '/';
    }
  }

  getUser(userId: number): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'api/users/' + userId);
  }

  getTasksForUser(userId: number): Observable<Task[]> {
    return this.http.get<Task[]>(this.baseUrl + 'api/tasks/forUser/' + userId);
  }

  getTask(taskId: number) {
    return this.http.get<Task>(this.baseUrl + 'api/tasks/' + taskId);
  }

  setTaskDone(taskId: number, userId: number, done: boolean = true) {
    return this.http.get<Task>(this.baseUrl + 'api/tasks/forUser/' + userId + '/' + (done ? 'setDone' : 'setUndone') + '/' + taskId);
  }

  getFilteredUserList(searchString: string) {
    return this.http.get<User[]>(this.baseUrl + 'api/users/filter' + (searchString ? '/' + searchString : ''));
  }

  addAssigneeToTask(user: User, taskId: number) {
    return this.http.post(this.baseUrl + 'api/tasks/' + taskId + '/addAssignee', user);
  }

  removeAssignee(taskID: number, userID: number) {
    return this.http.post(this.baseUrl + 'api/tasks/' + taskID + '/removeAssignee', userID);
  }

  updateTask(task: Task) {
    return this.http.put(this.baseUrl + 'api/tasks', task);
  }

  login(credentials: string) {
    return this.http.post(this.baseUrl + '/api/auth/login', credentials, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    });
  }
}
