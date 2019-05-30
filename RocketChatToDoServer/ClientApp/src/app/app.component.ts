import { Component } from '@angular/core';
import { TodobackendService } from './services/todobackend.service';
import { Router } from '@angular/router';
import { MessengerService } from './services/messenger.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  user: User;
  title = 'Todo List';
  constructor(private router: Router, backendService: TodobackendService, private messenger: MessengerService) {
    const token = localStorage.getItem('jwt');
    if (token) {
      backendService.getLoggedInUser()
        .subscribe(res => {
          localStorage.setItem('userId', res.id.toString());
          this.user = res;
        }, err => console.error(err));
    }
    messenger.subscribeToUserLoggedIn(res => {
      localStorage.setItem('userId', res.id.toString());
      this.user = res;
    });
    messenger.subscribeToUserLoggedOut(() => this.user = undefined);
  }

  logOut() {
    localStorage.removeItem('jwt');
    localStorage.removeItem('userId');
    this.messenger.raiseUserLoggedOut();
    this.router.navigate(['login']);
  }

  goHome() {
    this.router.navigate(['/']);
  }
}
