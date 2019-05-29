import { CanActivate, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { JwtHelper } from 'angular2-jwt';

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(private jwtHelper: JwtHelper, private router: Router) {
    }
    canActivate() {
        const token = localStorage.getItem('jwt');

        if (token && !this.jwtHelper.isTokenExpired(token)) {
            return true;
        }
        this.router.navigate(['login']);
        return false;
    }
}
