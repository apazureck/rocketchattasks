import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  public users: User[];
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) { 
    http.get<User[]>(baseUrl + 'api/Users')
    .subscribe(result => {
      this.users = result;
    }, error => console.error(error));
  }

  ngOnInit() {
  }

}
