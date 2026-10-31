import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpHeaders, HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { ConfigService } from '@services/config.service';
import { AuthService } from '@services/auth.service';
import { StateService } from '@services/state.service';

@Injectable()
export class Interceptor implements HttpInterceptor {
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };
  constructor(private http: HttpClient,
    private readonly configService: ConfigService,
    private stateService: StateService,
    private authService: AuthService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    var updatedRequest = request;
    if (!/^(http|https):/i.test(request.url) && !request.url.startsWith('/assets/')) {
      if (!request.url.endsWith(this.configService.getRefreshTokensFragment())) {
        //console.log("regular token");
        updatedRequest = this.cloneRequest(request, this.stateService.getJWTToken());
      }
      else {
        //console.log("refresh token !");
        updatedRequest = this.cloneRequest(request, this.stateService.getJWTRefreshToken());
      }
    }

    return next.handle(updatedRequest).pipe(catchError(error => {
      let backendUrl = this.configService.getBackendUrl();
      let loginUrl = backendUrl + this.configService.getLoginFragment();
      let refreshTokensUrl = backendUrl + this.configService.getRefreshTokensFragment();
      /*nel caso si abbia un unauthorized, si cerca di refreshare il token nel caso sia scaduto.*/
      if (error.url != loginUrl
        && error.url != refreshTokensUrl
        && error.status === 401) {
        //console.log("refreshing token..", updatedRequest);
        return this.authService.tryRefreshToken().pipe(
          tap((result: any) => {
            if (result != undefined) {
              if (result.token != undefined)
                this.stateService.setJWTToken(result.token);
              if (result.refreshToken != undefined)
                this.stateService.setJWTRefreshToken(result.refreshToken);
              if (result.claims != undefined)
                this.stateService.setClaims(result.claims);
            }
          }),
          switchMap(() => {
            //console.log("token refreshed !");
            return next.handle(this.cloneRequest(request, this.stateService.getJWTToken()))
          })
        );
      }
      return next.handle(updatedRequest).pipe(catchError(error => {
        if (error.status === 401) {
          this.authService.logOut();
        }
        return throwError(() => error);
      }));
    }));
  }

  cloneRequest(request: HttpRequest<any>, token: any): HttpRequest<any> {
    let updatedRequest = request.clone({
      setHeaders: {
        'Authorization': `Bearer ${token}`,
      },
      withCredentials: true,
      url: this.configService.getBackendUrl() + request.url
    });
    return updatedRequest;
  }
}
