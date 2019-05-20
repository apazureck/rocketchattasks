import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tasks',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css']
})
export class TasksComponent implements OnInit {
  public tasks: Task[];
  public user: User;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private route: ActivatedRoute) {
    const userId = route.snapshot.paramMap.get('userId');
    http.get<User>(baseUrl + 'api/users/' + userId)
      .subscribe(res => this.user = res, error => console.error(error));
    http.get<Task[]>(baseUrl + 'api/tasks/forUser/' + userId)
    .subscribe(res => this.tasks = res, error => console.error(error));
   }

  ngOnInit() {
  }

}
