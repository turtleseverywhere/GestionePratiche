import { Injectable } from '@angular/core';
import { StateService } from '@services/state.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private stateService: StateService,
    private configService: ConfigService,
    private http: HttpClient,
    private router: Router) {
  }
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };
  logIn(username: string, password: string) {
    let body = {
      username: username,
      password: password
    };
    return lastValueFrom(this.http.post<any>(this.configService.getLoginFragment(),
      body,
      this.httpOptions)).then(result => {
        //console.log(result);
        let User = {
          username: username
        }
        this.stateService.setUser(User);
        this.stateService.setJWTToken(result.token);
        this.stateService.setJWTRefreshToken(result.refreshToken);
        this.stateService.setClaims(result.claims);
      });
  }

  logOut() {
    this.stateService.setJWTToken(null);
    this.stateService.setJWTRefreshToken(null);
    this.stateService.setUser(null);
    this.stateService.setClaims([]);
    this.router.navigate(['home']);
  }

  tryRefreshToken() {
    let refreshToken = this.stateService.getJWTRefreshToken();
    let body = {
      refreshToken: refreshToken
    }
    return this.http.post<any>(this.configService.getRefreshTokensFragment(),
      body,
      this.httpOptions);
  }

  checkClaim(claim: string) {
    let claims = this.stateService.getClaims();
    if (claims)
      for (let i = 0; i < claims.length; i++) {
        if ((claims[i]['claimType'] == claim || claims[i]['claimType'] == "CANDOANYTHING") && claims[i]['claimValue'] == "True") {
          return true;
        }
      }
    return false;
  }
}
