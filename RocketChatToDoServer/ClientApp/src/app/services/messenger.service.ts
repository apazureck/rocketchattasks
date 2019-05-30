import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class MessengerService {

  constructor() { }

  private userLoggedInSubject = new Subject<User>();
  private userLoggedOutSubject = new Subject();

  raiseUserLoggedIn(user: User): void {
    this.userLoggedInSubject.next(user);
  }

  subscribeToUserLoggedIn(callback: (user) => void): void {
    this.userLoggedInSubject.asObservable()
      .subscribe(callback, this.handleError);
  }

  raiseUserLoggedOut(): void {
    this.userLoggedOutSubject.next();
  }

  subscribeToUserLoggedOut(callback: () => void): void {
    this.userLoggedOutSubject.asObservable()
      .subscribe(callback, this.handleError);
  }

  private handleError(err) {
    console.error(err);
  }
}
