import { Component, ViewChild, ElementRef, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material';
import { FormControl } from '@angular/forms';
import { debounceTime, switchMap, catchError, tap, finalize } from 'rxjs/operators';
import { TodobackendService } from '../../services/todobackend.service';

@Component({
  selector: 'app-taskdetail',
  templateUrl: './taskdetail.component.html',
  styleUrls: ['./taskdetail.component.css']
})
export class TaskDetailComponent {
  task: Task;
  filteredUsers: User[] = [];
  assigneeCtrl = new FormControl();
  isLoading = false;
  @ViewChild('assigneeInput') assigneeInput: Input;
  @ViewChild('auto') matAutocomplete: MatAutocomplete;

  constructor(private todobackendService: TodobackendService, route: ActivatedRoute) {
    const taskId = Number(route.snapshot.paramMap.get('taskId'));

    const that = this;

    todobackendService.getTask(taskId).subscribe(res => {
      that.task = res;
    }, err => console.error(err));

    this.assigneeCtrl.valueChanges
      .pipe(
        debounceTime(300),
        tap(() => that.isLoading = true),
        switchMap(value => that._filter(value).pipe(
          finalize(() => this.isLoading = false),
        )))
      .subscribe(users => {
        for (const a of that.task.assignees) {
          const i = users.findIndex(u => u.id === a.userID);
          if (i > -1) {
            users.splice(i);
          }
        }
        that.filteredUsers = users;
      }, err => console.error(err));
  }

  removeAssignee(assignee: UserTaskMap) {
    const that = this;
    this.todobackendService.removeAssignee(assignee.taskID, assignee.userID)
      .subscribe(res => {
        const idel = that.task.assignees.indexOf(assignee);
        if (idel > -1) {
          that.task.assignees.splice(idel, 1);
        }
      }, err => console.error(err));
  }

  assigneeAutoCompleteSelected(event: MatAutocompleteSelectedEvent) {
    const that = this;
    this.todobackendService.addAssigneeToTask((event.option.value as User), this.task.id)
      .pipe(switchMap(value => this.todobackendService.getTask(this.task.id)))
      .subscribe(res => {
        that.task.assignees = res.assignees;
        that.assigneeCtrl.reset();
      }, err => console.error(err));
  }

  private _filter(input: string) {
    return this.todobackendService.getFilteredUserList(input);
  }
}
