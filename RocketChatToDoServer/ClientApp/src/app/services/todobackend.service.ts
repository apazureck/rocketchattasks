import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
    return this.http.get<User[]>(this.baseUrl + 'api/users/filter/' + searchString);
  }
}
