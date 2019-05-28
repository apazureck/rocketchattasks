import { Component, ViewChild, ElementRef, Input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatAutocomplete, MatAutocompleteSelectedEvent, MatSnackBar } from '@angular/material';
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

  constructor(private todobackendService: TodobackendService, route: ActivatedRoute, private snackBar: MatSnackBar) {
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
        that.openSnackbar('Sucessfully removed Assignee');
      }, err => console.error(err));
  }

  assigneeAutoCompleteSelected(event: MatAutocompleteSelectedEvent) {
    const that = this;
    this.todobackendService.addAssigneeToTask((event.option.value as User), this.task.id)
      .pipe(switchMap(value => this.todobackendService.getTask(this.task.id)))
      .subscribe(res => {
        that.task.assignees = res.assignees;
        that.assigneeCtrl.reset();
        that.openSnackbar('Sucessfully added Assignee');
      }, err => console.error(err));
  }

  private _filter(input: string) {
    return this.todobackendService.getFilteredUserList(input);
  }

  private openSnackbar(message: string) {
    this.snackBar.open(message, null, {
      duration: 3000
    });
  }

  updateTask(event) {
    const that = this;
    this.todobackendService.updateTask(this.task).subscribe(res => {
      that.openSnackbar('Task sucessfully updated');
    }, err => console.error(err));
  }

  changeTaskActive(taskId: number, event: { checked: boolean }) {
    this.setTaskToDone(taskId, event.checked);
  }

  private async setTaskToDone(taskId: number, done: boolean) {
    try {
      let donetask = await this.todobackendService.getTask(taskId).toPromise();

      if (donetask && donetask.done === done) {
        return;
      }

      // todo : Set current user here
      donetask = await this.todobackendService.setTaskDone(donetask.id, donetask.initiatorId).toPromise();
    } catch (error) {
      console.error(error);
    }
  }
}
