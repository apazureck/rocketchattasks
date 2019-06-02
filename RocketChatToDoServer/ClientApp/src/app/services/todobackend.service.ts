import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, ObservableInput } from 'rxjs/Observable';
import { catchError } from 'rxjs/operators';

@Injectable()
export class TodobackendService {
  public lastPageBeforeLogin: string;
  constructor(@Inject('BASE_URL') private baseUrl: string, private http: HttpClient) {
    if (!baseUrl.endsWith('/')) {
      baseUrl = baseUrl + '/';
    }
  }

  private handleError<T>(err: any, caught: Observable<T>): ObservableInput<T> {
    console.error(err);
    if (err instanceof HttpErrorResponse) {
      if (err.status === 401) {
        try {
          localStorage.removeItem('jwt');
        } catch (error) {
          console.error(error);
        }
      }
    }
      throw err;
  }

  private getOptions() {
    return {
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + localStorage.getItem('jwt'),
        'Content-Type': 'application/json'
      })
    };
  }

  private get<T>(relativeUrl: string): Observable<T> {
    return this.http.get<T>(this.baseUrl + relativeUrl, this.getOptions())
      .pipe(catchError(this.handleError)) as Observable<T>;
  }

  private post<T>(relativeUrl: string, body: any): Observable<T> {
    return this.http.post<T>(this.baseUrl + relativeUrl, body, this.getOptions())
      .pipe(catchError(this.handleError)) as Observable<T>;
  }

  private put<T>(relativeUrl: string, body: any): Observable<T> {
    return this.http.put<T>(this.baseUrl + relativeUrl, body, this.getOptions())
      .pipe(catchError(this.handleError)) as Observable<T>;
  }

  getUser(userId: number): Observable<User> {
    return this.get<User>('api/users/' + userId);
  }

  getTasksForUser(userId: number): Observable<Task[]> {
    return this.get<Task[]>('api/tasks/forUser/' + userId);
  }

  getTask(taskId: number) {
    return this.get<Task>('api/tasks/' + taskId);
  }

  setTaskDone(taskId: number, userId: number, done: boolean = true) {
    return this.post<Task>('api/tasks/' + taskId + '/' + (done ? 'setDone' : 'setUndone'), userId);
  }

  getFilteredUserList(searchString: string) {
    return this.get<User[]>('api/users/filter' + (searchString ? '/' + searchString : ''));
  }

  addAssigneeToTask(user: User, taskId: number) {
    return this.post('api/tasks/' + taskId + '/addAssignee', user);
  }

  removeAssignee(taskID: number, userID: number) {
    return this.post('api/tasks/' + taskID + '/removeAssignee', userID);
  }

  updateTask(task: Task) {
    return this.put('api/tasks', task);
  }

  login(credentials: string) {
    return this.http.post(this.baseUrl + 'api/auth/login', credentials, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    });
  }

  getLoggedInUser() {
    return this.get<User>('api/users/currentlyLoggedIn');
  }
}
