import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TodobackendService } from '../services/todobackend.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  invalidLogin: boolean;

  constructor(private todoBackendService: TodobackendService, private router: Router) { }

  login(form: NgForm) {
    const credentials = JSON.stringify(form.value);
    this.todoBackendService.login(credentials).subscribe(response => {
      const token = (<any>response).token;
      localStorage.setItem('jwt', token);
      this.invalidLogin = false;
      this.router.navigate(['/']);
    }, err => {
      this.invalidLogin = true;
    });
  }

  logOut() {
    localStorage.removeItem('jwt');
  }
}
