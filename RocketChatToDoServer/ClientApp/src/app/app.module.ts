import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatButtonModule,
  MatCheckboxModule,
  MatInputModule,
  MatDatepickerModule,
  MatFormFieldModule,
  MatNativeDateModule,
  MatChipsModule,
  MatIconModule,
  MatAutocompleteModule,
  MatProgressSpinnerModule,
  MatSnackBarModule,
  MatProgressBarModule,
  MatToolbarModule
} from '@angular/material';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { UsersComponent } from './users/users.component';
import { TasksComponent } from './users/tasks/tasks.component';
import { TaskDetailComponent } from './tasks/taskdetail/taskdetail.component';
import { TodobackendService } from './services/todobackend.service';
import { LoginComponent } from './login/login.component';
import { AuthGuard } from './services/authGuard';
import { JwtHelper } from 'angular2-jwt';
import { MessengerService } from './services/messenger.service';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    UsersComponent,
    TasksComponent,
    TaskDetailComponent,
    LoginComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    MatButtonModule, MatCheckboxModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatNativeDateModule,
    MatChipsModule,
    MatIconModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatToolbarModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: TasksComponent, canActivate: [AuthGuard], pathMatch: 'full' },
      { path: 'login', component: LoginComponent },
      { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
      { path: 'users/:userId', component: TasksComponent, canActivate: [AuthGuard] },
      { path: 'users/:userId/:action/:taskId', component: TasksComponent, canActivate: [AuthGuard] },
      { path: 'tasks/:taskId', component: TaskDetailComponent, canActivate: [AuthGuard] }
    ])
  ],
  providers: [ MatDatepickerModule, TodobackendService, AuthGuard, JwtHelper, MessengerService ],
  bootstrap: [AppComponent]
})
export class AppModule { }
