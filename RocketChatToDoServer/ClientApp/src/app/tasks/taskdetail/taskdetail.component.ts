import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-taskdetail',
  templateUrl: './taskdetail.component.html',
  styleUrls: ['./taskdetail.component.css']
})
export class TaskDetailComponent implements OnInit {

  task: Task;
  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private route: ActivatedRoute) {
    const taskId = route.snapshot.paramMap.get('taskId');
    const that = this;
    http.get<Task>(baseUrl + 'api/tasks/' + taskId)
    .subscribe(res => {
      that.task = res;
    }, err => console.error(err));
  }

  ngOnInit() {
  }

}
