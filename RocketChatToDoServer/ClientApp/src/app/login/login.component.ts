import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TodobackendService } from '../services/todobackend.service';
import { Router } from '@angular/router';
import { AuthGuard } from '../services/authGuard';
import { MessengerService } from '../services/messenger.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  invalidLogin: boolean;

  constructor(private todoBackendService: TodobackendService,
    private router: Router,
    private authGuard: AuthGuard,
    private messenger: MessengerService) {
   }

  login(form: NgForm) {
    const credentials = JSON.stringify(form.value);
    this.todoBackendService.login(credentials).subscribe(response => {
      const token = (<any>response).token;
      const user = (<any>response).user;
      localStorage.setItem('jwt', token);
      this.invalidLogin = false;
      this.messenger.raiseUserLoggedIn(user);
      this.router.navigate([this.authGuard.getPreviousUrl()]);
    }, err => {
      this.invalidLogin = true;
    });
  }

  logOut() {
    localStorage.removeItem('jwt');
  }
}
