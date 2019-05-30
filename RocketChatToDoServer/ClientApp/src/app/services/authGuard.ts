import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { JwtHelper } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class AuthGuard implements CanActivate {
    private lastPageBeforeLogin: string;
    getPreviousUrl() {
        return this.lastPageBeforeLogin || '/';
    }
    constructor(private jwtHelper: JwtHelper, private router: Router) {
    }
    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
        const token = localStorage.getItem('jwt');

        if (token && !this.jwtHelper.isTokenExpired(token)) {
            return true;
        }
        this.lastPageBeforeLogin = state.url;
        this.router.navigate(['login']);
        return false;
    }
}
