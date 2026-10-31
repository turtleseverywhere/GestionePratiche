import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StateService {
  private User: BehaviorSubject<any> = new BehaviorSubject({});
  private Config: BehaviorSubject<any> = new BehaviorSubject({});
  private Language: BehaviorSubject<any> = new BehaviorSubject({});
  private HasNotifications: BehaviorSubject<boolean> = new BehaviorSubject(false);
  private Claims: [] = [];
  private CachedSignalRConnectionId: string = '';

  getClaims() {
    let claims = localStorage.getItem('Claims');
    if (claims != null)
      return JSON.parse(claims);
    return [];
  }
  setClaims(claims: []) {
    this.Claims = claims;
    if (claims)
      localStorage.setItem('Claims', JSON.stringify(claims));
    else
      localStorage.removeItem('Claims');
  }
  getCachedSignalRConnectionId() {
    let CachedSignalRConnectionId = localStorage.getItem('CSRC');
    if (CachedSignalRConnectionId != null)
      return JSON.parse(CachedSignalRConnectionId);
    return '';
  }
  setCachedSignalRConnectionId(connectionId: string) {
    this.CachedSignalRConnectionId = connectionId;
    if (connectionId)
      localStorage.setItem('CSRC', JSON.stringify(connectionId));
    else
      localStorage.removeItem('CSRC');
  }
  getJWTToken() {
    let JWTToken = localStorage.getItem('JWTToken');
    if (JWTToken != null)
      return JWTToken;
    return '';
  }
  setJWTToken(JWTToken: string) {
    if (JWTToken)
      localStorage.setItem('JWTToken', JWTToken);
    else
      localStorage.removeItem('JWTToken');
  }
  getJWTRefreshToken() {
    let JWTRefreshToken = localStorage.getItem('JWTRefreshToken');
    if (JWTRefreshToken != null)
      return JWTRefreshToken;
    return '';
  }
  setJWTRefreshToken(JWTRefreshToken: string) {
    if (JWTRefreshToken)
      localStorage.setItem('JWTRefreshToken', JWTRefreshToken);
    else
      localStorage.removeItem('JWTRefreshToken');
  }
  getUser() {
    let user = localStorage.getItem('User');
    if (user != null) {
      this.User.next(JSON.parse(user));
    }
    else {
      this.User.next(null);
    }
    return this.User;
    //return JSON.parse(localStorage.getItem('User'));
  }
  setUser(user: any) {
    //this.StoredUser = user;
    this.User.next(user);
    if (user)
      localStorage.setItem('User', JSON.stringify(user));
    else
      localStorage.removeItem('User');
  }
  getConfig() {
    let config = localStorage.getItem('Config');
    if (config != null) {
      this.Config.next(JSON.parse(config));
    }
    else {
      this.Config.next(null);
    }
    return this.Config;
  }
  setConfig(config: any) {
    this.Config.next(config);
    if (config)
      localStorage.setItem('Config', JSON.stringify(config));
    else
      localStorage.removeItem('Config');
  }
  getLanguage() {
    let language = localStorage.getItem('Language');
    if (language != null) {
      this.Language.next(language);
    }
    else {
      this.Language.next('en');
    }
    return this.Language;
  }
  setLanguage(language: any) {
    this.Language.next(language);
    if (language)
      localStorage.setItem('Language', language);
    else
      localStorage.removeItem('Language');
  }
  getHasNotifications() {
    return this.HasNotifications;
  }
  setHasNotifications(hasNotifications: boolean) {
    this.HasNotifications.next(hasNotifications);
  }
}
