import { Component, OnInit, Inject, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { TodobackendService } from '../../services/todobackend.service';

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
  constructor(private http: HttpClient,
    private todobackendService: TodobackendService,
    private route: ActivatedRoute,
    private router: Router) {
    const userId = Number(route.snapshot.paramMap.get('userId')) || Number(localStorage.getItem('userId'));
    try {
      this.setTaskDoneId = Number(route.snapshot.paramMap.get('taskId'));
    } catch (error) {
      this.error = error.message;
    }

    const that = this;
    todobackendService.getUser(userId)
      .subscribe(res => {
        that.user = res;
        that.getDoneTask();
      }, error => console.error(error));
    todobackendService.getTasksForUser(userId)
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

  private async setTaskToDone(taskId: number, done: boolean) {
    try {
      let donetask = await this.todobackendService.getTask(taskId).toPromise();

      if (donetask && donetask.done === done) {
        return;
      }

      donetask = await this.todobackendService.setTaskDone(donetask.id, this.user.id).toPromise();
      if (donetask) {
        const tid = this.tasks.findIndex(x => x.id === donetask.id);
        if (tid > -1) {
          this.tasks[tid] = donetask;
        }
      }
    } catch (error) {
      console.error(error);
    }
  }

  taskSelected(task: Task) {
    this.router.navigate(['tasks/' + task.id]);
  }
}
