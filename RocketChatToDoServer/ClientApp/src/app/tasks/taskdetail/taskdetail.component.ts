import { Component, Inject, ViewChild, ElementRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material';
import { Observable } from 'rxjs/Observable';
import { FormControl } from '@angular/forms';
import { debounceTime, switchMap, catchError } from 'rxjs/operators';
import { TodobackendService } from '../../services/todobackend.service';

@Component({
  selector: 'app-taskdetail',
  templateUrl: './taskdetail.component.html',
  styleUrls: ['./taskdetail.component.css']
})
export class TaskDetailComponent {
  task: Task;
  filteredUsers: User[];
  assigneeCtrl = new FormControl();
  @ViewChild('assigneeInput') assigneeInput: ElementRef;
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
      switchMap(value => this._filter(value)))
    .subscribe(users => that.filteredUsers = users);
  }

  removeAssignee(assignee: UserTaskMap) {
    const todelete = this.task.assignees.indexOf(assignee);
    if (todelete > -1) {
      this.task.assignees.splice(todelete);
    }
  }

  assigneeAutoCompleteSelected(event: MatAutocompleteSelectedEvent) {

  }

  private _filter(input: string) {
    return this.todobackendService.getFilteredUserList(input);
  }
}
