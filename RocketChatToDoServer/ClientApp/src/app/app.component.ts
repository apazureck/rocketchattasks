import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TodobackendService } from './services/todobackend.service';
import { Router } from '@angular/router';
import { AuthGuard } from './services/authGuard';
import { MessengerService } from './services/messenger.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  user: User;
  title = 'Todo List';
  constructor(private router: Router, backendService: TodobackendService, messenger: MessengerService) {
    const token = localStorage.getItem('jwt');
    if (token) {
      backendService.getLoggedInUser()
        .subscribe(res => {
          this.user = res;
        }, err => console.error(err));
    }
    messenger.subscribeToUserLoggedIn(res => {
      this.user = res;
    });
  }

  logOut() {
    localStorage.removeItem('jwt');
  }
}
