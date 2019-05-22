import { Component, OnInit, Inject, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css']
})
export class TasksComponent {
  public tasks: Task[];
  public user: User;
  private setTaskDoneId: number;
  private error: string;
  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private route: ActivatedRoute) {
    const userId = route.snapshot.paramMap.get('userId');
    try {
      this.setTaskDoneId = Number(route.snapshot.paramMap.get('taskId'));
    } catch (error) {
      this.error = error.message;
    }

    const that = this;
    http.get<User>(baseUrl + 'api/users/' + userId)
      .subscribe(res => {
        that.user = res;
        that.getDoneTask();
      }, error => console.error(error));
    http.get<Task[]>(baseUrl + 'api/tasks/forUser/' + userId)
      .subscribe(res => {
        that.tasks = res;
        that.getDoneTask();
      }, error => console.error(error));
  }

  getDoneTask() {
    if (this.setTaskDoneId && this.user && this.tasks) {
      this.setTaskToDone(this.setTaskDoneId, true);
    }
  }

  changeTaskActive(taskId: number, event: { checked: boolean }) {
    this.setTaskToDone(taskId, event.checked);
  }

  private setTaskToDone(taskId: number, done: boolean) {
    const donetask = this.tasks.find(t => t.id === taskId);
    if (donetask && donetask.done == done)
      return;

    this.http.get('api/tasks/forUser/' + this.user.id + '/' + (done ? 'setDone' : 'setUndone') + '/' + taskId).subscribe(res => {
      if (donetask) {
        donetask.done = true;
      }
    }, error => console.error(error));
  }
}
