import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '@services/auth.service';

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private router: Router,
    private authService: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot): boolean {
    let claim = route.data['claim'];
    if (claim) {
      let userHasPermission = this.authService.checkClaim(claim);
      if (!userHasPermission) {
        this.router.navigate(['/home']);
        return false;
      }
      return userHasPermission;
    }
    else
      return true;
  }
}
