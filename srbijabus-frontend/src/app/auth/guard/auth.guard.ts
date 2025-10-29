import {ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot} from '@angular/router';
import {inject} from "@angular/core";
import {AuthService} from "../../services/auth.service";

export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  let userRole: string | null = null;
  authService.user$.subscribe(role => userRole = role);

  if (!userRole) {
    router.navigate(['login']);
    return false;
  }

  if (route.data['role'] && !route.data['role'].includes(userRole)) {
    router.navigate(['buslines']);
    return false;
  }

  return true;
};
