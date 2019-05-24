import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-taskdetail',
  templateUrl: './taskdetail.component.html',
  styleUrls: ['./taskdetail.component.css']
})
export class TaskDetailComponent {
  task: Task;
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private route: ActivatedRoute) {
    const taskId = Number(route.snapshot.paramMap.get('taskId'));
    this.getTask(taskId);
  }

  private getTask(taskId: number) {
    const that = this;
    this.http.get<Task>(this.baseUrl + 'api/tasks/' + taskId)
      .subscribe(res => {
        that.task = res;
      }, err => console.error(err));
  }

  removeAssignee(assignee: UserTaskMap) {
    const todelete = this.task.assignees.indexOf(assignee);
    if (todelete > -1) {
      this.task.assignees.splice(todelete);
    }
  }
}
