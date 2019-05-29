import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TodobackendService } from './services/todobackend.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  invalidLogin: boolean;

  constructor(private todoBackendService: TodobackendService, private router: Router) {
  }
  title = 'app';

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
}
