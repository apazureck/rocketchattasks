import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { JwtHelper } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import { TodobackendService } from './todobackend.service';
import { MessengerService } from './messenger.service';

@Injectable()
export class AuthGuard implements CanActivate {
    private lastPageBeforeLogin: string;
    private firstlogin = true;

    getPreviousUrl() {
        return this.lastPageBeforeLogin || '/';
    }
    constructor(private jwtHelper: JwtHelper,
        private router: Router,
        private backendService: TodobackendService,
        private messenger: MessengerService) {
    }
    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
        const token = localStorage.getItem('jwt');
        if (token && !this.jwtHelper.isTokenExpired(token)) {
            if (this.firstlogin) {
                this.backendService.getLoggedInUser().subscribe(x => this.messenger.raiseUserLoggedIn(x));
            }
            return true;
        }
        this.lastPageBeforeLogin = state.url;
        this.router.navigate(['login']);
        return false;
    }
}
